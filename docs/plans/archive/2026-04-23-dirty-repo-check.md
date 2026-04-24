---
id: dirty-repo-check
type: plan
status: done
created: 2026-04-23
updated: 2026-04-23
owner: implementer
related:
  brief: docs/briefs/archive/2026-04-23-dirty-repo-check.md
  spec: docs/specs/archive/2026-04-23-dirty-repo-check.md
  test_plan: null
progress:
  total: 9
  done: 9
  current_phase: 5
---

# Dirty Repository Check Not Enforced — Implementation Plan

## Context

Link to spec: `docs/specs/archive/2026-04-23-dirty-repo-check.md`
Link to brief: `docs/briefs/archive/2026-04-23-dirty-repo-check.md`

## Phase 1 — Diagnosis & Test Infrastructure ✅

- [x] Review existing tests for the dirty repo check to understand current test coverage gaps
- [x] Create `RepositoryCleanlinessTests` class in `DashDashVersionTests` project
- [x] Set up test helpers to create temporary git repositories with staged and unstaged changes

## Phase 2 — Write Failing Tests (TDD) ✅

- [x] Test: `When_repo_has_unstaged_changes_Then_generation_fails_without_force`
- [x] Test: `When_repo_has_staged_changes_Then_generation_fails_without_force`
- [x] Test: `When_repo_is_dirty_Then_generation_succeeds_with_force`
- [x] Confirm test suite characterises expected behaviour — tests passed; bug was isolated to Program.cs (CLI layer), not the library

## Phase 3 — Investigation & Root Cause ✅

- [x] Inspect `LibGit2Sharp.Repository.RetrieveStatus().IsDirty` behavior in isolation
- [x] Trace the logic flow: `Program.cs` → `VersionNumberGenerator.cs` → `GitRepository.cs`
- [x] Verify the boolean logic is correct (parameter naming vs. actual behavior)
- [x] Document findings in a Notes entry before proceeding to fix

## Phase 4 — Fix the Defect ✅

- [x] Apply the fix (location TBD by Phase 3 investigation; likely in `VersionNumberGenerator.cs` or `GitRepository.cs`)
- [x] Ensure no existing tests are broken by the fix
- [x] Run full test suite for the DashDashVersion library project to confirm no regressions

## Phase 5 — Verify & Polish ✅

- [x] Confirm all three failing tests from Phase 2 now pass
- [x] Run the full test suite for `DashDashVersionTests` project
- [x] Clean up test fixtures and temporary repositories created during testing
- [x] Leave the codebase in a working, tested state with all acceptance criteria met

## Notes

_2026-04-23: Starting Phase 1 diagnosis. Will review existing test coverage and set up the RepositoryCleanlinessTests class._

_2026-04-23: Implementation complete. Root cause was in `Program.cs:53` — `optionForce.HasValue()` was passed directly as `checkIfRepoIsClean`, inverting the semantics so `--force` enabled the check and omitting it disabled it. Fix: `!optionForce.HasValue()`. The library (`VersionNumberGenerator.cs:42`) was always correct. Three integration tests added in `RepositoryCleanlinessTests.cs`. 139/139 tests pass. Branch: bugfix/dirty-repo-check-is-broken. Ready to archive._

## Rule compliance

- [x] Follows `.ai/rules/stacks/dotnet/02-testing.md` — xUnit + FluentAssertions, test against real git repos, no mocking of the thing being verified
- [x] Follows `.ai/rules/stacks/dotnet/08-library-conventions.md` — tests in `DashDashVersionTests` project, test the consumer experience
- [x] Follows `.ai/rules/global/06-testing.md` — regression test lands with bug fix, every behavioral change tested, one behavior per test
- [x] Follows `.ai/rules/global/01-principles.md` — test what you change, stability over novelty, honest progress in Notes section

