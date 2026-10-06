/**
 * Resolve which repos this task targets.
 * Labels: repo:backend | repo:frontend | repo:both
 * Body fallback: "Repository scope" dropdown text (backend|frontend|both)
 * Default: hosting repo only (REPO_KIND=backend|frontend).
 */
export function resolveScope({ labels = [], body = "", repoKind = "backend" } = {}) {
  const names = labels.map((l) => (typeof l === "string" ? l : l.name || ""));
  if (names.includes("repo:both")) {
    return { mode: "both", backend: true, frontend: true };
  }
  if (names.includes("repo:backend") && names.includes("repo:frontend")) {
    return { mode: "both", backend: true, frontend: true };
  }
  if (names.includes("repo:backend")) {
    return { mode: "backend", backend: true, frontend: false };
  }
  if (names.includes("repo:frontend")) {
    return { mode: "frontend", backend: false, frontend: true };
  }

  const bodyScope = String(body || "").match(
    /### Repository scope\s*\n\s*(backend|frontend|both)/i,
  );
  if (bodyScope) {
    const v = bodyScope[1].toLowerCase();
    if (v === "both") return { mode: "both", backend: true, frontend: true };
    if (v === "frontend") return { mode: "frontend", backend: false, frontend: true };
    return { mode: "backend", backend: true, frontend: false };
  }

  if (repoKind === "frontend") {
    return { mode: "frontend", backend: false, frontend: true };
  }
  return { mode: "backend", backend: true, frontend: false };
}

export function shouldRunInThisRepo(scope, repoKind) {
  if (repoKind === "backend") return scope.backend;
  if (repoKind === "frontend") return scope.frontend;
  return true;
}
