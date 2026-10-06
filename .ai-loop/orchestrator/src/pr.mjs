import { spawnSync } from "node:child_process";
import { REPO_ROOT } from "./config.mjs";
import { CONTROLLED_FAILURE_STAGE } from "./verify.mjs";

export function buildPrBody({
  title,
  issueNumber,
  implementation,
  verification,
  iterationDetails = [],
  failedAttempts,
  risks = "See review",
  databaseChanges = "None",
  breakingChanges = "None",
  agentId,
  branch,
  siblingPrUrl,
  prCi,
}) {
  const detailBlocks =
    iterationDetails.length > 0
      ? iterationDetails
          .map((it) => {
            const lines = [
              `### Iteration ${it.iteration}`,
              `Status: ${it.status}`,
            ];
            if (it.reason) lines.push(`Reason: ${it.reason}`);
            return lines.join("\n");
          })
          .join("\n\n")
      : "(see attempts in state)";

  const ciMode = prCi?.mode || "unavailable";
  const ciLines = [
    `Mode: \`${ciMode}\``,
    prCi?.url ? `URL: ${prCi.url}` : null,
    prCi?.note || null,
    ciMode === "unavailable"
      ? "GitHub PR CI unavailable — do not treat agent verify PASS as merge approval. Enable required status checks on `main` and set `AI_LOOP_GH_TOKEN` so `pull_request` CI can run."
      : null,
  ]
    .filter(Boolean)
    .join("\n");

  return `## Summary

${title}

Closes #${issueNumber}

## Loop Result

Iterations: ${iterationDetails.length || "(see below)"}

${detailBlocks}

### Agent
Agent ID: \`${agentId || "n/a"}\`

### Branch
\`${branch || "n/a"}\`

### Verification
(Agent / pre-PR quality gate — not a substitute for GitHub CI)

\`\`\`text
${verification || "(see workflow logs)"}
\`\`\`

### Independent GitHub CI
(Merge gate — independent of agent verify)

${ciLines}

### Human Review
Required

## Implementation

${implementation || "(see commits)"}

## Tests

- Covered by \`./scripts/verify\` (unit/integration/smoke as applicable)
- Independent PR CI must also pass before merge

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
- [ ] Independent GitHub CI green
- [ ] Ready to merge (human only — do not auto-merge)
`;
}

export function buildIterationDetailsFromState(state, finalIteration, controlled) {
  const attempts = state?.attempts || [];
  const details = [];
  for (const a of attempts) {
    const n = details.length + 1;
    if (a.status === "failed") {
      const reason =
        controlled && n === 1
          ? CONTROLLED_FAILURE_STAGE
          : a.failure || "verify failed";
      details.push({ iteration: n, status: "FAILED", reason });
    } else if (a.status === "passed") {
      details.push({ iteration: n, status: "PASSED" });
    }
  }
  if (
    details.length === 0 &&
    finalIteration &&
    state?.final_status === "passed"
  ) {
    details.push({ iteration: finalIteration, status: "PASSED" });
  }
  return details;
}

function ghTokenEnv() {
  // Prefer a PAT: default GITHUB_TOKEN cannot create PRs unless the repo
  // setting "Allow GitHub Actions to create and approve pull requests" is on.
  // Even when PR create works, default GITHUB_TOKEN does not trigger other workflows.
  const token =
    process.env.AI_LOOP_GH_TOKEN ||
    process.env.GH_TOKEN ||
    process.env.GITHUB_TOKEN;
  if (!token) return process.env;
  return { ...process.env, GH_TOKEN: token, GITHUB_TOKEN: token };
}

export function usingDefaultActionsToken() {
  return (
    process.env.GITHUB_ACTIONS === "true" &&
    !process.env.AI_LOOP_GH_TOKEN
  );
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
  if (usingDefaultActionsToken()) {
    console.warn(
      "[ai-loop] WARNING: AI_LOOP_GH_TOKEN unset in Actions. " +
        "PR may be created with default GITHUB_TOKEN; pull_request CI often will NOT start.",
    );
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

export function updatePullRequestBody(prUrlOrNumber, body) {
  const id = String(prUrlOrNumber || "").includes("/")
    ? String(prUrlOrNumber).replace(/\/$/, "").split("/").pop()
    : String(prUrlOrNumber);
  if (!id) return { ok: false };
  const r = spawnSync("gh", ["pr", "edit", id, "--body", body], {
    cwd: REPO_ROOT,
    encoding: "utf8",
    env: ghTokenEnv(),
  });
  return { ok: r.status === 0, stderr: r.stderr, stdout: r.stdout };
}
