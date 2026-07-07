#!/usr/bin/env bash
# Completes remaining security plan items blocked from IDE hooks.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UI="$ROOT/../MyIndustry.UI"

echo "==> Copying appsettings.Production.json templates"
cp "$ROOT/MyIndustry.Container/ProductionAllowedHosts.Api.json" "$ROOT/MyIndustry.Api/appsettings.Production.json"
cp "$ROOT/MyIndustry.Container/ProductionAllowedHosts.Identity.json" "$ROOT/MyIndustry.Identity.Api/appsettings.Production.json"
cp "$ROOT/MyIndustry.Container/ProductionAllowedHosts.Gateway.json" "$ROOT/MyIndustry.Gateway/appsettings.Production.json"
echo "    OK"

echo "==> dotnet build"
cd "$ROOT" && dotnet build

echo "==> dotnet test (AllowedHosts + security unit tests)"
dotnet test --no-build --filter "FullyQualifiedName~AllowedHostsExtensionsTests|FullyQualifiedName~SensitiveDataMaskerTests|FullyQualifiedName~CorsExtensionsTests|FullyQualifiedName~UserServiceTests|FullyQualifiedName~ImageUploadValidatorTests"

if [ -d "$UI" ]; then
  echo "==> npm run build"
  cd "$UI" && npm run build
  echo "==> npm audit (high+)"
  npm audit --audit-level=high || true
else
  echo "==> Skipping frontend (MyIndustry.UI not found at $UI)"
fi

echo ""
echo "Done. Compose layout: see COMPOSE.md"
echo "  Local:  docker compose up -d"
echo "  Debug:  docker compose --profile debug up -d"
echo "  Dokploy app: docker-compose.dokploy.yaml"
