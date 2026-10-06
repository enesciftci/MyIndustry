# AI Loop smoke marker

Infrastructure marker for the autonomous AI loop **self-correction harness** (issue #7).

When `AI_LOOP_SELF_CORRECTION_HARNESS=1`, the orchestrator injects a controlled first-verify
failure (`CONTROLLED_SELF_CORRECTION_TEST`). The agent adds or updates this file on the
self-correction iteration; the next `./scripts/verify` run is real and must pass.

Do not delete this file while the harness issue is used for loop regression tests.
