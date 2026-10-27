# How the plan works

| File | Holds |
|------|-------|
| `ROADMAP.md` | Phases and their gates, mapped to GitHub milestones |
| `BOARD.md` | Who works on what right now; mirrored on the GitHub board |
| `E<n>-<name>.md` | One epic: its task cards |

**Card format** (in an epic file):

```
### F-04 · Short title
- Issue: #4 · Size: S · Status: Ready
- Goal: one sentence on the outcome.
- Acceptance:
  - [ ] observable, testable criterion
- Files: paths the task may touch
- Notes: links to decisions, dependencies (`after S-01`)
```

- **IDs:** epic letter + number, never reused: `F-` Foundation (P0), `S-` SQLite PoC (P1), `X-` Future.
- **Size:** S / M / L (D-14). `L` is split before it becomes `Ready`.
- **Status:** `Backlog` (idea, incomplete) → `Ready` (card complete, dependencies done) → `In Progress`
  (assigned on BOARD.md) → `Review` (PR open) → `Done` (merged, acceptance checked).
- **Who edits:** only the main agent and the owner edit plan files (GUARDRAILS.md). Workers report through their PR.
- **GitHub mirror (D-03):** each card or epic links an issue; issue milestone = phase, labels = type/priority/area;
  board Status = card Status. When they disagree, the plan file wins and the main agent updates GitHub.
- **Done cards** stay in the epic file (with the PR number) until the epic closes; then the file gets a one-line summary.
