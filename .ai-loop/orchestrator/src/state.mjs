import { spawnSync } from "node:child_process";
import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { REPO_ROOT } from "./config.mjs";

export function taskIdForIssue(issueNumber) {
  return `issue-${issueNumber}`;
}

export function statePath(taskId) {
  return path.join(REPO_ROOT, ".ai-loop", "state", `${taskId}.json`);
}

export function initState(taskId, taskSummary) {
  const script = path.join(REPO_ROOT, "scripts", "ai-loop-init");
  const r = spawnSync(script, ["--task-id", taskId, "--task", taskSummary], {
    cwd: REPO_ROOT,
    encoding: "utf8",
  });
  if (r.status !== 0) {
    throw new Error(`ai-loop-init failed: ${r.stderr || r.stdout}`);
  }
  return readState(taskId);
}

export function recordState(taskId, { status, failure, nextAction, command, files }) {
  const script = path.join(REPO_ROOT, "scripts", "ai-loop-record");
  const args = ["--task-id", taskId, "--status", status];
  if (failure) args.push("--failure", failure.slice(0, 4000));
  if (nextAction) args.push("--next-action", nextAction);
  if (command) args.push("--command", command);
  if (files) args.push("--files", files);
  const r = spawnSync(script, args, { cwd: REPO_ROOT, encoding: "utf8" });
  // exit 3 = aborted by MAX_ITERATIONS
  return { code: r.status ?? 1, stdout: r.stdout || "", stderr: r.stderr || "" };
}

export function readState(taskId) {
  const p = statePath(taskId);
  if (!fs.existsSync(p)) return null;
  return JSON.parse(fs.readFileSync(p, "utf8"));
}

export function patchState(taskId, patch) {
  const p = statePath(taskId);
  const cur = readState(taskId) || {};
  const next = { ...cur, ...patch };
  fs.mkdirSync(path.dirname(p), { recursive: true });
  fs.writeFileSync(p, `${JSON.stringify(next, null, 2)}\n`);
  return next;
}

export function fingerprintFailure({ failedStage, exitCode, excerpt }) {
  const firstLine = (excerpt || "")
    .split("\n")
    .map((l) => l.trim())
    .find((l) => l && !l.startsWith("==>") && !l.startsWith("["));
  const material = `${failedStage}|${exitCode}|${firstLine || ""}`;
  return crypto.createHash("sha256").update(material).digest("hex").slice(0, 16);
}

export function copyStateArtifact(taskId, destDir) {
  const src = statePath(taskId);
  if (!fs.existsSync(src)) return;
  fs.mkdirSync(destDir, { recursive: true });
  fs.copyFileSync(src, path.join(destDir, `${taskId}.json`));
}
