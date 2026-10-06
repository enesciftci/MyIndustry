import { spawnSync } from "node:child_process";
import { env, REPO_ROOT } from "./config.mjs";

/**
 * Pick the agent-created branch from a Cloud RunResult.
 * Prefer an entry matching repoUrl; otherwise first branch with a name.
 */
export function extractBranchFromResult(result, repoUrl) {
  const branches = result?.git?.branches;
  if (!Array.isArray(branches) || branches.length === 0) return null;
  const normalized = String(repoUrl || "").replace(/\.git$/, "").toLowerCase();
  const match = branches.find((b) => {
    if (!b?.branch) return false;
    const u = String(b.repoUrl || "")
      .replace(/\.git$/, "")
      .toLowerCase();
    return !normalized || !u || u === normalized || u.endsWith(normalized.replace(/^https?:\/\/github\.com\//, ""));
  });
  if (match?.branch) return match.branch;
  const first = branches.find((b) => b?.branch);
  return first?.branch || null;
}

/**
 * Cloud agent wrapper. When AI_LOOP_MOCK_AGENT=1, uses local mock that
 * creates a cursor/mock-* branch and the smoke marker file.
 */
export async function createAgentDriver({
  apiKey,
  repoUrl,
  baseBranch = "main",
  issueNumber,
  modelId = "composer-2.5",
}) {
  if (env("AI_LOOP_MOCK_AGENT") === "1") {
    return createMockAgent({ issueNumber, baseBranch });
  }

  const { Agent, CursorAgentError } = await import("@cursor/sdk");
  const agent = await Agent.create({
    apiKey,
    model: { id: modelId },
    cloud: {
      repos: [{ url: repoUrl, startingRef: baseBranch }],
      workOnCurrentBranch: false,
      autoCreatePR: false,
      skipReviewerRequest: true,
    },
  });

  return {
    kind: "cloud",
    agentId: agent.agentId || agent.agent_id || null,
    CursorAgentError,
    async send(prompt) {
      const run = await agent.send(prompt);
      const result = await run.wait();
      const branch = extractBranchFromResult(result, repoUrl);
      return {
        runId: result.id,
        status: result.status,
        result,
        branch,
      };
    },
    async dispose() {
      if (typeof agent[Symbol.asyncDispose] === "function") {
        await agent[Symbol.asyncDispose]();
      } else if (typeof agent.close === "function") {
        await agent.close();
      }
    },
  };
}

function createMockAgent({ issueNumber, baseBranch }) {
  let calls = 0;
  const mockBranch = `cursor/mock-issue-${issueNumber || "0"}`;
  let branchReady = false;

  function ensureMockBranchLocal() {
    if (branchReady) return;
    spawnSync("git", ["fetch", "origin", baseBranch], { cwd: REPO_ROOT });
    const baseRef =
      spawnSync("git", ["rev-parse", "--verify", `origin/${baseBranch}`], {
        cwd: REPO_ROOT,
      }).status === 0
        ? `origin/${baseBranch}`
        : baseBranch;
    spawnSync("git", ["checkout", "-B", mockBranch, baseRef], { cwd: REPO_ROOT });
    branchReady = true;
  }

  return {
    kind: "mock",
    agentId: "mock-agent",
    CursorAgentError: class extends Error {},
    async send(prompt) {
      calls += 1;
      const fs = await import("node:fs");
      const path = await import("node:path");

      ensureMockBranchLocal();

      const passFirst = process.env.AI_LOOP_MOCK_PASS_FIRST === "1";
      const marker = path.join(REPO_ROOT, "docs", "ai-loop-smoke-marker.md");

      // Iteration 1: no marker (self-correct path). Later / fix prompts: add marker.
      if (passFirst || calls >= 2 || prompt.includes("Verification FAILED")) {
        fs.mkdirSync(path.dirname(marker), { recursive: true });
        fs.writeFileSync(
          marker,
          "# AI Loop Smoke Marker\n\nCreated by mock autonomous loop for verification.\n",
        );
        spawnSync("git", ["add", "docs/ai-loop-smoke-marker.md"], { cwd: REPO_ROOT });
        spawnSync(
          "git",
          ["commit", "-m", "chore: add AI loop smoke marker (mock agent)"],
          { cwd: REPO_ROOT },
        );
      }

      return {
        runId: `mock-run-${calls}`,
        status: "finished",
        result: {
          status: "finished",
          git: { branches: [{ repoUrl: "mock", branch: mockBranch }] },
        },
        branch: mockBranch,
      };
    },
    async dispose() {},
  };
}
