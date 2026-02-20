#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 3 ]]; then
  echo "usage: $0 <trx-file> <output-json> <github-output-file|->" >&2
  exit 1
fi

trx_file="$1"
out_json="$2"
out_env="$3"

passed=0
failed=0
skipped=0
total=0
status="❌"
notes="metrics missing"

if [[ -f "$trx_file" ]]; then
  counters_line="$(grep -m1 -o '<Counters[^>]*/>' "$trx_file" || true)"
  if [[ -n "$counters_line" ]]; then
    passed="$(echo "$counters_line" | sed -n 's/.*passed="\([0-9]\+\)".*/\1/p')"
    failed="$(echo "$counters_line" | sed -n 's/.*failed="\([0-9]\+\)".*/\1/p')"
    total="$(echo "$counters_line" | sed -n 's/.*total="\([0-9]\+\)".*/\1/p')"
    skipped="$(echo "$counters_line" | sed -n 's/.*notExecuted="\([0-9]\+\)".*/\1/p')"

    passed="${passed:-0}"
    failed="${failed:-0}"
    total="${total:-0}"
    skipped="${skipped:-0}"

    if [[ "$failed" == "0" ]]; then
      status="✅"
      notes="all tests passed"
    else
      status="❌"
      notes="test failures detected"
    fi
  fi
fi

mkdir -p "$(dirname "$out_json")"
cat > "$out_json" <<JSON
{"passed":$passed,"failed":$failed,"skipped":$skipped,"total":$total,"status":"$status","notes":"$notes"}
JSON

if [[ "$out_env" != "-" ]]; then
  {
    echo "tests_passed=$passed"
    echo "tests_failed=$failed"
    echo "tests_skipped=$skipped"
    echo "tests_total=$total"
    echo "tests_status=$status"
    echo "tests_notes=$notes"
  } >> "$out_env"
fi
