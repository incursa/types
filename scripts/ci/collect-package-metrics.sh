#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 3 ]]; then
  echo "usage: $0 <package-dir> <output-json> <github-output-file|->" >&2
  exit 1
fi

pkg_dir="$1"
out_json="$2"
out_env="$3"

count=0
status="❌"
notes="no packages found"
versions=""

if compgen -G "$pkg_dir/*.nupkg" > /dev/null; then
  mapfile -t files < <(ls -1 "$pkg_dir"/*.nupkg | sort)
  count="${#files[@]}"
  status="✅"
  notes="packages created"

  entries=()
  for f in "${files[@]}"; do
    base="$(basename "$f" .nupkg)"
    parsed="$(echo "$base" | sed -E 's/^(.*)\.([0-9]+\.[0-9]+\.[0-9]+([.-][A-Za-z0-9.-]+)?)$/\1 \2/')"
    if [[ "$parsed" == "$base" ]]; then
      entries+=("$base")
    else
      id="${parsed% *}"
      ver="${parsed##* }"
      entries+=("$id $ver")
    fi
  done
  versions="$(IFS='; '; echo "${entries[*]}")"
fi

mkdir -p "$(dirname "$out_json")"
versions_json="$(printf '%s' "$versions" | sed 's/"/\\"/g')"
cat > "$out_json" <<JSON
{"count":$count,"status":"$status","versions":"$versions_json","notes":"$notes"}
JSON

if [[ "$out_env" != "-" ]]; then
  {
    echo "package_count=$count"
    echo "package_status=$status"
    echo "package_versions=$versions"
    echo "package_notes=$notes"
  } >> "$out_env"
fi
