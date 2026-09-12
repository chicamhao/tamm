# uni-tam — chapter-driven, first-person narrative adventure

Built on the **starter** frame — a thin, copy-pasteable foundation for Unity projects.
Everything is a **pattern**, not a monolithic engine: DI via composition root, an input hub
bound to the InputSystem asset, R3-reactive services, config-as-ScriptableObject,
PlayerPrefs saves.

The loop: **interact with objects to collect cards, use cards to converse with NPCs,
progress chapters, and reshape the world around you.**

The collect-objects scaffold in `Game.Session`/`Game.Interaction`/`Game.UI` is the
**reference game** — copy its shape (interact → service rule → reactive UI), then rename
into the card system.

## Stack

- Unity (2022.3+) with the **Input System (New)** — `activeInputHandler: 1` in ProjectSettings
- [R3](https://github.com/Cysharp/R3) (NuGet + R3.Unity) — reactive state, the game loop
- TextMeshPro, Cinemachine (kit), URP (rendering)

## Assemblies

| Assembly | Folder | Contents |
|---|---|---|
| `Game.Core` | `Assets/Scripts/Framework/` | Pure-C# DI container. Zero deps — reuse first. |
| `Game.Runtime` | `Assets/Scripts/Runtime/` | Everything else: composition root, services, input hub, character controller, UI leaves, interaction. |
| `Game.Runtime.Tests` | `Assets/Scripts/Tests/` | Edit-mode NUnit tests (Test Runner). |

## Layers

```
Game.Core      Container (DI, pure C#)
Game.Core      PauseState / ProgressStore / InputSettings — services owned by Booster
Game.Input     InputMonitor — THE input contract: actions bound from the asset, cursor, hub state
Game.Character Controller / Pusher — kit character controller (consumes hub)
Game.UI        GameHud / SettingsMenu — leaf MonoBehaviours; the only file allowed to touch Services
Game.Interaction Collectible / ClickCollector — reference-game interaction
```

Invariant: **only** leaves read `Game.Core.Services`; services never reference leaves;
`InputMonitor` is the only file that touches input devices or `Cursor`.

## Scene setup (reference: Playground)

1. `Booster` (any persistent object) + assign `GameSettings` asset (Assets → Create → Game/Settings).
2. Player with `InputMonitor` + `Controller`. `InputMonitor._actions` optional — falls back to
   `InputSystem.actions`, registered in `ProjectSettings/EditorBuildSettings.asset` →
   `com.unity.input.settings.actions` (points at `Assets/InputSystem_Actions.inputactions`).
3. `ClickCollector` (+ assign the player's `InputMonitor`) and up to N `Collectible`s (need colliders).
4. `GameHud` with 3 TMPro texts; `SettingsMenu` (+ `_input` ref, `_panel`, slider, save/load/new-game buttons).

## Patterns

- **Add a service:** class in `Gem.Core`-style namespace → `Provide` in `Booster.Awake` (deps first)
  → expose on `Booster` → add accessor on `Services` (if leaves need it) → dispose via `IDisposable`.
- **Add an action:** add name + bindings in `Assets/InputSystem_Actions.inputactions` (Player map)
  → bind in `InputMonitor.Start` → read: state fields (`Move`, …), edge reads (`GetPauseInputDown`), or new getter.
- **Pause:** `Services.Pause.Toggle()` → `Time.timeScale` freezes simulation; cursor unlock through
  `InputMonitor.SetCursorState(false)` freezes gameplay input via `CanProcessInput`.
- **Save/load:** `Services.Progress.Save()/Load()` (PlayerPrefs counter MVP — swap for a file
  format the day a save holds cards, dialogue flags, and chapter progress).
- **Settings persistence:** `InputSettings` seeds from `GameSettings`, writes PlayerPrefs on change.

## Conventions

`conventions.yaml` at the root — sealed-by-default, `Assert.IsNotNull` at init, `?.Invoke()` events.

## Tests

Test Runner → `Game.Runtime.Tests` (Edit Mode). Container behaviors + session rules
(`GameSession.Evaluate`/`RestoreProgress`/`Reset`). R3 ticks are frame-driven and inert in Edit Mode,
so session tests exercise rules directly.

Character/camera have no seams (production code is frozen), so they run as **Play Mode** tests in
`Game.Runtime.PlayTests`: a self-built player (`Controller` + `CharacterController` + `PlayerInput` +
`InputMonitor`) is driven on a live scene — move, jump, camera pitch clamps, body yaw.

> Both suites take real seconds (the play-mode tests wait ~2s each for physics/look accumulation).
> The yaw test asserts the body actually rotates under `Look.x` — if the built-in
> `CharacterController` starts fighting `transform.Rotate` in a future Unity upgrade, that test
> fails first and the rotation path needs revisiting.

## Porting checklist

1. Reread `InputMonitor.cs` — the whole input contract. Remap bindings in the actions asset.
2. Decide cursor/sensitivity per platform (already centralized).
3. Swap `Time.timeScale` pause for an OS-level pause if needed.
4. Everything else is engine-agnostic C# + R3.