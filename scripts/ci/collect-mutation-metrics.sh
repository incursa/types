#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 3 ]]; then
  echo "usage: $0 <stryker-dir> <output-json> <github-output-file|->" >&2
  exit 1
fi

report_dir="$1"
out_json="$2"
out_env="$3"

score="0"
threshold="35"
killed="0"
survived="0"
no_coverage="0"
status="⚠️"
notes="metrics missing"

if [[ -d "$report_dir" ]]; then
  score_line="$(rg -i -m1 'mutation score[^0-9]*[0-9]+(\.[0-9]+)?' "$report_dir" 2>/dev/null || true)"
  if [[ -n "$score_line" ]]; then
    score="$(echo "$score_line" | sed -E 's/.*([0-9]+(\.[0-9]+)?).*/\1/')"
    notes="parsed from report text"
  fi

  killed_line="$(rg -i -m1 'killed[^0-9]*[0-9]+' "$report_dir" 2>/dev/null || true)"
  survived_line="$(rg -i -m1 'survived[^0-9]*[0-9]+' "$report_dir" 2>/dev/null || true)"
  no_cov_line="$(rg -i -m1 'no[ -]?coverage[^0-9]*[0-9]+' "$report_dir" 2>/dev/null || true)"

  if [[ -n "$killed_line" ]]; then killed="$(echo "$killed_line" | sed -E 's/.*([0-9]+).*/\1/')"; fi
  if [[ -n "$survived_line" ]]; then survived="$(echo "$survived_line" | sed -E 's/.*([0-9]+).*/\1/')"; fi
  if [[ -n "$no_cov_line" ]]; then no_coverage="$(echo "$no_cov_line" | sed -E 's/.*([0-9]+).*/\1/')"; fi
fi

if awk -v a="$score" -v b="$threshold" 'BEGIN { exit !(a >= b) }'; then
  status="✅"
else
  status="❌"
fi

mkdir -p "$(dirname "$out_json")"
cat > "$out_json" <<JSON
{"score":$score,"threshold":$threshold,"killed":$killed,"survived":$survived,"no_coverage":$no_coverage,"status":"$status","notes":"$notes"}
JSON

if [[ "$out_env" != "-" ]]; then
  {
    echo "mutation_score=$score"
    echo "mutation_threshold=$threshold"
    echo "mutation_killed=$killed"
    echo "mutation_survived=$survived"
    echo "mutation_no_coverage=$no_coverage"
    echo "mutation_status=$status"
    echo "mutation_notes=$notes"
  } >> "$out_env"
fi
