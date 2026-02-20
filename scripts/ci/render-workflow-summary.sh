#!/usr/bin/env bash
set -euo pipefail

summary_file="${GITHUB_STEP_SUMMARY:-}"
if [[ -z "$summary_file" ]]; then
  echo "GITHUB_STEP_SUMMARY is not set" >&2
  exit 1
fi

title="${SUMMARY_TITLE:-CI Quality Summary}"
workflow="${SUMMARY_WORKFLOW:-${GITHUB_WORKFLOW:-unknown}}"
ref_name="${SUMMARY_REF:-${GITHUB_REF_NAME:-unknown}}"
sha_short="${SUMMARY_SHA:-${GITHUB_SHA:-unknown}}"
sha_short="${sha_short:0:7}"
trigger="${SUMMARY_TRIGGER:-${GITHUB_EVENT_NAME:-unknown}}"
overall="${SUMMARY_RESULT:-⚠️ Partial}"

{
  echo "# $title"
  echo ""
  echo "**Workflow:** $workflow  "
  echo "**Ref:** $ref_name  "
  echo "**Commit:** $sha_short  "
  echo "**Trigger:** $trigger  "
  echo "**Result:** $overall"
  echo ""
  echo "## Executive Gates"
  echo "| Gate | Status | Metric | Threshold | Notes |"
  echo "|---|---|---:|---:|---|"
  if [[ -n "${SUMMARY_GATE_ROWS:-}" ]]; then
    printf "%b\n" "$SUMMARY_GATE_ROWS"
  else
    echo "| 📝 Reporting | ❌ | Metrics missing | n/a | No gate rows were provided |"
  fi

  if [[ -n "${SUMMARY_ARTIFACT_ROWS:-}" ]]; then
    echo ""
    echo "## Artifacts"
    printf "%b\n" "$SUMMARY_ARTIFACT_ROWS"
  fi

  if [[ -n "${SUMMARY_ACTIONS:-}" ]]; then
    echo ""
    echo "## 📝 Action Required"
    printf "%b\n" "$SUMMARY_ACTIONS"
  fi
} >> "$summary_file"
