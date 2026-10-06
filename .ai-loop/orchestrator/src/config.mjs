import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
export const ORCHESTRATOR_DIR = path.resolve(__dirname, "..");
export const REPO_ROOT = path.resolve(ORCHESTRATOR_DIR, "../..");

export function loadConfigEnv() {
  const configPath = path.join(REPO_ROOT, ".ai-loop", "config.env");
  const defaults = {
    MAX_ITERATIONS: 5,
    MAX_RETRIES_PER_TEST: 3,
    MAX_EXECUTION_TIME_MINUTES: 45,
    MAX_CHANGED_FILES: 40,
    COMMAND_TIMEOUT_SECONDS: 900,
    VERIFY_E2E: 0,
    VERIFY_AUDIT_STRICT: 0,
  };
  const out = { ...defaults };
  if (!fs.existsSync(configPath)) return out;
  const text = fs.readFileSync(configPath, "utf8");
  for (const line of text.split("\n")) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith("#")) continue;
    const eq = trimmed.indexOf("=");
    if (eq < 0) continue;
    const key = trimmed.slice(0, eq).trim();
    const value = trimmed.slice(eq + 1).trim();
    if (key in defaults) {
      const n = Number(value);
      out[key] = Number.isFinite(n) ? n : value;
    }
  }
  return out;
}

export function env(name, fallback = "") {
  const v = process.env[name];
  return v == null || v === "" ? fallback : v;
}

/** Never log secret values. */
export function assertApiKeyPresent() {
  if (env("AI_LOOP_MOCK_AGENT") === "1") return;
  if (!process.env.CURSOR_API_KEY) {
    throw new Error(
      "CURSOR_API_KEY is missing. Set it as a GitHub Actions secret (never commit it).",
    );
  }
}
