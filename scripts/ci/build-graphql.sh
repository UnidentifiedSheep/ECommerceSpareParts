#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
artifact_root="${1:-${repository_root}/artifacts/graphql}"

if [[ "$artifact_root" != /* ]]; then
  artifact_root="${repository_root}/${artifact_root}"
fi

cd "$repository_root"

dotnet tool restore

source_schemas=(
  "Main:src/Services/Main/Main.Api/Main.Api.csproj"
  "Search:src/Services/Search/Search.Api/Search.Api.csproj"
  "Analytics:src/Services/Analytics/Analytics.Api/Analytics.Api.csproj"
)

for source_schema in "${source_schemas[@]}"; do
  schema_name="${source_schema%%:*}"
  schema_project="${source_schema#*:}"
  schema_dir="${artifact_root}/${schema_name}"

  mkdir -p "$schema_dir"

  dotnet restore "$schema_project"
  dotnet build \
    "$schema_project" \
    --configuration Release \
    --no-restore

  DOTNET_ENVIRONMENT=Development \
  ASPNETCORE_ENVIRONMENT=Development \
    dotnet run \
      --project "$schema_project" \
      --configuration Release \
      --no-build \
      --no-launch-profile \
      -- schema export \
      --schema-name "$schema_name" \
      --output "$schema_dir/schema.graphqls"

  cp \
    "schemas/graphql/${schema_name}/schema-settings.json" \
    "$schema_dir/schema-settings.json"
done

mkdir -p "${artifact_root}/Gateway"

dotnet tool run nitro -- fusion compose \
  -f "${artifact_root}/Main/schema.graphqls" \
  -f "${artifact_root}/Search/schema.graphqls" \
  -f "${artifact_root}/Analytics/schema.graphqls" \
  -a "${artifact_root}/Gateway/gateway.far"

test -s "${artifact_root}/Gateway/gateway.far"

if command -v unzip > /dev/null 2>&1; then
  unzip -t "${artifact_root}/Gateway/gateway.far"
fi
