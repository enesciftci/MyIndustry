#!/usr/bin/env bash
# Shared PASS/FAIL reporting for AI Loop verification scripts.
# Optional: set REPORT_LOG to also append lines to a file (no subshell).

REPORT_LINES=()
REPORT_FAILED=0
REPORT_LOG="${REPORT_LOG:-}"

_emit() {
  local line="$1"
  echo "$line"
  if [[ -n "$REPORT_LOG" ]]; then
    echo "$line" >> "$REPORT_LOG"
  fi
}

report_pass() {
  local name="$1"
  local line="[PASS] ${name}"
  REPORT_LINES+=("$line")
  _emit "$line"
}

report_fail() {
  local name="$1"
  local detail="${2:-}"
  local line="[FAIL] ${name}"
  REPORT_LINES+=("$line")
  REPORT_FAILED=1
  _emit "$line"
  if [[ -n "$detail" ]]; then
    _emit "       ${detail}"
  fi
}

report_skip() {
  local name="$1"
  local detail="${2:-}"
  local line="[SKIP] ${name}"
  REPORT_LINES+=("$line")
  _emit "$line"
  if [[ -n "$detail" ]]; then
    _emit "       ${detail}"
  fi
}

# run_step "Name" command args...
run_step() {
  local name="$1"
  shift
  local logfile
  logfile="$(mktemp)"
  echo "==> ${name}: $*"
  set +e
  "$@" >"$logfile" 2>&1
  local rc=$?
  set -e
  cat "$logfile"
  if [[ -n "$REPORT_LOG" ]]; then
    cat "$logfile" >> "$REPORT_LOG"
  fi
  if [[ $rc -eq 0 ]]; then
    report_pass "$name"
  else
    report_fail "$name" "command: $* (exit ${rc})"
    echo "------- last 40 lines -------"
    tail -n 40 "$logfile" || true
    echo "-----------------------------"
  fi
  rm -f "$logfile"
  return 0
}

print_summary() {
  echo ""
  echo "======== VERIFICATION SUMMARY ========"
  local line
  if [[ ${#REPORT_LINES[@]} -eq 0 ]]; then
    echo "(no stages recorded)"
  else
    for line in "${REPORT_LINES[@]}"; do
      echo "$line"
    done
  fi
  echo "======================================"
  if [[ "$REPORT_FAILED" -ne 0 ]]; then
    echo "RESULT: FAILED"
    return 1
  fi
  echo "RESULT: PASSED"
  return 0
}
