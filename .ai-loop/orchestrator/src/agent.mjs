import { env } from "./config.mjs";

/**
 * Cloud agent wrapper. When AI_LOOP_MOCK_AGENT=1, uses local mock that
 * creates the smoke marker file (for offline E2E of the outer loop).
 */
export async function createAgentDriver({
  apiKey,
  repoUrl,
  branch,
  modelId = "composer-2.5",
}) {
  if (env("AI_LOOP_MOCK_AGENT") === "1") {
    return createMockAgent();
  }

  const { Agent, CursorAgentError } = await import("@cursor/sdk");
  const agent = await Agent.create({
    apiKey,
    model: { id: modelId },
    cloud: {
      repos: [{ url: repoUrl, startingRef: branch }],
      workOnCurrentBranch: true,
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
      return {
        runId: result.id,
        status: result.status,
        result,
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

function createMockAgent() {
  let calls = 0;
  return {
    kind: "mock",
    agentId: "mock-agent",
    CursorAgentError: class extends Error {},
    async send(prompt) {
      calls += 1;
      const { spawnSync } = await import("node:child_process");
      const fs = await import("node:fs");
      const path = await import("node:path");
      const { REPO_ROOT } = await import("./config.mjs");

      // Iteration 1: deliberately do nothing useful if smoke expects self-correct,
      // or create marker immediately when AI_LOOP_MOCK_PASS_FIRST=1
      const passFirst = process.env.AI_LOOP_MOCK_PASS_FIRST === "1";
      const marker = path.join(REPO_ROOT, "docs", "ai-loop-smoke-marker.md");

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

      return { runId: `mock-run-${calls}`, status: "finished", result: { status: "finished" } };
    },
    async dispose() {},
  };
}
