# uni-tam — chapter-driven, first-person narrative adventure

Built on the **starter** frame — a thin, copy-pasteable foundation for Unity projects.
Everything is a **pattern**, not a monolithic engine: DI via composition root, an input hub
bound to the InputSystem asset, R3-reactive services, config-as-ScriptableObject,
PlayerPrefs saves.

The loop: **interact with objects to collect cards, use cards to converse with NPCs,
progress chapters, and reshape the world around you.**

The card-interaction scaffold in `Game.Core`/`Game.Interaction`/`Game.UI` is the
**reference loop** — copy its shape (interact → service rule → reactive UI): every
`Interactable` carries an id; `InteractionService` grants known card ids and opens the
card selection menu for known actor ids.

## Designer & writer onboarding

Content is text files first — you never touch script code.

**Your daily loop:**
1. **Edit a YAML file** under `Assets/Settings/Game/YAML/` while Unity is open
   (also valid in VS Code — refresh Unity after).
   - `cards.yaml` — the collectible cards: `card_id`, `display_name`, `description`, `target_actor_ids`.
   - `dialogues.yaml` — NPC reactions, keyed by card + actor: `card_id` is `cardId_actorId`
     (add `_2` → that line only plays in chapter 2). Each line: `text`, `duration`, optional `expression_id`.
   - `chapters.yaml` — per-actor scene state per chapter (`spawn_point_id`, `is_visible`), plus
     `chapter_advances` gates that unlock the next chapter from progress (e.g. `owns_card` / `had_conversation`).
   - `expressions.yaml` — facial-expression morph targets (id-referenced by dialogue lines).
2. **Import:** `Assets → Import Content from YAML`. A console warning appears if you edit YAML
   without re-importing.
3. **Play test** the Playground scene. World tags show what each object/NPC is; aim glow marks
   what's interactable; **F2** opens the dev chapter stepper (↑/↓ moves chapters, gates bypassed);
   the Settings gear has Save / Load / New Game (boot auto-restores your save).
4. **Iterate ids:** object ids grant cards, actor ids (e.g. `cam`, `sm`) open the card menu. New
   card ids in `cards.yaml` just work; new *actor* ids also just work (derived from dialogue keys).

**Scene wiring** (once per object/NPC, in the Inspector): an `Interactable` with the id + a
collider; optional `Outline` (aim glow) and `WorldTag` (3D label above). UI leaves
(`CardSelectionMenu`, `DialoguePanel`, …) live under the Canvas and hold font/layout config.

## Stack

- Unity **6000.7.0a6** (alpha) with the **Input System (New)** — `activeInputHandler: 1` in ProjectSettings
- [R3](https://github.com/Cysharp/R3) (NuGet + R3.Unity) — reactive state, the game loop
- [UniTask](https://github.com/Cysharp/UniTask) (NuGet) — async flows (installed; pacing currently lives in leaf `Update`s)
- [YamlDotNet](https://github.com/yaml) (NuGet) — the content pipeline's YAML
- TextMeshPro, Cinemachine, URP (rendering), QuickOutline (aim highlight)
- PlayerPrefs (Windows registry) save file — design, not a file format
- Manifest trimmed to used packages/modules (see `Packages/manifest.json`)

## Assemblies

| Assembly | Folder | Contents |
|---|---|---|
| `Game.Core` | `Assets/Scripts/Framework/` | Pure-C# DI container. Zero deps — reuse first. |
| `Game.Runtime` | `Assets/Scripts/Runtime/` | Composition root, services, input hub, interaction, UI leaves, content data classes (`Game.Content`). |
| `Game.Editor` | `Assets/Scripts/Editor/` | Designer tools: YAML ⇄ ScriptableObject content pipeline (cards/dialogues/chapters/expressions) + `ChapterDebug` (persisted chapter bump). |
| `Game.Debug` | `Assets/Scripts/Debug/` | Dev overlays (`DebugMenu`, F2 chapter stepper); editor/dev builds only. |
| `QuickOutline` | `Assets/QuickOutline/` | Vendored `Outline` component (aim highlight). |
| `Game.Runtime.Tests` | `Assets/Scripts/Tests/` | Edit-mode NUnit tests (Test Runner). |

## Layers

```
Game.Core       Container (DI, pure C#)
Game.Core       PauseState / ProgressStore / CardInventory / DialogueService / ChapterState / InputSettings — services owned by Bootstrapper
Game.Content    CardSettings / DialogueSettings / ChapterSettings / ExpressionSettings — id-keyed content dictionaries (imported from YAML)
Game.Input      InputMonitor — THE input contract: actions bound from the asset, cursor, hub state
Game.Character  Controller / Pusher — kit character controller (consumes hub)
Game.UI         SettingsMenu / CardSelectionMenu / DialoguePanel / ToastOverlay / TutorialPrompt — leaf MonoBehaviours; the only files allowed to touch Services
Game.Interaction Interactor / Interactable / WorldTag / ChapterSpawner — crosshair interact, aim highlight, world labels, chapter placement
Game.Debug      DebugMenu — F2 dev overlay (chapter stepper)
```

Invariant: **only** leaves read `Game.Core.Services`; services never reference leaves;
`InputMonitor` is the only file that touches input devices or `Cursor` (debug overlays
violate it on purpose — see `DebugMenu`/`ToastOverlay` ponytail notes).

## Scene setup (reference: Playground)

1. `Bootstrapper` (any persistent object) + assign `GameSettings` (Assets → Create → Game/Settings).
   Its Content refs (`Cards`/`Dialogues`/`Chapters`) are generated by the YAML import.
2. Player with `InputMonitor` + `Controller`. `InputMonitor._actions` optional — falls back to
   `InputSystem.actions`, registered in `ProjectSettings/EditorBuildSettings.asset` →
   `com.unity.input.settings.actions` (points at the `InputSystem_Actions.inputactions` asset
   under `Assets/Settings/Input/`). Add `Interactor` (+ `_input` → the player's InputMonitor).
3. Every interactable is an `Interactable` with an `_id` (collider + id). The id routes via
   `InteractionService`: registered card ids grant, registered actor ids open the card menu.
   Optional feedback per entity: `Outline` (aim glow) and `WorldTag` (3D label above, TMPro child).
4. UI leaves: `SettingsMenu`; `CardSelectionMenu` (panel + button template + `_cards` + `_input`);
   `DialoguePanel` (speaker/line TMP texts + `_input`); `ToastOverlay` (card notifications);
   `DebugMenu` (F2 dev overlay); `TutorialPrompt` (build-only first-run hint).

## Patterns

- **Add a service:** class in `Gem.Core`-style namespace → `Provide` in `Bootstrapper.Awake` (deps first)
  → expose on `Bootstrapper` → add accessor on `Services` (if leaves need it) → dispose via `IDisposable`.
- **Add an action:** add name + bindings in the `InputSystem_Actions.inputactions` asset (Player map)
  → bind in `InputMonitor.Start` → read: state fields (`Move`, …), edge reads (`GetPauseInputDown`), or new getter.
- **Content pipeline (designer tools):** edit `Assets/Settings/Game/YAML/{cards,dialogues,chapters,expressions}.yaml`
  → `Assets → Import Content from YAML` populates four id-keyed dictionaries — `CardSettings.asset`,
  `DialogueSettings.asset`, `ChapterSettings.asset` (incl. `chapter_advances` gates), `ExpressionSettings.asset`.
  Export is the reverse. Staleness check warns when YAML changes without an import.
- **Gameplay loop:** every `Interactable` carries an id → `InteractionService` grants cards / opens the
  card menu (`CardSelectionMenu`) → `DialogueService` plays the keyed entry (`cardId_actorId[_chapter]`,
  `DialoguePanel` shows it) → conversations + grants publish progress → `ChapterState` advances when a
  gate's conditions all hold → `ChapterSpawner` re-places actors.
- **Pause:** `Services.Pause.Toggle()` → `Time.timeScale` freezes simulation; cursor unlock through
  `InputMonitor.SetCursorState(false)` freezes gameplay input via `CanProcessInput`.
- **Save/load:** `Services.Progress.Save()/Load()` persisting cards (comma list), conducted
  conversations, and the current chapter (PlayerPrefs; swap for a file format the day saves grow).
  Bootstrapper auto-loads on boot; `SettingsMenu` exposes Save/Load/New Game.
- **Settings persistence:** `InputSettings` seeds from `GameSettings`, writes PlayerPrefs on change.
- **Dev overlays:** `Game.Debug`-only `DebugMenu` (F2, arrow keys step chapters); `ToastOverlay` (card
  notifications); `TutorialPrompt` (first-run hint, exported builds only); `ChapterDebug` (batch-mode
  `-executeMethod` chapter bump for CI). IMGUI leaves need no scene wiring — just the component.

## Conventions

`conventions.yaml` at the root — sealed-by-default, `Assert.IsNotNull` at init, `?.Invoke()` events.

## Tests

Test Runner → `Game.Runtime.Tests` (Edit Mode). The pure rule engines:
`CardInventoryTest` (grant-once, silent restore), `InteractionServiceTest` (id routing + actor derivation
from dialogue/chapter keys), `DialogueServiceTest` (resolution, chapter-key fallback, reward on finish),
`ChapterStateTest` (gate conditions, auto-advance). R3 ticks are frame-driven and inert in Edit Mode,
so these tests drive rules directly.

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