import { spawnSync } from "node:child_process";
import { REPO_ROOT } from "./config.mjs";

function gh(args) {
  const r = spawnSync("gh", args, {
    cwd: REPO_ROOT,
    encoding: "utf8",
    env: process.env,
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
