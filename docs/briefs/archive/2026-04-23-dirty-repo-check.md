---
id: dirty-repo-check
type: brief
status: approved
created: 2026-04-23
updated: 2026-04-23
owner: product-analyst
related:
  spec: docs/specs/archive/2026-04-23-dirty-repo-check.md
  plan: null
---

# Dirty Repository Check Not Enforced — Brief

## Problem

The `git-flow-version` tool is supposed to detect when a repository has uncommitted changes (staged or unstaged) and refuse to generate a version number, requiring the user to pass `--force` to proceed. However, the dirty repo check appears to be broken: the tool successfully generates a version even when files are changed, without requiring the force flag.

This breaks the tool's guarantee that versions can be reliably traced back to exact commits. Users may unknowingly version code based on a "dirty" repository state, making reproducibility and auditability of version numbers impossible.

## Users affected

- **Library/tool consumers:** Users who depend on version numbers to identify exact deployments or builds; dirty versions are useless for traceability.
- **CI/CD pipelines:** Automated build systems that rely on the tool to gate versioning on clean repository state; a silent failure undermines release discipline.
- **Git-flow practitioners:** Teams following git-flow conventions expect strict tooling enforcement; a non-functional guard violates the convention.

## Scope

**In scope:**

- Identifying why the dirty repo check is not working (code defect investigation).
- Fixing the logic so that `git-flow-version` correctly detects any unstaged or staged changes and blocks version generation without `--force`.
- Ensuring the fix is validated with tests covering both staged and unstaged file changes.
- Verifying the `--force` flag bypasses the check as intended.

**Out of scope:**

- Changing the semantics of what "dirty" means (e.g., untracked files are already excluded by design; not reconsidering that choice).
- Adding new flags or configuration options to control the dirty check behavior.
- Updating user-facing documentation beyond confirming existing docs match the corrected behavior.

## Acceptance criteria

- The tool exits with a non-zero status and a clear error message when the repository has staged or unstaged changes, _without_ the `--force` flag.
- The tool successfully generates a version when the same repository state is used with the `--force` flag.
- A test case exists that verifies the tool rejects a dirty repo (both staged and unstaged scenarios).
- A test case exists that verifies `--force` overrides the check.

## Success metrics

- Zero false negatives: the dirty check always catches actual uncommitted changes.
- Tool reliability: users can trust that a version number always means the repo was clean (or explicitly forced).

## Open questions

- What are the current implementation details of the dirty check? (Code review will answer this.)
- Are there existing tests for the dirty check, and why are they not catching the regression? (Code review will answer this.)

## Notes

_2026-04-23: Awaiting approval before architect drafts spec._
