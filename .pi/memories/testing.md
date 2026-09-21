# Testing

Run from the Test Runner window. Two suites in two assemblies:

## `Game.Runtime.Tests` — Edit Mode

The pure rule engines, tested directly. R3 ticks are frame-driven and inert in Edit Mode,
so these tests drive rules without a running scene:

| Test | Covers |
|---|---|
| `CardInventoryTest` | grant-once, silent restore |
| `InteractionServiceTest` | id routing + actor derivation from dialogue/chapter keys |
| `DialogueServiceTest` | resolution, chapter-key fallback, reward on finish |
| `ChapterStateTest` | gate conditions, auto-advance |

## `Game.Runtime.PlayTests` — Play Mode

Character/camera have no seams (production code is frozen), so they run on a live scene:
a self-built player (`Controller` + `CharacterController` + `PlayerInput` +
`InputMonitor`) is driven — move, jump, camera pitch clamps, body yaw.

> Both suites take real seconds (the play-mode tests wait ~2s each for physics/look accumulation).
> The yaw test asserts the body actually rotates under `Look.x` — if the built-in
> `CharacterController` starts fighting `transform.Rotate` in a future Unity upgrade, that test
> fails first and the rotation path needs revisiting.
