import assert from "node:assert/strict";
import fs from "node:fs";
import { describe, it, before, after } from "node:test";
import {
  CONTROLLED_FAILURE_STAGE,
  maybeInjectControlledFailure,
} from "./verify.mjs";
import { patchState, readState, statePath } from "./state.mjs";

const TASK = "issue-harness-unit";

describe("controlled self-correction harness", () => {
  before(() => {
    delete process.env.AI_LOOP_SELF_CORRECTION_TEST;
    const p = statePath(TASK);
    if (fs.existsSync(p)) fs.unlinkSync(p);
    patchState(TASK, { task_id: TASK });
  });

  after(() => {
    delete process.env.AI_LOOP_SELF_CORRECTION_TEST;
    const p = statePath(TASK);
    if (fs.existsSync(p)) fs.unlinkSync(p);
  });

  it("does nothing when env unset", () => {
    delete process.env.AI_LOOP_SELF_CORRECTION_TEST;
    const r = maybeInjectControlledFailure(TASK);
    assert.equal(r, null);
    assert.equal(readState(TASK)?.controlled_failure_injected, undefined);
  });

  it("injects once then no-ops", () => {
    process.env.AI_LOOP_SELF_CORRECTION_TEST = "1";
    const first = maybeInjectControlledFailure(TASK);
    assert.ok(first);
    assert.equal(first.ok, false);
    assert.equal(first.exitCode, 42);
    assert.equal(first.failedStage, CONTROLLED_FAILURE_STAGE);
    assert.match(first.output, /CONTROLLED_SELF_CORRECTION_TEST/);
    assert.equal(readState(TASK).controlled_failure_injected, true);
    assert.equal(readState(TASK).failure_injected, true);

    const second = maybeInjectControlledFailure(TASK);
    assert.equal(second, null);
  });
});
