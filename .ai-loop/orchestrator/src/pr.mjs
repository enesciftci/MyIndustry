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
    { cwd: REPO_ROOT, encoding: "utf8" },
  );
  if (r.status !== 0) {
    throw new Error(`gh pr create failed: ${r.stderr || r.stdout}`);
  }
  const url = (r.stdout || "").trim().split("\n").filter(Boolean).pop();
  return { url, dryRun: false, body };
}
