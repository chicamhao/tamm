# Graph Report - uni-tam  (2026-09-19)

## Corpus Check
- 61 files · ~231,858 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 783 nodes · 1503 edges · 45 communities (40 shown, 3 thin omitted)
- Extraction: 90% EXTRACTED · 10% INFERRED · 0% AMBIGUOUS · INFERRED: 155 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `65c2edee`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- ChapterGateEntry
- Interactable
- Container
- Game.Editor
- Bootstrapper
- CardInventory
- DialogueService
- Outline
- uni-tam — chapter-driven, first-person narrative adventure
- ChapterSpawner
- ChapterSettings
- InputMonitor
- Chopstick
- .BuildPlayer
- unity-mcp
- VirtualJoystick
- BallController
- RatManager
- MinigameService
- ContentYAMLParser.cs
- RoundRules
- CardEntry
- PromptIcon
- ContentTools
- ActionPromptPanel
- DialogueEntry
- Game.Core
- .RunImport
- CardSelectionMenu
- Services
- ContentYAMLExporter
- ProgressStore
- GameState
- ChapterEntry
- .FailTurn
- ToastOverlay
- MonoBehaviour
- Pusher
- DialoguePanel
- SettingsMenu
- WorldTag
- MobileDisableAutoSwitchControls
- PauseState

## God Nodes (most connected - your core abstractions)
1. `ActionPromptPanel` - 39 edges
2. `BallController` - 35 edges
3. `RatManager` - 35 edges
4. `DialogueService` - 34 edges
5. `CardInventory` - 33 edges
6. `InputMonitor` - 31 edges
7. `Chopstick` - 31 edges
8. `Outline` - 28 edges
9. `Game.Core` - 27 edges
10. `Bootstrapper` - 26 edges

## Surprising Connections (you probably didn't know these)
- `Bootstrapper` --references--> `Container`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/Bootstrapper.cs → Assets/Scripts/Framework/Container.cs
- `WorldTag` --references--> `CardSettings`  [EXTRACTED]
  Assets/Scripts/Runtime/Interaction/WorldTag.cs → Assets/Scripts/Runtime/Content/CardSettings.cs
- `CardSelectionMenu` --references--> `CardSettings`  [EXTRACTED]
  Assets/Scripts/Runtime/UI/CardSelectionMenu.cs → Assets/Scripts/Runtime/Content/CardSettings.cs
- `ToastOverlay` --references--> `CardSettings`  [EXTRACTED]
  Assets/Scripts/Runtime/UI/ToastOverlay.cs → Assets/Scripts/Runtime/Content/CardSettings.cs
- `ChapterState` --references--> `ChapterSettings`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/ChapterState.cs → Assets/Scripts/Runtime/Content/ChapterSettings.cs

## Import Cycles
- None detected.

## Communities (45 total, 3 thin omitted)

### Community 0 - "ChapterGateEntry"
Cohesion: 0.29
Nodes (7): ChapterGateEntry, Chapter, Conditions, ProgressConditionEntry, ActorId, CardId, Type

### Community 1 - "Interactable"
Cohesion: 0.18
Nodes (7): Interactable, Id, Camera, Color, Interactor, MinigameTrigger, Game.Interaction

### Community 2 - "Container"
Cohesion: 0.10
Nodes (18): Dictionary, List, Container, ReactiveProperty, InputSettings, MouseSensitivity, List, Test (+10 more)

### Community 3 - "Game.Editor"
Cohesion: 0.25
Nodes (3): MenuItem, ChapterDebug, Game.Editor

### Community 4 - "Bootstrapper"
Cohesion: 0.14
Nodes (12): InputSettings, Bootstrapper, Cards, Chapter, CurrentLevel, Dialogue, InputSettings, Instance (+4 more)

### Community 5 - "CardInventory"
Cohesion: 0.09
Nodes (17): HashSet, IEnumerable, IReadOnlyCollection, Subject, CardInventory, CardSelectionRequested, Granted, Owned (+9 more)

### Community 6 - "DialogueService"
Cohesion: 0.08
Nodes (24): DialogueEntry, Dictionary, List, DialogueEntry, DialogueLine, DialogueSettings, List, ReactiveProperty (+16 more)

### Community 7 - "Outline"
Cohesion: 0.09
Nodes (22): Color, HashSet, List, Renderer, Vector3, ListVector3, Mode, OutlineAll (+14 more)

### Community 8 - "uni-tam — chapter-driven, first-person narrative adventure"
Cohesion: 0.14
Nodes (13): Assemblies, Conventions, Designer & writer onboarding, Importing a minigame (the agent recipe), Layers, Minigames, Patterns, Ported: Banh Đũa (Chơi Chuyền), codename Rat — rounds 1–3 (+5 more)

### Community 9 - "ChapterSpawner"
Cohesion: 0.29
Nodes (5): ActorBinding, IDisposable, Transform, ActorBinding, ChapterSpawner

### Community 10 - "ChapterSettings"
Cohesion: 0.07
Nodes (35): AnimationClip, ChapterEntry, List, MinigameEntry, ContentAssetFactory, Created, Skipped, Updated (+27 more)

### Community 11 - "InputMonitor"
Cohesion: 0.09
Nodes (15): CharacterController, GameObject, LayerMask, PlayerInput, Controller, _isCurrentDeviceMouse, InputSettings, Vector2 (+7 more)

### Community 12 - "Chopstick"
Cohesion: 0.06
Nodes (22): Color, IEnumerator, Renderer, SphereCollider, Chopstick, State, ChopstickState, Available (+14 more)

### Community 13 - ".BuildPlayer"
Cohesion: 0.29
Nodes (8): CharacterController, IEnumerator, PlayerInput, Rigidbody, CharacterControllerTest, BoxCollider, Player, UnityTest

### Community 15 - "VirtualJoystick"
Cohesion: 0.16
Nodes (11): Vector2, TouchscreenInput, VirtualJoystick, PointerDownEvent, PointerEnterEvent, PointerLeaveEvent, PointerMoveEvent, PointerUpEvent (+3 more)

### Community 16 - "BallController"
Cohesion: 0.11
Nodes (13): RaycastHit, Rigidbody, SphereCollider, Transform, Vector3, BallController, EffectiveGravity, LastThrowDirection (+5 more)

### Community 17 - "RatManager"
Cohesion: 0.18
Nodes (7): List, TMP_Text, RatManager, CatchLineY, CollectedThisTurn, RequiredThisTurn, GameState

### Community 18 - "MinigameService"
Cohesion: 0.24
Nodes (4): GameObject, List, MinigameService, ActiveId

### Community 19 - "ContentYAMLParser.cs"
Cohesion: 0.15
Nodes (15): ExpressionEntry, Id, MorphTargets, ExpressionsFile, ExpressionDefinitions, MinigameDefinitionsFile, MinigameDefinitions, MinigameEntry (+7 more)

### Community 21 - "CardEntry"
Cohesion: 0.25
Nodes (8): CardEntry, CardId, Description, DisplayName, IconPath, TargetActorIds, CardsFile, CardDefinitions

### Community 22 - "PromptIcon"
Cohesion: 0.07
Nodes (29): Color32, Vector2, VertexHelper, ArrowIcon, Dir, Down, DownLeft, DownRight (+21 more)

### Community 23 - "ContentTools"
Cohesion: 0.27
Nodes (3): MenuItem, ContentTools, StalenessChecker

### Community 24 - "ActionPromptPanel"
Cohesion: 0.11
Nodes (17): Canvas, Canvas, Color, RectTransform, TMP_Text, Transform, Vector2, ActionPromptPanel (+9 more)

### Community 25 - "DialogueEntry"
Cohesion: 0.20
Nodes (10): DialogueEntry, CardId, Lines, MinigameId, DialogueLineEntry, Duration, ExpressionId, Text (+2 more)

### Community 26 - "Game.Core"
Cohesion: 0.19
Nodes (6): MinigameTest, Game.UI, Game.Core, Game.Content, Game.Minigames.Tests, Game.Input

### Community 27 - ".RunImport"
Cohesion: 0.32
Nodes (6): List, ChaptersFile, ChapterAdvances, ChapterEntries, ContentYAMLParser, IDeserializer

### Community 28 - "CardSelectionMenu"
Cohesion: 0.24
Nodes (7): Button, GameObject, IDisposable, List, RectTransform, TMP_Text, CardSelectionMenu

### Community 29 - "Services"
Cohesion: 0.20
Nodes (9): Services, Cards, Chapter, Dialogue, InputSettings, Interactions, Minigame, Pause (+1 more)

### Community 30 - "ContentYAMLExporter"
Cohesion: 0.33
Nodes (4): ContentYAMLExporter, Exported, Skipped, ISerializer

### Community 31 - "ProgressStore"
Cohesion: 0.29
Nodes (3): IEnumerable, String, ProgressStore

### Community 32 - "GameState"
Cohesion: 0.25
Nodes (8): GameState, Failed, GameOver, Picking, RoundComplete, SliceComplete, WaitingForCatch, WaitingForThrow

### Community 33 - "ChapterEntry"
Cohesion: 0.40
Nodes (5): ChapterEntry, ActorId, Chapter, IsVisible, SpawnPointId

### Community 35 - "ToastOverlay"
Cohesion: 0.22
Nodes (5): IDisposable, List, Toast, ToastOverlay, Toast

### Community 36 - "MonoBehaviour"
Cohesion: 0.20
Nodes (4): DebugMenu, TutorialPrompt, Game.Debug, MonoBehaviour

### Community 37 - "Pusher"
Cohesion: 0.24
Nodes (5): LayerMask, Pusher, ControllerColliderHit, Game.Character.Tests, Game.Character

### Community 39 - "DialoguePanel"
Cohesion: 0.25
Nodes (4): GameObject, IDisposable, TMP_Text, DialoguePanel

### Community 40 - "SettingsMenu"
Cohesion: 0.29
Nodes (6): Button, GameObject, String, TMP_Text, SettingsMenu, Slider

### Community 41 - "WorldTag"
Cohesion: 0.38
Nodes (3): Camera, TMP_Text, WorldTag

### Community 42 - "MobileDisableAutoSwitchControls"
Cohesion: 0.40
Nodes (3): PlayerInput, MobileDisableAutoSwitchControls, Game.Mobile.Utilities

### Community 43 - "PauseState"
Cohesion: 0.40
Nodes (3): ReactiveProperty, PauseState, IsPaused

## Knowledge Gaps
- **133 isolated node(s):** `C:\Users\usaree\.unity\relay\relay_win.exe`, `OutlineAll`, `OutlineVisible`, `OutlineHidden`, `OutlineAndSilhouette` (+128 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 269 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Bootstrapper` connect `Bootstrapper` to `Container`, `MonoBehaviour`, `CardInventory`, `DialogueService`, `ChapterSettings`, `PauseState`, `MinigameService`, `Game.Core`, `ProgressStore`?**
  _High betweenness centrality (0.212) - this node is a cross-community bridge._
- **Why does `ActionPromptPanel` connect `ActionPromptPanel` to `RatManager`, `RoundRules`, `PromptIcon`, `MonoBehaviour`?**
  _High betweenness centrality (0.207) - this node is a cross-community bridge._
- **Why does `RatManager` connect `RatManager` to `GameState`, `.FailTurn`, `MonoBehaviour`, `Chopstick`, `.OnChopstickTapped`, `BallController`, `RoundRules`, `ActionPromptPanel`?**
  _High betweenness centrality (0.119) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `CardInventory` (e.g. with `.GrantId_AddsOnce_AndFires()` and `.GrantId_EmptyOrNull_IsIgnored()`) actually correct?**
  _`CardInventory` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `C:\Users\usaree\.unity\relay\relay_win.exe`, `OutlineAll`, `OutlineVisible` to the rest of the system?**
  _133 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Container` be split into smaller, more focused modules?**
  _Cohesion score 0.0967741935483871 - nodes in this community are weakly interconnected._
- **Should `Bootstrapper` be split into smaller, more focused modules?**
  _Cohesion score 0.14285714285714285 - nodes in this community are weakly interconnected._