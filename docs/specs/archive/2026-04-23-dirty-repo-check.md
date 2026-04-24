---
id: dirty-repo-check
type: spec
status: approved
created: 2026-04-23
updated: 2026-04-23
owner: architect
related:
  brief: docs/briefs/archive/2026-04-23-dirty-repo-check.md
  plan: docs/plans/archive/2026-04-23-dirty-repo-check.md
  decisions: []
---

# Dirty Repository Check Not Enforced — Technical Spec

## Scope

This spec covers investigation and repair of the `RepoIsDirty` detection mechanism in the `git-flow-version` tool. The check should prevent version generation when the repository has uncommitted changes (staged or unstaged) unless the `--force` flag is used. The fix is scoped to the repository abstraction layer and integration tests that validate the behavior.

Out of scope: changing the definition of "dirty" to include untracked files (this is by design); adding configuration options; public API changes.

## Approach

### 1. Root Cause Analysis

The dirty repo check flows through:
1. **Program.cs** (entry point): Parses `--force` flag and passes as boolean to `GenerateVersionNumber`
2. **VersionNumberGenerator.cs**: Receives the force flag and checks `repo.RepoIsDirty` against it
3. **GitRepoReader.cs**: Exposes `RepoIsDirty` property
4. **GitRepository.cs**: Computes `IsDirty` from `repository.RetrieveStatus().IsDirty` (LibGit2Sharp)

The check at `VersionNumberGenerator.cs` line 42 reads:
```csharp
if(checkIfRepoIsClean && repo.RepoIsDirty)
    throw new InvalidDataException("...");
```

The bug was in `Program.cs` line 53: `optionForce.HasValue()` was passed directly as `checkIfRepoIsClean`, inverting the semantics — `--force` enabled the check instead of disabling it. The fix passes `!optionForce.HasValue()` so that omitting `--force` enforces the check and supplying it bypasses it.

### 2. Testing Strategy

Write integration tests against a real git repository (not mocked) to:
- **Verify the bug:** Create a test repository with both staged and unstaged changes; confirm the tool currently returns a version without `--force`
- **Validate the fix:** After fixing, confirm the tool rejects the dirty repo and requires `--force`
- **Confirm force override:** Verify `--force` allows version generation on a dirty repo

Test file organization: Add tests to `DashDashVersionTests` project in a new test class `RepositoryCleanlinessTests`.

### 3. Implementation Plan

1. **Write failing tests** that reproduce the bug:
   - `When_repo_has_unstaged_changes_Then_generation_fails_without_force`
   - `When_repo_has_staged_changes_Then_generation_fails_without_force`
   - `When_repo_is_dirty_Then_generation_succeeds_with_force`

2. **Run tests** to confirm they fail (proving the bug exists)

3. **Investigate** the actual behavior of `LibGit2Sharp.Repository.RetrieveStatus().IsDirty`:
   - Does it correctly detect staged changes?
   - Does it correctly detect unstaged changes?
   - Is there a known issue with how it's being called?

4. **Fix the defect** (location TBD after investigation, likely in `VersionNumberGenerator.cs` or `GitRepository.cs`)

5. **Verify tests pass** with the fix in place

## Data model

No new data structures. The fix operates on existing state:
- `repo.RepoIsDirty` boolean property (existing)
- `--force` CLI flag (existing)

## Interfaces

**Public API (no changes expected):**
```csharp
public static VersionNumber GenerateVersionNumber(
    string path,
    string branch,
    bool checkIfRepoIsClean)
```

The `checkIfRepoIsClean` parameter is correctly derived from the `--force` flag in `Program.cs`. The naming is inverted (when `--force` is given, `checkIfRepoIsClean` is false), which is semantically correct but must be validated in tests.

## Alternatives considered

1. **Mock LibGit2Sharp in unit tests:** Rejected. The whole point of this check is to verify that LibGit2Sharp correctly reports repo state. Mocking defeats the purpose. Integration tests against a real repo are required.

2. **Extend the dirty check to include untracked files:** Rejected. Out of scope; untracked files are deliberately excluded by design.

3. **Add a flag to customize the dirty definition:** Rejected. Out of scope; not part of the bug fix.

## Architectural decisions

None at this stage. The architecture is already sound. The investigation will determine if the bug is in the logic (e.g., inverted boolean) or in how LibGit2Sharp's `IsDirty` property behaves.

## Open questions

- Does `LibGit2Sharp.Repository.RetrieveStatus().IsDirty` correctly detect both staged and unstaged changes? (Answer: code inspection + test)
- Is there a known issue with the integration between LibGit2Sharp and the Windows file system, or specific git configurations that break the check? (Answer: investigation during implementation)
- Are there existing tests for the dirty check, and if so, why are they not catching this regression? (Answer: code review will reveal)

