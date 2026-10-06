import { spawn } from "node:child_process";
import path from "node:path";
import { REPO_ROOT, env } from "./config.mjs";
import { patchState, readState } from "./state.mjs";

export const CONTROLLED_FAILURE_STAGE = "CONTROLLED_SELF_CORRECTION_TEST";

/**
 * When AI_LOOP_SELF_CORRECTION_TEST=1, the first verify call for a taskId
 * returns a deterministic failure (no scripts/verify spawn) and records
 * controlled_failure_injected in state. Later calls run real verify.
 */
export function maybeInjectControlledFailure(taskId) {
  if (env("AI_LOOP_SELF_CORRECTION_TEST") !== "1") return null;
  if (!taskId) return null;
  const state = readState(taskId);
  if (state?.controlled_failure_injected) return null;

  patchState(taskId, {
    controlled_failure_injected: true,
    failure_injected: true,
  });

  return {
    ok: false,
    exitCode: 42,
    signal: null,
    command: "./scripts/verify",
    failedStage: CONTROLLED_FAILURE_STAGE,
    controlled: true,
    output: [
      `[FAIL] ${CONTROLLED_FAILURE_STAGE}`,
      "       Deterministic self-correction harness failure.",
      "       This is NOT caused by agent code changes.",
      `       Marker: ${CONTROLLED_FAILURE_STAGE}`,
      "RESULT: FAILED",
    ].join("\n"),
  };
}

export function runVerify({ timeoutSeconds = 900, taskId } = {}) {
  const injected = maybeInjectControlledFailure(taskId);
  if (injected) {
    return Promise.resolve(injected);
  }

  const script = path.join(REPO_ROOT, "scripts", "verify");
  return new Promise((resolve) => {
    const child = spawn(script, [], {
      cwd: REPO_ROOT,
      env: { ...process.env },
      shell: false,
    });
    let stdout = "";
    let stderr = "";
    const killTimer = setTimeout(() => {
      child.kill("SIGKILL");
    }, timeoutSeconds * 1000);

    child.stdout.on("data", (d) => {
      stdout += d.toString();
    });
    child.stderr.on("data", (d) => {
      stderr += d.toString();
    });
    child.on("close", (code, signal) => {
      clearTimeout(killTimer);
      const combined = `${stdout}\n${stderr}`.trim();
      const failedStage =
        combined
          .split("\n")
          .reverse()
          .find((l) => l.startsWith("[FAIL]"))
          ?.replace("[FAIL] ", "") || null;
      resolve({
        ok: code === 0,
        exitCode: code ?? 1,
        signal,
        output: combined,
        failedStage,
        command: "./scripts/verify",
        controlled: false,
      });
    });
  });
}

export function excerptOutput(output, maxLines = 200) {
  const lines = (output || "").split("\n");
  return lines.slice(-maxLines).join("\n");
}
