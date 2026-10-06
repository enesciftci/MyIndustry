# AI Loop Smoke Marker

Created for AI loop smoke / self-correction infrastructure tests.

When `AI_LOOP_SELF_CORRECTION_TEST=1`, the orchestrator injects `CONTROLLED_SELF_CORRECTION_TEST` on the **first** verify (state-gated). Later iterations run real `./scripts/verify`. This marker file may be created by the agent as acceptance criteria for smoke issues.
