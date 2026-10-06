# AI Loop smoke marker

Infrastructure marker for the autonomous AI loop **self-correction harness** (see issue #7).

When `AI_LOOP_SELF_CORRECTION_HARNESS=1` (or the issue title matches the self-correction infrastructure test), the orchestrator injects `CONTROLLED_SELF_CORRECTION_TEST` on **iteration 1** if this file is missing. After the agent adds this file and pushes, **iteration 2** runs real `./scripts/verify`.

This file is safe to keep in the repository; it documents that the loop self-correction path was exercised.
