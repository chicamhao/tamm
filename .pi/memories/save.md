## Save format is PlayerPrefs, not a file

## Context

The save currently persists cards (comma list), conducted conversations, and the current
chapter. On desktop this lands in the Windows registry via PlayerPrefs.

## Decision

Stay on PlayerPrefs for now. Treat it as a design constraint, not a file format. The
day saves grow (save slots, cloud, mobile), swap `Services.Progress` internals for a
real file format — callers don't change.

## Consequences

- Zero file I/O, no path/platform handling to ship.
- Not user-inspectable, and regains nothing between OSes without rework.
- The swap is localized to `ProgressStore` when it comes.
