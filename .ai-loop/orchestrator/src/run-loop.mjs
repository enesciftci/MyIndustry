#!/usr/bin/env node
/**
 * Autonomous AI Loop orchestrator.
 * Outer loop: Cloud Agent (or mock) → ./scripts/verify → fix → PR
 *
 * Cloud path: Agent starts from main (workOnCurrentBranch=false);
 * Cursor creates cursor/... branch; we discover it from result.git.branches.
 *
 * Never prints CURSOR_API_KEY or other secrets.
 */
import fs from "node:fs";
import path from "node:path";
import {
  REPO_ROOT,
  assertApiKeyPresent,
  env,
  loadConfigEnv,
} from "./config.mjs";
import { createAgentDriver } from "./agent.mjs";
import {
  changedFilesVsBase,
  currentDiffPatch,
  diffStat,
  syncBranch,
} from "./git.mjs";
import {
  commentIssue,
  ensurePrCi,
  fetchIssue,
  replaceLabels,
  triggerSiblingWorkflow,
} from "./github.mjs";
import { buildFixPrompt, buildInitialPrompt } from "./prompt.mjs";
import {
  buildIterationDetailsFromState,
  buildPrBody,
  createPullRequest,
  isPrPermissionError,
  updatePullRequestBody,
} from "./pr.mjs";
import { resolveScope, shouldRunInThisRepo } from "./scope.mjs";
import {
  copyStateArtifact,
  fingerprintFailure,
  initState,
  patchState,
  readState,
  recordState,
  taskIdForIssue,
} from "./state.mjs";
import {
  CONTROLLED_FAILURE_STAGE,
  excerptOutput,
  runVerify,
} from "./verify.mjs";

function log(...args) {
  console.log("[ai-loop]", ...args);
}

function parseIssueNumber() {
  const n = env("ISSUE_NUMBER") || env("INPUT_ISSUE_NUMBER");
  if (!n) throw new Error("ISSUE_NUMBER is required");
  return Number(n);
}

function labelNames(labels) {
  return (labels || []).map((l) => (typeof l === "string" ? l : l.name));
}

function resolveSelfCorrectionTest(issue) {
  if (env("AI_LOOP_SELF_CORRECTION_TEST") === "1") return true;
  return labelNames(issue.labels).includes("ai-self-correction-test");
}

async function main() {
  const startedAt = Date.now();
  const cfg = loadConfigEnv();
  const issueNumber = parseIssueNumber();
  const repoKind = env("REPO_KIND", "backend");
  const baseBranch = env("BASE_BRANCH", "main");
  const repoUrl =
    env("REPO_URL") ||
    `${env("GITHUB_SERVER_URL", "https://github.com")}/${env("GITHUB_REPOSITORY")}`;
  const mock = env("AI_LOOP_MOCK_AGENT") === "1";
  const dryGit = mock || env("AI_LOOP_DRY_GIT") === "1";
  const skipGhIssue = mock || env("AI_LOOP_SKIP_GH") === "1";

  assertApiKeyPresent();

  let issue;
  if (skipGhIssue) {
    const scopeLabel =
      repoKind === "frontend" ? "repo:frontend" : "repo:backend";
    issue = {
      number: issueNumber,
      title: env("ISSUE_TITLE", "AI Loop Smoke Test"),
      body:
        env("ISSUE_BODY") ||
        [
          "### Repository scope",
          repoKind === "frontend" ? "frontend" : "backend",
          "",
          "## Goal",
          "Smoke-test the autonomous AI loop.",
          "## Acceptance Criteria",
          "- [ ] Create docs/ai-loop-smoke-marker.md",
          "- [ ] ./scripts/verify passes",
        ].join("\n"),
      labels: [{ name: "ai-task" }, { name: scopeLabel }],
    };
  } else {
    issue = fetchIssue(issueNumber);
  }

  const selfCorrectionTest = resolveSelfCorrectionTest(issue);
  if (selfCorrectionTest) {
    process.env.AI_LOOP_SELF_CORRECTION_TEST = "1";
    log("self-correction harness ENABLED (AI_LOOP_SELF_CORRECTION_TEST=1)");
  }

  const scope = resolveScope({
    labels: issue.labels || [],
    body: issue.body || "",
    repoKind,
  });
  log("scope", scope.mode, "repoKind", repoKind);

  if (!shouldRunInThisRepo(scope, repoKind)) {
    log("Skipping: this repository is out of scope for the issue");
    process.exit(0);
  }

  if (!skipGhIssue) {
    replaceLabels(issueNumber, {
      remove: ["ai-task"],
      add: ["ai-task-running"],
    });
  }

  const taskId = taskIdForIssue(issueNumber);
  initState(taskId, issue.title);

  const driver = await createAgentDriver({
    apiKey: process.env.CURSOR_API_KEY,
    repoUrl,
    baseBranch,
    issueNumber,
    modelId: env("AI_LOOP_MODEL", "composer-2.5"),
  });

  patchState(taskId, {
    issue_number: issueNumber,
    agent_id: driver.agentId,
    branch: null,
    repo_kind: repoKind,
    scope: scope.mode,
    status: "in_progress",
    self_correction_test: selfCorrectionTest,
  });

  let activeBranch = null;
  const fingerprintCounts = new Map();
  let lastVerify = null;
  let terminal = null;

  try {
    for (let i = 1; i <= cfg.MAX_ITERATIONS; i++) {
      if ((Date.now() - startedAt) / 60000 > cfg.MAX_EXECUTION_TIME_MINUTES) {
        terminal = "HUMAN_INTERVENTION_REQUIRED";
        recordState(taskId, {
          status: "aborted",
          failure: "MAX_EXECUTION_TIME_MINUTES exceeded",
          nextAction: "HUMAN_INTERVENTION_REQUIRED",
        });
        break;
      }

      const state = readState(taskId);
      const prompt =
        i === 1
          ? buildInitialPrompt({
              issueNumber,
              title: issue.title,
              body: issue.body,
              baseBranch,
              repoKind,
              maxIterations: cfg.MAX_ITERATIONS,
            })
          : buildFixPrompt({
              iteration: i,
              maxIterations: cfg.MAX_ITERATIONS,
              activeBranch,
              failedCommand: lastVerify?.command,
              exitCode: lastVerify?.exitCode,
              failedStage: lastVerify?.failedStage,
              excerpt: excerptOutput(lastVerify?.output || "", 200),
              previousAction:
                i === 2
                  ? "iteration 1 implement + verify"
                  : `iteration ${i - 1} fix + verify`,
              previousAttempts: JSON.stringify(state?.attempts || [], null, 2),
              changedFiles: activeBranch
                ? changedFilesVsBase(baseBranch)
                : [],
              diffStat: activeBranch ? diffStat(baseBranch) : "",
              diffPatch: activeBranch ? currentDiffPatch(baseBranch) : "",
              fingerprint: state?.last_fingerprint || "",
              sameFingerprintCount:
                fingerprintCounts.get(state?.last_fingerprint) || 0,
            });

      log(`agent send iteration=${i} kind=${driver.kind} agent_id=${driver.agentId}`);
      const sendResult = await driver.send(prompt);
      if (sendResult.status === "error") {
        recordState(taskId, {
          status: "failed",
          failure: `agent run status=error runId=${sendResult.runId}`,
          nextAction: "retry_or_human",
          command: "agent.send",
        });
        lastVerify = {
          command: "agent.send",
          exitCode: 2,
          output: `agent run error ${sendResult.runId}`,
          failedStage: "Agent Run",
        };
        // Avoid immediate re-send while Cloud Agent may still be winding down.
        log("agent run error; waiting 20s before next iteration");
        await new Promise((r) => setTimeout(r, 20000));
        continue;
      }

      if (sendResult.branch) {
        activeBranch = sendResult.branch;
        patchState(taskId, {
          branch: activeBranch,
          agent_id: driver.agentId,
          issue_number: issueNumber,
        });
        log("discovered branch", activeBranch);
      }

      if (!activeBranch) {
        terminal = "HUMAN_INTERVENTION_REQUIRED";
        recordState(taskId, {
          status: "aborted",
          failure:
            "Unable to determine agent-created branch (result.git.branches empty).",
          nextAction: "HUMAN_INTERVENTION_REQUIRED",
        });
        patchState(taskId, { final_status: terminal });
        break;
      }

      if (!dryGit) {
        syncBranch(activeBranch);
      } else if (mock) {
        // Mock already checked out the cursor/mock-* branch locally.
        syncBranch(activeBranch);
      }

      const files = changedFilesVsBase(baseBranch);
      if (files.length > cfg.MAX_CHANGED_FILES) {
        terminal = "HUMAN_INTERVENTION_REQUIRED";
        recordState(taskId, {
          status: "aborted",
          failure: `MAX_CHANGED_FILES exceeded (${files.length} > ${cfg.MAX_CHANGED_FILES})`,
          nextAction: "HUMAN_INTERVENTION_REQUIRED",
          files: files.join(","),
        });
        break;
      }

      const smokeMarker = path.join(
        REPO_ROOT,
        "docs",
        "ai-loop-smoke-marker.md",
      );
      // Prefer controlled harness over mock marker when enabled.
      if (selfCorrectionTest) {
        log("running verify (self-correction harness may inject first fail)");
        lastVerify = await runVerify({
          timeoutSeconds: cfg.COMMAND_TIMEOUT_SECONDS,
          taskId,
        });
      } else if (mock && !fs.existsSync(smokeMarker)) {
        lastVerify = {
          ok: false,
          exitCode: 1,
          command: "smoke-marker-check",
          failedStage: "Smoke Marker",
          output:
            "[FAIL] Smoke Marker\n       docs/ai-loop-smoke-marker.md missing (mock self-correction path)\nRESULT: FAILED",
        };
      } else {
        log("running ./scripts/verify");
        lastVerify = await runVerify({
          timeoutSeconds: cfg.COMMAND_TIMEOUT_SECONDS,
          taskId,
        });
      }

      const summaryLines = (lastVerify.output || "")
        .split("\n")
        .filter(
          (l) => /^\[(PASS|FAIL|SKIP)\]/.test(l) || l.startsWith("RESULT:"),
        )
        .join("\n");

      if (lastVerify.ok) {
        const rec = recordState(taskId, {
          status: "passed",
          command: "./scripts/verify",
          nextAction: "create_pr",
          files: files.join(","),
        });
        patchState(taskId, {
          final_status: "passed",
          verification_summary: summaryLines,
          branch: activeBranch,
          iteration: i,
          agent_id: driver.agentId,
        });
        log("verify PASSED", rec.stdout);

        const stateAfter = readState(taskId);
        const iterationDetails = buildIterationDetailsFromState(
          stateAfter,
          i,
          selfCorrectionTest || stateAfter?.controlled_failure_injected,
        );

        let prCi = {
          mode: dryGit || mock ? "skipped_dry_run" : "pending",
          url: null,
          note: dryGit || mock ? "Dry-run / mock — PR CI not started." : null,
        };

        const body = buildPrBody({
          title: issue.title,
          issueNumber,
          implementation: `Autonomous loop completed in ${i} iteration(s) on \`${activeBranch}\`.`,
          verification: summaryLines,
          iterationDetails,
          failedAttempts: JSON.stringify(
            (stateAfter?.failures || []).slice(0, 10),
            null,
            2,
          ),
          agentId: driver.agentId,
          branch: activeBranch,
          prCi,
        });

        let pr;
        try {
          pr = createPullRequest({
            title: `[AI] ${issue.title}`,
            body,
            head: activeBranch,
            base: baseBranch,
            dryRun: dryGit || mock,
          });
        } catch (prErr) {
          const permission = isPrPermissionError(prErr.message);
          log(
            permission
              ? "PR create blocked by GitHub Actions permissions; leaving branch for human"
              : `PR create failed: ${prErr.message}`,
          );
          patchState(taskId, {
            pr_url: null,
            pr_error: String(prErr.message || prErr).slice(0, 500),
            pr_body_preview: body.slice(0, 500),
            pr_ci_mode: "unavailable",
          });
          if (!skipGhIssue) {
            commentIssue(
              issueNumber,
              [
                "## AI Loop verify passed — PR not created",
                "",
                `- Branch: \`${activeBranch}\``,
                `- Iterations: ${i}`,
                `- Agent: \`${driver.agentId || "n/a"}\``,
                "",
                permission
                  ? [
                      "GitHub Actions is not allowed to create PRs with the default `GITHUB_TOKEN`.",
                      "",
                      "**Fix (pick one):**",
                      "1. Repo **Settings → Actions → General → Workflow permissions** → enable *Allow GitHub Actions to create and approve pull requests*",
                      "2. Or add secret `AI_LOOP_GH_TOKEN` (PAT with `contents` + `pull_requests`) and re-run / open PR manually",
                      "",
                      "Open PR manually:",
                      "```bash",
                      `gh pr create --base ${baseBranch} --head ${activeBranch} --title "[AI] ${issue.title}"`,
                      "```",
                    ].join("\n")
                  : `PR error: \`${String(prErr.message || prErr).slice(0, 300)}\``,
                "",
                "Human review required before merge to `main`.",
              ].join("\n"),
            );
            replaceLabels(issueNumber, {
              remove: ["ai-task-running"],
              add: ["ai-task-done"],
            });
          }
          terminal = "passed";
          break;
        }

        if (pr.url && !pr.dryRun) {
          prCi = ensurePrCi({
            prUrl: pr.url,
            headBranch: activeBranch,
            repoKind,
          });
          const bodyWithCi = buildPrBody({
            title: issue.title,
            issueNumber,
            implementation: `Autonomous loop completed in ${i} iteration(s) on \`${activeBranch}\`.`,
            verification: summaryLines,
            iterationDetails,
            failedAttempts: JSON.stringify(
              (readState(taskId)?.failures || []).slice(0, 10),
              null,
              2,
            ),
            agentId: driver.agentId,
            branch: activeBranch,
            prCi: {
              mode: prCi.mode,
              url: prCi.url,
              note: prCi.note,
            },
          });
          updatePullRequestBody(pr.url, bodyWithCi);
          patchState(taskId, {
            pr_url: pr.url,
            pr_ci_triggered: prCi.triggered,
            pr_ci_mode: prCi.mode,
            pr_ci_url: prCi.url,
            pr_ci_note: prCi.note,
          });
          log("PR CI", prCi.mode, prCi.url || prCi.note);
        } else {
          patchState(taskId, {
            pr_url: pr.url,
            pr_body_preview: pr.dryRun ? body.slice(0, 500) : undefined,
            pr_ci_mode: prCi.mode,
          });
        }

        if (!skipGhIssue) {
          commentIssue(
            issueNumber,
            [
              "## AI Loop completed",
              "",
              `- Status: **passed**`,
              `- Branch: \`${activeBranch}\``,
              `- Iterations: ${i}`,
              `- Agent: \`${driver.agentId || "n/a"}\``,
              pr.url ? `- PR: ${pr.url}` : "- PR: dry-run / skipped",
              prCi?.mode
                ? `- Independent GitHub CI: \`${prCi.mode}\`${prCi.url ? ` (${prCi.url})` : ""}`
                : null,
              selfCorrectionTest
                ? `- Self-correction harness: used (\`${CONTROLLED_FAILURE_STAGE}\`)`
                : null,
              "",
              "Human review required before merge to `main`. Agent verify PASS ≠ GitHub CI.",
            ]
              .filter(Boolean)
              .join("\n"),
          );
          replaceLabels(issueNumber, {
            remove: ["ai-task-running"],
            add: ["ai-task-done"],
          });
        }

        if (scope.mode === "both" && repoKind === "backend") {
          const sibling = triggerSiblingWorkflow({
            uiRepo: env("AI_LOOP_UI_REPO"),
            issueNumber,
          });
          if (!sibling.ok) {
            log(sibling.reason || sibling.stderr || "sibling trigger skipped");
            if (!skipGhIssue) {
              commentIssue(
                issueNumber,
                [
                  "## Cross-repo note",
                  "",
                  sibling.reason ||
                    "Could not dispatch frontend ai-loop.yml. Trigger UI repo manually with the same issue / label.",
                ].join("\n"),
              );
            }
          }
        }

        terminal = "passed";
        break;
      }

      const fp = fingerprintFailure({
        failedStage: lastVerify.failedStage,
        exitCode: lastVerify.exitCode,
        excerpt: excerptOutput(lastVerify.output, 80),
      });
      const count = (fingerprintCounts.get(fp) || 0) + 1;
      fingerprintCounts.set(fp, count);
      patchState(taskId, {
        last_fingerprint: fp,
        branch: activeBranch,
        agent_id: driver.agentId,
      });

      const rec = recordState(taskId, {
        status: "failed",
        failure: `${lastVerify.failedStage || "verify"} exit=${lastVerify.exitCode} fp=${fp}`,
        nextAction:
          count >= cfg.MAX_RETRIES_PER_TEST
            ? "HUMAN_INTERVENTION_REQUIRED"
            : "fix",
        command: "./scripts/verify",
        files: files.join(","),
      });

      log(
        "verify FAILED",
        "fp",
        fp,
        "count",
        count,
        "record",
        rec.code,
        lastVerify.controlled ? "(controlled harness)" : "",
      );

      if (count >= cfg.MAX_RETRIES_PER_TEST) {
        terminal = "HUMAN_INTERVENTION_REQUIRED";
        patchState(taskId, { final_status: terminal });
        break;
      }
      if (rec.code === 3) {
        terminal = "LOOP_FAILED";
        patchState(taskId, { final_status: terminal });
        break;
      }
      if (i >= cfg.MAX_ITERATIONS) {
        terminal = "LOOP_FAILED";
        patchState(taskId, { final_status: terminal });
        break;
      }
    }

    if (terminal !== "passed") {
      const status = terminal || "LOOP_FAILED";
      patchState(taskId, { final_status: status, branch: activeBranch });
      if (!skipGhIssue) {
        commentIssue(
          issueNumber,
          [
            "## AI Loop stopped",
            "",
            `- Status: **${status}**`,
            activeBranch
              ? `- Branch: \`${activeBranch}\``
              : "- Branch: *(undetermined)*",
            `- Agent: \`${driver.agentId || "n/a"}\``,
            status === "HUMAN_INTERVENTION_REQUIRED" && !activeBranch
              ? "\nReason: Unable to determine agent-created branch."
              : "",
            "",
            "Human intervention required. Inspect `.ai-loop` artifacts in the workflow run.",
          ]
            .filter(Boolean)
            .join("\n"),
        );
        replaceLabels(issueNumber, {
          remove: ["ai-task-running"],
          add: ["ai-task-failed"],
        });
      }
      copyStateArtifact(taskId, path.join(REPO_ROOT, ".ai-loop", "logs"));
      await driver.dispose();
      process.exit(2);
    }

    copyStateArtifact(taskId, path.join(REPO_ROOT, ".ai-loop", "logs"));
    await driver.dispose();
    process.exit(0);
  } catch (err) {
    console.error("[ai-loop] fatal:", err?.message || err);
    try {
      await driver.dispose();
    } catch {
      /* ignore */
    }
    if (!skipGhIssue) {
      replaceLabels(issueNumber, {
        remove: ["ai-task-running"],
        add: ["ai-task-failed"],
      });
      commentIssue(
        issueNumber,
        `## AI Loop fatal error\n\n\`${String(err?.message || err).slice(0, 500)}\`\n\n(No secrets included.)`,
      );
    }
    process.exit(1);
  }
}

main();
