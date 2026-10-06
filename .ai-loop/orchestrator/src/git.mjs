import { spawnSync } from "node:child_process";
import { REPO_ROOT } from "./config.mjs";

function run(cmd, args, opts = {}) {
  const r = spawnSync(cmd, args, {
    cwd: REPO_ROOT,
    encoding: "utf8",
    ...opts,
  });
  return {
    code: r.status ?? 1,
    stdout: (r.stdout || "").trim(),
    stderr: (r.stderr || "").trim(),
  };
}

export function slugify(title, max = 40) {
  const s = String(title || "task")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, max)
    .replace(/-+$/g, "");
  return s || "task";
}

/** @deprecated Cloud path no longer pre-creates ai/* branches. Kept for tools/debug. */
export function branchName(issueNumber, title) {
  return `ai/${issueNumber}-${slugify(title)}`;
}

/**
 * Local helper only. Cloud Agent path must NOT call this.
 * Mock agent creates cursor/mock-* branches itself in agent.mjs.
 */
export function ensureBranch({ baseBranch, branch, dryRun = false }) {
  run("git", ["fetch", "origin", baseBranch]);
  const remoteBranch = run("git", ["rev-parse", "--verify", `origin/${branch}`]);
  if (remoteBranch.code === 0) {
    run("git", ["checkout", "-B", branch, `origin/${branch}`]);
    return { created: false, branch };
  }
  const remoteBase = run("git", ["rev-parse", "--verify", `origin/${baseBranch}`]);
  const baseRef =
    remoteBase.code === 0
      ? `origin/${baseBranch}`
      : run("git", ["rev-parse", "--verify", baseBranch]).code === 0
        ? baseBranch
        : "HEAD";
  const co = run("git", ["checkout", "-B", branch, baseRef]);
  if (co.code !== 0) {
    throw new Error(`git checkout -B ${branch} failed: ${co.stderr || co.stdout}`);
  }
  if (!dryRun) {
    const push = run("git", ["push", "-u", "origin", branch]);
    if (push.code !== 0) {
      throw new Error(`git push failed: ${push.stderr || push.stdout}`);
    }
  }
  return { created: true, branch };
}

export function syncBranch(branch) {
  run("git", ["fetch", "origin", branch]);
  const r = run("git", ["checkout", "-B", branch, `origin/${branch}`]);
  if (r.code !== 0) {
    // local-only mock: stay on local branch
    run("git", ["checkout", branch]);
  }
}

export function changedFilesVsBase(baseBranch) {
  const r = run("git", ["diff", "--name-only", `origin/${baseBranch}...HEAD`]);
  if (r.code !== 0) {
    const r2 = run("git", ["diff", "--name-only", `${baseBranch}...HEAD`]);
    return r2.stdout ? r2.stdout.split("\n").filter(Boolean) : [];
  }
  return r.stdout ? r.stdout.split("\n").filter(Boolean) : [];
}

export function diffStat(baseBranch) {
  const r = run("git", ["diff", "--stat", `origin/${baseBranch}...HEAD`]);
  if (r.code !== 0) {
    return run("git", ["diff", "--stat", `${baseBranch}...HEAD`]).stdout;
  }
  return r.stdout;
}

export function currentDiffPatch(baseBranch, maxChars = 12000) {
  const r = run("git", ["diff", `origin/${baseBranch}...HEAD`]);
  const text = r.code === 0 ? r.stdout : run("git", ["diff", `${baseBranch}...HEAD`]).stdout;
  if (!text) return "(no diff)";
  return text.length > maxChars ? `${text.slice(0, maxChars)}\n...[truncated]` : text;
}
