import fs from "node:fs";
import path from "node:path";
import { REPO_ROOT } from "./config.mjs";
import { CONTROLLED_FAILURE_STAGE } from "./verify.mjs";

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
  baseBranch,
  repoKind,
  maxIterations,
}) {
  return `You are implementing a GitHub Issue in an autonomous AI development loop.

## Issue
- Number: #${issueNumber}
- Title: ${title}
- Base branch: ${baseBranch}
- Repo kind: ${repoKind}

## Issue body
${body || "(empty)"}

## Hard rules
1. Do NOT commit or push to \`${baseBranch}\` / main.
2. Cursor will create a working branch for you (typically \`cursor/...\`). Commit and push only on that agent branch.
3. Follow AGENTS.md. Do not delete/disable/weaken tests. Do not commit secrets.
4. Prefer minimal diffs that satisfy Acceptance Criteria.
5. After implementing, you may run targeted tests, but the outer orchestrator will run \`./scripts/verify\`.
6. Maximum outer iterations: ${maxIterations}. Be correct on the first try when possible.
7. Do not merge the PR.

## AGENTS.md (excerpt)
${readAgentsExcerpt()}

## Required outcome for this turn
Implement the issue requirements, commit, and push on the Cursor-created working branch.
Summarize what you changed at the end.
`;
}

export function buildFixPrompt({
  iteration,
  maxIterations,
  activeBranch,
  failedCommand,
  exitCode,
  failedStage,
  excerpt,
  previousAttempts,
  previousAction,
  changedFiles,
  diffStat,
  diffPatch,
  fingerprint,
  sameFingerprintCount,
}) {
  const controlled =
    failedStage === CONTROLLED_FAILURE_STAGE ||
    String(excerpt || "").includes(CONTROLLED_FAILURE_STAGE);

  return `Verification FAILED. Perform root-cause analysis and fix. Do NOT repeat the same failing change.
Do NOT start a new task or create a new branch. Continue on the same working branch with the same agent session.

## Iteration
${iteration} / ${maxIterations}
Working branch: \`${activeBranch || "(unknown)"}\`
Previous action: ${previousAction || "implement / prior fix turn"}
Failure fingerprint: ${fingerprint} (seen ${sameFingerprintCount} time(s))
${controlled ? `\n## Controlled failure marker\n\`${CONTROLLED_FAILURE_STAGE}\`\nThis failure was injected by the orchestrator harness to prove self-correction. It is NOT caused by your code. Acknowledge it, keep your implementation, stay on the same branch, and continue so the next outer verify can run for real.\n` : ""}

## Failed verification
- Command: ${failedCommand}
- Exit code: ${exitCode}
- Failed stage (if known): ${failedStage || "unknown"}
${controlled ? `- Controlled failure: YES (${CONTROLLED_FAILURE_STAGE})` : "- Controlled failure: no"}

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
1. Identify root cause (not just symptoms). If marker is \`${CONTROLLED_FAILURE_STAGE}\`, treat as harness proof — do not revert good work.
2. Apply a different fix than prior attempts if fingerprint repeats (skip for controlled harness).
3. Commit and push to the **same** working branch${activeBranch ? ` (\`${activeBranch}\`)` : ""} only if a real code fix is needed.
4. Do not weaken or delete tests to pass.
5. Do not touch secrets or main.
6. Do not open a new agent session or new branch.
`;
}
