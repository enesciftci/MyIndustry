import { spawnSync } from "node:child_process";
import { REPO_ROOT } from "./config.mjs";

export function buildPrBody({
  title,
  issueNumber,
  implementation,
  verification,
  iterations,
  failedAttempts,
  risks = "See review",
  databaseChanges = "None",
  breakingChanges = "None",
  agentId,
  siblingPrUrl,
  loopResult,
}) {
  return `## Summary

${title}

Closes #${issueNumber}

## Implementation

${implementation || "(see commits)"}

## Tests

- Covered by \`./scripts/verify\` (unit/integration/smoke as applicable)

## Verification

\`\`\`text
${verification || "(see workflow logs)"}
\`\`\`

## Iterations

${iterations}

## Failed Attempts

${failedAttempts || "None"}

## Loop Result

${loopResult || "(see Iterations / Verification above)"}

## Risks

${risks}

## Database Changes

${databaseChanges}

## Breaking Changes

${breakingChanges}

## AI-generated changes

- Yes — autonomous AI loop (Cursor Cloud Agent + GHA orchestrator)
${agentId ? `- Agent ID: \`${agentId}\`` : ""}
${siblingPrUrl ? `\n## Related PR\n\n${siblingPrUrl}\n` : ""}

## Human review checklist

- [ ] Scope matches the issue
- [ ] No secrets committed
- [ ] Tests were not deleted/disabled/weakened
- [ ] CI green
- [ ] Ready to merge (human only — do not auto-merge)
`;
}

function ghTokenEnv() {
  // Prefer a PAT: default GITHUB_TOKEN cannot create PRs unless the repo
  // setting "Allow GitHub Actions to create and approve pull requests" is on.
  const token =
    process.env.AI_LOOP_GH_TOKEN ||
    process.env.GH_TOKEN ||
    process.env.GITHUB_TOKEN;
  if (!token) return process.env;
  return { ...process.env, GH_TOKEN: token, GITHUB_TOKEN: token };
}

export function isPrPermissionError(message) {
  const m = String(message || "").toLowerCase();
  return (
    m.includes("not permitted to create or approve pull requests") ||
    m.includes("resource not accessible by integration") ||
    m.includes("github actions is not permitted")
  );
}

export function createPullRequest({
  title,
  body,
  head,
  base,
  dryRun = false,
}) {
  if (dryRun || process.env.AI_LOOP_SKIP_PR === "1") {
    return { url: null, dryRun: true, body };
  }
  const r = spawnSync(
    "gh",
    [
      "pr",
      "create",
      "--title",
      title,
      "--body",
      body,
      "--base",
      base,
      "--head",
      head,
    ],
    { cwd: REPO_ROOT, encoding: "utf8", env: ghTokenEnv() },
  );
  if (r.status !== 0) {
    const detail = (r.stderr || r.stdout || "").trim();
    const err = new Error(`gh pr create failed: ${detail}`);
    err.code = isPrPermissionError(detail) ? "PR_PERMISSION" : "PR_CREATE";
    err.detail = detail;
    throw err;
  }
  const url = (r.stdout || "").trim().split("\n").filter(Boolean).pop();
  return { url, dryRun: false, body };
}
