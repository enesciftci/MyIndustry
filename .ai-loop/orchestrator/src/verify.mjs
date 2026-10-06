import { spawn } from "node:child_process";
import path from "node:path";
import { REPO_ROOT } from "./config.mjs";

export function runVerify({ timeoutSeconds = 900 } = {}) {
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
      });
    });
  });
}

export function excerptOutput(output, maxLines = 200) {
  const lines = (output || "").split("\n");
  return lines.slice(-maxLines).join("\n");
}
