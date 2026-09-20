# UI Toolkit Migration Plan — uGUI/IMGUI/TMP → UI Toolkit only

> Status: **proposal (reviewed scope decisions)** · Branches/commits at each phase boundary
> Scope decisions (2026-09-20 session):
> 1. **Include DebugMenu** — target is *zero* IMGUI and *zero* uGUI/TMP in the repo.
> 2. **Full purge** — `com.unity.ugui` and TextMeshPro removed entirely, including the
>    3D `WorldTag` labels (migrated to screen-space UITK `Label`s).
> 3. **Rat icons restyled**, not ported 1:1 — native UITK shapes (border-radius discs,
>    unicode arrows, animated ripple). No mesh generation.
> 4. Plan saved to this doc.

---

## 1. Goal & non-goals

**Goal:** every piece of UI in the project ships as UI Toolkit — runtime game UI, IMGUI
leaves, dev overlays. No `UnityEngine.UI`, no `TMPro`, no `OnGUI`, no `Canvas`/`RectTransform`
in scenes, and `com.unity.ugui` removed from the manifest.

**Non-goals (out of scope for now):**
- Editor tooling — already UI Toolkit by default; no change required.
- Reskinning/motion work beyond what's needed to look intentionally "native".
- Performance optimization passes beyond removing the uGUI pipeline.

---

## 2. Current state inventory

### Code using uGUI (`UnityEngine.UI`)
| File | What it owns |
|---|---|
| `Runtime/UI/SettingsMenu.cs` | pause menu: `Slider`, 3 `Button`, 2 TMP texts; R3 → save/load/new game |
| `Runtime/UI/CardSelectionMenu.cs` | clones a button template per owned card; R3 `CardSelectionRequested` |
| `Runtime/UI/DialoguePanel.cs` | 2 TMP texts, R3-driven show line / advance |
| `Runtime/Minigames/Rat/UI/ActionPromptPanel.cs` | 662-line runtime-built HUD (chips, prompt, HUD texts, banner); ~30 serialized layout fields reapplied in `LateUpdate` |
| `Runtime/Minigames/Rat/UI/PromptIcon.cs` | abstract `MaskableGraphic`, procedural `OnPopulateMesh` (fills + anti-aliasing skirts) |
| `Runtime/Minigames/Rat/UI/ArrowIcon.cs`, `TapIcon.cs`, `CircleIcon.cs` | subclasses: swipe arrows, hand+ripple, chip discs |
| `Runtime/Minigames/Rat/RatManager.cs` | 5 TMP texts, `Canvas` lookup, `ActionPromptPanel.CreateIn` path |

### Code using IMGUI (`OnGUI`)
| File | What it owns |
|---|---|
| `Runtime/UI/ToastOverlay.cs` | card-granted toast stack (label list, fade) |
| `Runtime/UI/TutorialPrompt.cs` | build-only first-run hint box |
| `Debug/DebugMenu.cs` | F2 chapter stepper (dev-only, `Game.Debug` asmdef, dev/editor builds only) |

### TMP usage outside UI
| File | What it owns |
|---|---|
| `Runtime/Interaction/WorldTag.cs` | 3D label above `Interactable`s (`TMP_Text` child, follows main camera) |

### Scenes / assets
- `Playground.unity` — uGUI Canvas subtree (34 RectTransforms, Slider/Buttons/Images/TMP).
- `Rat.unity` — uGUI Canvas subtree (8 RectTransforms).
- No `UIDocument`, `EventSystem`, or UITK `PanelSettings` in any scene yet.
- `Assets/TextMesh Pro/` (fonts + `TMP Settings.asset`) — the only font assets: `LiberationSans.ttf`.
- Existing UITK precedent (the pattern to extend): `Runtime/Mobile/UI/` —
  `TouchScreenInputUI.uxml`, `ToucScreenInputStyle.uss`, `TouchScreenInputPanelSetting.asset`
  (ScaleWithScreenSize, 1600×900), plus `Runtime/Input/TouchscreenInput.cs`
  (MonoBehaviour controller + `UIDocument` + `Q<>` lookups + safe-area handling).
- Project uses Input System only (`activeInputHandler: 1`).

---

## 3. Target architecture

**Controllers stay MonoBehaviours.** Each screen = one `VisualTreeAsset` (UXML) + one USS
+ one controller MonoBehaviour attached next to the UIDocument that grabs elements in
`Start` via `root.Q<>` — exactly the `TouchscreenInput` pattern. Name elements `camelCase`.

**One shared UI host.** A single UIDocument GameObject per level (alive in the core
scene / added to level scenes like other leaves) hosting *all* runtime UI:
- one shared `RuntimePanelSettings` (ScaleWithScreenSize 1600×900, overlaid on main render);
- `theme.uss` — the project palette: Ink `#0F3E9E`, Fill `#9ED9FF`, ChipIdle `#8CD6E6`-adjacent,
  ChipDone green, from `ActionPromptPanel`'s colours;
- `common.uss` — buttons, panels, labels, fade utility classes.

**One fiber per concern:** settings/card-menu/dialogue as `VisualElement` "screens" toggled by
`style.display` (visibility driven by the existing R3 services — services untouched).

**Text:** UITK uses its own text stack (TextCore). Register `LiberationSans.ttf`
(via UITK `TextSettings` / font asset) so all labels render with the project's font.
No TMP anywhere after the purge.

**Procedural icons retheme:** replaced by native UITK primitives —
- **Chips / circles / discs:** `border-radius: 50%` + `background-color` on plain VEs.
- **Arrows:** unicode glyphs (`↑`, `↗`, …) in `TextElement`, or a rotated `border-right/top`
  chevron; sizes/colours from USS, no per-vertex code.
- **Ripple:** a `border-radius: 50%` VE animating `width/height` + `opacity` via
  `schedule.Execute` (unscaled — must keep animating while paused at `timeScale 0`).

**3D world labels (`WorldTag` → `WorldLabel`):** migrate the TMP label into the shared
panel as a screen-space `Label`; a small controller projects the object's world position
→ screen each `LateUpdate` (`Camera.main` — the label already captures the camera today),
flips/hides when behind the camera, and writes `style.translate`. This matches the existing
world-badge math in `ActionPromptPanel`.

---

## 4. Phases (each ends runnable + green; commit per phase)

### Phase 0 — UITK runtime foundation *(no gameplay change)*
- Create: `RuntimePanelSettings.asset`, `theme.uss`, `common.uss`, UI-host GameObject pattern.
- Register `LiberationSans.ttf` for UITK text; confirm TextElement rendering quality.
- Build a throwaway test screen proving: UITK renders over the 3D world, pointer hover/click
  works with Input System (no EventSystem needed), cursor lock/unlock via `InputMonitor`
  behaves, safe area respected.
- **Verify:** test screen in-editor + Web build; `InputMonitor` actions and UITK input do not
  conflict; text looks right.

### Phase 1 — Playground leaves (`SettingsMenu`, `CardSelectionMenu`, `DialoguePanel`)
- Port each to UXML+USS+controller. `Slider` → UITK `Slider`; buttons → `Button`; TMP →
  `Label`. `CardSelectionMenu` replaces template cloning with buttons built on `Open`
  (or a `ListView` if it keeps growing). R3 wiring unchanged.
- Remove the Playground Canvas subtree; add the UI host.
- **Verify:** pause toggle works at `timeScale 0`, save/load/new-game, dialogue advance on
  Interact, card select → dialogue plays, F2 dev overlay still opens (DebugMenu still IMGUI
  *this phase* — coexists briefly).

### Phase 2 — IMGUI leaves → UITK (`ToastOverlay`, `TutorialPrompt`, `DebugMenu`)
- `ToastOverlay` → stacked `Label`s with fade; `TutorialPrompt` → styled box (keeps the
  `Application.isEditor` build-only guard); `DebugMenu` → a small UITK screen (still F2-only,
  still in `Game.Debug` asmdef).
- **Verify:** card grants toast; build-only prompt in non-editor play; F2 stepper works.
- Exit: **zero `OnGUI` in the repo.**

### Phase 3 — Rat HUD & icons restyle (`ActionPromptPanel`, `ArrowIcon`, `TapIcon`, `CircleIcon`, `PromptIcon`, `RatManager`)
- Layout → `RatHud.uxml`/`RatHud.uss`: chip row (top-center flex), prompt block (center),
  HUD texts (top-left / bottom-center), result banner (center). The ~30 serialized offsets
  collapse into USS layout; keep a small options struct only for what truly needs live tuning.
- Icons → native primitives per §3 (no `OnPopulateMesh`). Animation via unscaled scheduler.
- Keep the world-badge behaviour (`LateUpdate` projection → `style.translate`) and the
  `RatManager` ↔ controller contract (Step state, round/hearts/instruction/progress text).
  Drop the `Canvas` lookup and `CreateIn` path; controller queries the document.
- **Verify:** full rat loop (throw → tap → catch → result); badge rides the ball; chips
  animate while paused; layouts correct at 16:9 and narrow/safe-area resolutions.

### Phase 4 — `WorldTag` → screen-space + package purge
- Rework `WorldTag.cs` into the shared-panel `WorldLabel` controller (§3). Verify labels
  track actors and hide correctly.
- Delete `Assets/TextMesh Pro/`, `TMP Settings.asset`; strip the Rat scene Canvas; remove
  `com.unity.ugui` from `Packages/manifest.json` (+ sweep `packages-lock.json`).
- Remove `using UnityEngine.UI;`/`using TMPro;` from all runtime code; compile to zero errors;
  delete `Game.Content`/`Game.Input` TMP leftovers if any.
- **Verify:** clean reimport + full build on Windows and Web; playthrough of Playground + Rat.

### Phase 5 — Consolidation & regression net
- Sweep scenes for any remaining `RectTransform`/`Canvas`/`GraphicRaycaster`.
- Add a Play-Mode smoke test: instantiate the UI host, assert `Q` element names/state
  (mirrors existing `Game.Runtime.PlayTests`).
- **Verify:** whole game loop (interact → card → dialogue → chapter gate) on PC + Web builds.

---

## 5. Key risks
| Risk | Mitigation |
|---|---|
| UITK + Input System coexistence (no EventSystem) | Phase 0 test screen before any real port |
| Animations at `timeScale 0` (paused menus/HUD) | unscaled scheduled callbacks; verify in Phase 1/3 |
| `com.unity.ugui` removal breaking hidden deps | Phase 4 full clean build + Web/Windows check; re-add is a manifest revert |
| Text look/auto-size drift after TMP removal | LiberationSans registration + visual pass in Phase 0/1 |
| World labels losing depth/occlusion feel | `WorldLabel` hide-behind-camera + optional z-order by screen-y in Phase 4 |

## 6. Ordering rationale
Phases are ordered by dependency + risk-reduction: foundations (0) → safest ports (1, 2)
→ visually risky but isolated restyle (3) → the only cross-system change (4) → hardening (5).
The repo keeps a working game at every commit; nothing merges with red tests.

## 7. Decisions log
- 2026-09-20: include `DebugMenu` (zero IMGUI target)
- 2026-09-20: full purge incl. `WorldTag` TMP labels (no `com.unity.ugui` in manifest)
- 2026-09-20: rat icons restyled native (not 1:1 port)
- 2026-09-20 (Phase 4): delete orphaned uGUI-era prefabs (`Template.prefab`,
  `UI_EventSystem.prefab`, `NestedParent_Unpack.prefab`) — nothing instantiates them and
  `com.unity.ugui`/EventSystem vanish with the purge
- 2026-09-20 (Phase 4): do **not** preserve `LiberationSans.ttf` — UITK keeps the accepted
  default font; the §3 font-registration intent is dropped

---

## 8. Execution status (checkpoint, 2026-09-20 EOD)

**Architecture implemented:** one `RuntimeUI` component per level scene (UIDocument host +
shared `PanelSettings`, 1600×900 ScaleWithScreenSize) with per-level roots
`PlaygroundUI.uxml`/`RatUI.uxml`; every screen is UXML+USS; leaves are MonoBehaviours that
query their subtree by name (cached `RuntimeUI.Resolve`, optional serialized ref for additive
scenes). Palette/skins live in `Runtime/UI/*.uss`.

### Done & verified
- **Phase 0** — PanelSettings, `common.uss`, roots, `RuntimeUI`; UITK renders/loads in the
  live player (PlayMode smoke test asserts document + every screen element resolves).
- **Phase 1** — Playground scene rebuilt: uGUI Canvas + `UI_EventSystem` deleted, `RuntimeUI`
  host wired (UIDocument + panel + `PlaygroundUI.uxml`), leaf GOs recreated with
  `SettingsMenu`/`CardSelectionMenu`/`DialoguePanel`/`Toasts` scripts + refs (`_input`→
  PlayerCapsule, `_cards`→ CardSettings); uGUI\*TMPro leaves rewritten to UITK (Slider, Labels,
  Buttons; R3 wiring untouched).
- **Phase 2** — `ToastOverlay` (toast stack + fade), `TutorialPrompt`, `DebugMenu` all UITK;
  zero `OnGUI` left in runtime. `DebugMenu` keeps its `Game.Debug` dev/editor-only asmdef.
- **Tests** — EditMode 22/22 pass. `RuntimeUITest` (PlayMode) passes headless: core boots,
  services provide, host+tree resolve, a real `Cards.Granted` event renders a toast.
- **Entry fix (pre-existing gap)** — `Bootstrapper.unity` (the persistent core scene) was not
  in `EditorBuildSettings`; added as the first scene so builds/players boot services.

### Not started (do next)
- **Phase 5 — consolidation** — scene sweep, PlayMode-suite judgement of the 3 pre-existing
  headless failures (Jump/MoveInput physics tests + `MinigameTest.WinningGrants` fail on clean
  HEAD too — batch-environment, not migration regressions), Windows/Web build check.

### Phase 3 — done (2026-09-20 later session)
- **Rat scene surgery** — headless via `RatSceneSurgery` editor script
  (`Assets/Scripts/Editor/RatSceneSurgery.cs`, `-executeMethod`): stripped `ActionPromptPanel`,
  `resultText`, `roundText`, `InstructionText`, `ProgressText`, `heartsText`, `Canvas`,
  `EventSystem` GOs; rebuilt the `RatManager` component (dropped the five stale TMP text refs,
  re-wired `Ball`); added the `RuntimeUI` root (UIDocument → `RatUI.uxml` on the shared
  `RuntimeUIPanelSettings`, mirroring the Playground host field-for-field) and the `RatHud`
  leaf with its `_runtimeUI` serialized to the Rat host (required — Rat is an additive overlay
  over Playground, so two hosts are alive at once). SceneRoots updated; zero `RectTransform`/
  `Canvas`/`EventSystem` left in `Rat.unity`.
- **Old icon files deleted** — `ActionPromptPanel.cs`, `ArrowIcon.cs`, `TapIcon.cs`,
  `CircleIcon.cs`, `PromptIcon.cs` (+ `.meta`); `RatManager` doc comment updated. Zero
  references remain (only the `ArrowIcon` *element name* in `RatUI.uxml`, unrelated to class).
- **New PlayMode gate** — `RatUITest` (`Assets/Scripts/Tests/PlayMode/RatUITest.cs`) boots the
  real entry chain (core → Playground → Rat additive overlay), asserts the Rat document is
  hosted by the Rat scene's own `RuntimeUI`, every `RatHud` subtree resolves by name, the
  rewired `RatManager` drives the HUD to THROW on boot, and the uGUI roots are gone.
- **Tests** — EditMode 22/22 pass. PlayMode 6/9 pass: the same 3 documented pre-existing
  headless failures (Jump/MoveInput physics + `WinningGrants`); the new `RatUITest` is green.

### Phase 4 — done (2026-09-20 later session)
- **`WorldTag` → `WorldLabel`** (`Assets/Scripts/Runtime/Interaction/WorldLabel.cs`): same
  GameObject as the `Interactable`, resolves the level `RuntimeUI` (serialized `_runtimeUI` set
  to the Playground host — additive scenes), appends a `Label` under the shared `WorldLabels`
  layer in `PlaygroundUI.uxml`/`RatUI.uxml`, and `LateUpdate`s the owner's world position via
  `Camera.main.WorldToScreenPoint` → `style.translate` (same math as `RatHud.PlacePrompt`),
  `display: none` while the point is behind the camera. Chip styling in `common.uss`
  (`.world-label`). `WorldTag.cs` deleted — last `using TMPro;` in the repo.
- **Scene surgery** — one-shot `WorldLabelSurgery` editor script (pattern of `RatSceneSurgery`,
  deleted after use since it referenced the removed class): rewired all 6 Playground tags
  (4 cubes + 2 RobotKyle NPCs), copying `_cards` + `_heightOffset` (0.8/1.0), and destroyed the
  6 TMP `Tag` child GOs. `Playground.unity` now has **zero** `RectTransform`/`TextMeshPro`/
  `WorldTag`.
- **Package purge** — `com.unity.ugui` removed from `Packages/manifest.json` and
  `packages-lock.json` (no dependents). `Assets/TextMesh Pro/` deleted entirely (incl.
  `TMP Settings.asset`; *decision*: the LiberationSans TTF was not preserved — UITK keeps the
  verified default font, plan's §3 registration intent dropped). Orphaned prefabs deleted:
  `Template.prefab` (uGUI `Image` + `TextMeshProUGUI`), `UI_EventSystem.prefab` +
  `NestedParent_Unpack.prefab` (both referenced `UnityEngine.EventSystem`, which ships inside
  `com.unity.ugui`; unused). Zero `using UnityEngine.UI;`/`using TMPro;` left in runtime code
  (only UniTask's vendored README prose mentions TMP).
- **Gates** — EditMode 22/22 pass. PlayMode 6/9 pass: the same 3 documented pre-existing
  headless failures (Jump/MoveInput physics + `WinningGrants`); `RuntimeUITest` extended with a
  Phase 4 gate (6 labels under `WorldLabels` when Playground boots) and is green.
  `BuildWebCLI` Web export builds clean (no missing-script/uGUI/TMP errors). Windows build not
  runnable on this macOS host — same caveat as Phase 5.

### Phase 5 — done (2026-09-20 later session)
- **Scene sweep — clean.** Ran `grep` over `Assets/Scenes/*.unity` and every `*.prefab` for
  `RectTransform`/`Canvas`/`GraphicRaycaster`/`UnityEngine.EventSystems.EventSystem`/`TextMeshPro`:
  all zero (the only hits were UITK's own `UnityEngine.UIElements`, a false positive). The
  Playground/Rat/Bootstrapper scenes and the remaining prefabs carry no uGUI/IMGUI/TMP remnants.
- **Regression found & fixed (this is why the smoke suite was red at the start of Phase 5).** Unity
  6.7 split the old `UIDocument` host into a renderer (`PanelRenderer`) and a scriptable-anchor UI
  `UIDocument`. The 6.7 editor auto-migrated the Playground host's `UIDocument` → `PanelRenderer` on a
  save *after* the Phase-4 checkpoint, leaving the host with ***only*** a `PanelRenderer`. Reflecting
  the real API (probed via a throwaway editor/play test, since removed): `PanelRenderer` is a pure
  renderer with **no `rootVisualElement`** — that surface lives only on `UIDocument` — so the whole
  Playground UI tree became un-scriptable and `RuntimeUITest` failed (
  `RuntimeUI requires a UIDocument on the same GameObject`). A `UIDocument`-only host (Rat) still works,
  and re-adding a `UIDocument` beside the `PanelRenderer` (same `PanelSettings` + `sourceAsset`)
  restores `rootVisualElement` (SettingsScreen/CardScreen resolve, verified empirically).
- **Consolidation: every host carries the `PanelRenderer` + `UIDocument` pair.** Added the
  idempotent `PanelRendererHostMigration` editor script (`-executeMethod`) that gives each host the
  component it's missing, copying `PanelSettings`/`sourceAsset` from the one it has: `Playground.unity`
  got its `UIDocument` back; `Rat.unity` got a `PanelRenderer`; `UI_TouchScreenInput.prefab` (mobile)
  also got a `PanelRenderer`. Runtime code stays on `UIDocument.rootVisualElement` (the only scripting
  API — PanelRenderer can't be scripted in this build); the pair is the canonical 6.7 arrangement.
- **Smoke tests hardened** — both `RuntimeUITest` and `RatUITest` now assert the host carries the
  `PanelRenderer` + `UIDocument` pair, so this drift can't silently regress again.
- **Gates** — EditMode 22/22. PlayMode 6/9: the UI smoke tests (`UitkPanel`, `RatHud`) are green; the
  remaining 3 are **pre-existing on clean HEAD** (their sources are unchanged by the migration):
  `Jump_WhenGrounded`/`MoveInput_Advances` (physics don't settle headless — character never grounds/
  moves) and `WinningGrantsTheRewardCard`. Corrections to the earlier note: `WinningGrants` is **not**
  a batch-environment artifact — it's a deterministic test-setup mismatch (it constructs a
  `MinigameService` but never calls `Start`, so `ActiveId` stays null and `Complete`'s stale-outcome
  guard swallows the grant). Out of the migration's remit; left as-is and documented.
- **Builds** — `BuildWebCLI` Web export builds clean again with the pair on both level hosts
  (fresh `WebGL.data.br`, no missing-script/uGUI/TMP errors). Windows build still not runnable on this
  macOS host (documented since Phase 4).

### Notes / discoveries (additional)
- `Object.FindFirstObjectByType` emits a deprecation warning on this build — fine, it is the
  supported surface here (see notes below) and matches existing `RuntimeUI`/test usage.
- Test runs go through the Unity CLI (`unity test . --mode EditMode|PlayMode --output f.xml`),
  not raw `-runTests` flags. Reminder: startup logs only after `unity open .` on first diff.
- This build's (6000.7.0b1) Dart-flavored surface lacks `Object.FindFirstObjectByName` and
  `SerializedObject.SetProperty` — write against `GameObject.Find` and
  `SerializedProperty` value props (`objectReferenceValue` / `enumValueIndex` / …) instead.
- NUnit `Is.EqualTo` on UITK enums (`DisplayStyle.Flex`) compares enum instances, not values,
  in PlayMode here — assert on `.ToString()`.
- The Rat overlay's prefab activation fires the QuickOutline uv4 log *after* load, so
  `RatUITest` keeps `LogAssert.ignoreFailingMessages` on for all its frames and gates on
  structural asserts (same noise rationale as `RuntimeUITest`).

### Notes / discoveries
- The repo's Dart-flavored runtime rejects `import`/`using` in `eval`, `for (T x : col)`
  foreach and `FindObjectsByType` — write C# accordingly.
- Dev flow boots from **`Bootstrapper.unity`** (core) then additively loads levels — the
  README’s “Playground holds the Bootstrapper” is stale.
- Batch (headless) PlayMode trips pre-existing scene noise (QuickOutline uv4 mesh writes,
  2 legacy missing-script warnings); the smoke test suppresses its load window.
- `Playground.unity`, `Rat.unity`, and `ProjectSettings/EditorBuildSettings.asset` contain the
  migration scene work; all other files listed in `git status` are the user’s own WIP.