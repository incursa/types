#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 5 ]]; then
  echo "usage: $0 <cobertura-xml> <line-threshold> <branch-threshold> <output-json> <github-output-file|->" >&2
  exit 1
fi

xml_file="$1"
line_gate="$2"
branch_gate="$3"
out_json="$4"
out_env="$5"

line_pct="0.00"
branch_pct="0.00"
status_line="❌"
status_branch="❌"
notes="metrics missing"

if [[ -f "$xml_file" ]]; then
  coverage_line="$(grep -m1 -o '<coverage[^>]*>' "$xml_file" || true)"
  if [[ -n "$coverage_line" ]]; then
    line_rate="$(echo "$coverage_line" | sed -n 's/.*line-rate="\([0-9.]*\)".*/\1/p')"
    branch_rate="$(echo "$coverage_line" | sed -n 's/.*branch-rate="\([0-9.]*\)".*/\1/p')"
    line_rate="${line_rate:-0}"
    branch_rate="${branch_rate:-0}"
    line_pct="$(awk -v r="$line_rate" 'BEGIN { printf "%.2f", r * 100 }')"
    branch_pct="$(awk -v r="$branch_rate" 'BEGIN { printf "%.2f", r * 100 }')"

    if awk -v a="$line_pct" -v b="$line_gate" 'BEGIN { exit !(a >= b) }'; then status_line="✅"; else status_line="❌"; fi
    if awk -v a="$branch_pct" -v b="$branch_gate" 'BEGIN { exit !(a >= b) }'; then status_branch="✅"; else status_branch="❌"; fi
    notes="parsed from cobertura"
  fi
fi

mkdir -p "$(dirname "$out_json")"
cat > "$out_json" <<JSON
{"line_pct":$line_pct,"branch_pct":$branch_pct,"line_threshold":$line_gate,"branch_threshold":$branch_gate,"line_status":"$status_line","branch_status":"$status_branch","notes":"$notes"}
JSON

if [[ "$out_env" != "-" ]]; then
  {
    echo "coverage_line_pct=$line_pct"
    echo "coverage_branch_pct=$branch_pct"
    echo "coverage_line_threshold=$line_gate"
    echo "coverage_branch_threshold=$branch_gate"
    echo "coverage_line_status=$status_line"
    echo "coverage_branch_status=$status_branch"
    echo "coverage_notes=$notes"
  } >> "$out_env"
fi
