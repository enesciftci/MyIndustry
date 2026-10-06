import { spawnSync } from "node:child_process";
import { REPO_ROOT } from "./config.mjs";

function ghTokenEnv() {
  const token =
    process.env.AI_LOOP_GH_TOKEN ||
    process.env.GH_TOKEN ||
    process.env.GITHUB_TOKEN;
  if (!token) return process.env;
  return { ...process.env, GH_TOKEN: token, GITHUB_TOKEN: token };
}

function gh(args) {
  const r = spawnSync("gh", args, {
    cwd: REPO_ROOT,
    encoding: "utf8",
    env: ghTokenEnv(),
  });
  return {
    code: r.status ?? 1,
    stdout: (r.stdout || "").trim(),
    stderr: (r.stderr || "").trim(),
  };
}

export function fetchIssue(issueNumber) {
  const r = gh([
    "issue",
    "view",
    String(issueNumber),
    "--json",
    "number,title,body,labels,state",
  ]);
  if (r.code !== 0) {
    throw new Error(`gh issue view failed: ${r.stderr || r.stdout}`);
  }
  return JSON.parse(r.stdout);
}

export function replaceLabels(issueNumber, { remove = [], add = [] }) {
  for (const name of remove) {
    gh(["issue", "edit", String(issueNumber), "--remove-label", name]);
  }
  for (const name of add) {
    gh(["issue", "edit", String(issueNumber), "--add-label", name]);
  }
}

export function commentIssue(issueNumber, body) {
  // Never include secrets in body (caller responsibility).
  const r = gh(["issue", "comment", String(issueNumber), "--body", body]);
  if (r.code !== 0) {
    console.error("issue comment failed:", r.stderr || r.stdout);
  }
}

export function triggerSiblingWorkflow({
  uiRepo,
  issueNumber,
  tokenEnv = "AI_LOOP_GH_TOKEN",
}) {
  const token = process.env[tokenEnv];
  if (!token || !uiRepo) {
    return {
      ok: false,
      reason:
        "MANUAL CONFIGURATION REQUIRED: set AI_LOOP_UI_REPO and AI_LOOP_GH_TOKEN for cross-repo dispatch",
    };
  }
  const r = spawnSync(
    "gh",
    [
      "workflow",
      "run",
      "ai-loop.yml",
      "--repo",
      uiRepo,
      "-f",
      `issue_number=${issueNumber}`,
    ],
    {
      encoding: "utf8",
      env: { ...process.env, GH_TOKEN: token, GITHUB_TOKEN: token },
    },
  );
  return {
    ok: r.status === 0,
    stdout: r.stdout,
    stderr: r.stderr,
  };
}

function sleepMs(ms) {
  spawnSync("node", ["-e", `Atomics.wait(new Int32Array(new SharedArrayBuffer(4)),0,0,${ms})`], {
    encoding: "utf8",
  });
}

/**
 * Ensure independent PR CI starts after gh pr create.
 * Default GITHUB_TOKEN-created PRs often get no pull_request workflows;
 * fall back to workflow_dispatch on the head branch.
 */
export function ensurePrCi({
  prUrl,
  headBranch,
  repoKind = "backend",
  pollAttempts = 6,
  pollDelayMs = 5000,
}) {
  const workflowFile = "tests.yml";
  const checkNeedle =
    repoKind === "frontend" ? "Frontend Tests" : "Run Tests";
  const prId = prUrl
    ? String(prUrl).replace(/\/$/, "").split("/").pop()
    : null;

  if (!prId && !headBranch) {
    return {
      triggered: false,
      mode: "unavailable",
      url: null,
      note: "No PR URL or head branch to attach CI.",
    };
  }

  for (let i = 0; i < pollAttempts; i++) {
    if (prId) {
      const checks = gh(["pr", "checks", prId, "--json", "name,state,link"]);
      if (checks.code === 0 && checks.stdout) {
        try {
          const list = JSON.parse(checks.stdout);
          const hit = (list || []).find(
            (c) =>
              String(c.name || "").includes(checkNeedle) ||
              String(c.name || "").toLowerCase().includes("test"),
          );
          if (hit) {
            return {
              triggered: true,
              mode: "pull_request",
              url: hit.link || null,
              note: `Detected check \`${hit.name}\` (state=${hit.state}).`,
            };
          }
        } catch {
          /* continue */
        }
      }
    }
    // Also look at recent workflow runs on the head branch
    const runs = gh([
      "run",
      "list",
      "--workflow",
      workflowFile,
      "--branch",
      headBranch,
      "--limit",
      "3",
      "--json",
      "databaseId,url,status,event,displayTitle",
    ]);
    if (runs.code === 0 && runs.stdout) {
      try {
        const list = JSON.parse(runs.stdout);
        const recent = (list || []).find(
          (r) => r.event === "pull_request" || r.event === "workflow_dispatch",
        );
        if (recent) {
          return {
            triggered: true,
            mode: recent.event === "pull_request" ? "pull_request" : "workflow_dispatch",
            url: recent.url || null,
            note: `Found workflow run (${recent.event}).`,
          };
        }
      } catch {
        /* continue */
      }
    }
    if (i < pollAttempts - 1) sleepMs(pollDelayMs);
  }

  // Fallback: explicit dispatch on head ref
  const dispatch = gh([
    "workflow",
    "run",
    workflowFile,
    "--ref",
    headBranch,
  ]);
  if (dispatch.code !== 0) {
    return {
      triggered: false,
      mode: "unavailable",
      url: null,
      note:
        `Could not start CI via pull_request or workflow_dispatch: ${dispatch.stderr || dispatch.stdout}. ` +
        "Set AI_LOOP_GH_TOKEN (PAT) so pull_request events trigger tests.yml.",
    };
  }

  sleepMs(3000);
  const after = gh([
    "run",
    "list",
    "--workflow",
    workflowFile,
    "--branch",
    headBranch,
    "--limit",
    "1",
    "--json",
    "url,status,event",
  ]);
  let url = null;
  if (after.code === 0 && after.stdout) {
    try {
      url = JSON.parse(after.stdout)?.[0]?.url || null;
    } catch {
      /* ignore */
    }
  }
  return {
    triggered: true,
    mode: "workflow_dispatch",
    url,
    note:
      "pull_request CI was not detected; dispatched tests.yml on head branch as fallback.",
  };
}
