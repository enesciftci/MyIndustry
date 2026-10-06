import fs from "node:fs";
import path from "node:path";
import { REPO_ROOT } from "./config.mjs";

function readAgentsExcerpt(max = 6000) {
  const p = path.join(REPO_ROOT, "AGENTS.md");
  if (!fs.existsSync(p)) return "(AGENTS.md missing)";
  const t = fs.readFileSync(p, "utf8");
  return t.length > max ? `${t.slice(0, max)}\n...[truncated]` : t;
}

export function buildInitialPrompt({
  issueNumber,
  title,
  body,
  branch,
  baseBranch,
  repoKind,
  maxIterations,
}) {
  return `You are implementing a GitHub Issue in an autonomous AI development loop.

## Issue
- Number: #${issueNumber}
- Title: ${title}
- Branch (already created; work ONLY on this branch): ${branch}
- Base branch: ${baseBranch}
- Repo kind: ${repoKind}

## Issue body
${body || "(empty)"}

## Hard rules
1. Do NOT commit or push to ${baseBranch}/main.
2. Work only on branch \`${branch}\`. Commit and push your changes to that branch.
3. Follow AGENTS.md. Do not delete/disable/weaken tests. Do not commit secrets.
4. Prefer minimal diffs that satisfy Acceptance Criteria.
5. After implementing, you may run targeted tests, but the outer orchestrator will run \`./scripts/verify\`.
6. Maximum outer iterations: ${maxIterations}. Be correct on the first try when possible.
7. Do not merge the PR.

## AGENTS.md (excerpt)
${readAgentsExcerpt()}

## Required outcome for this turn
Implement the issue requirements, commit, and push to \`${branch}\`.
Summarize what you changed at the end.
`;
}

export function buildFixPrompt({
  iteration,
  maxIterations,
  failedCommand,
  exitCode,
  failedStage,
  excerpt,
  previousAttempts,
  changedFiles,
  diffStat,
  diffPatch,
  fingerprint,
  sameFingerprintCount,
}) {
  return `Verification FAILED. Perform root-cause analysis and fix. Do NOT repeat the same failing change.

## Iteration
${iteration} / ${maxIterations}
Failure fingerprint: ${fingerprint} (seen ${sameFingerprintCount} time(s))

## Failed verification
- Command: ${failedCommand}
- Exit code: ${exitCode}
- Failed stage (if known): ${failedStage || "unknown"}

## Error output (excerpt)
\`\`\`
${excerpt}
\`\`\`

## Previous attempts
${previousAttempts || "(none)"}

## Changed files vs base
${(changedFiles || []).join("\n") || "(none)"}

## Diff stat
${diffStat || "(none)"}

## Diff (truncated)
\`\`\`diff
${diffPatch || "(none)"}
\`\`\`

## Required outcome
1. Identify root cause (not just symptoms).
2. Apply a different fix than prior attempts if fingerprint repeats.
3. Commit and push to the same feature branch.
4. Do not weaken or delete tests to pass.
5. Do not touch secrets or main.
`;
}
