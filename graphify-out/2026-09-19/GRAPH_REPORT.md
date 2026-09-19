# Graph Report - uni-tam  (2026-09-19)

## Corpus Check
- 60 files · ~231,194 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 771 nodes · 1478 edges · 36 communities (32 shown, 2 thin omitted)
- Extraction: 89% EXTRACTED · 11% INFERRED · 0% AMBIGUOUS · INFERRED: 157 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `4236b224`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- ChapterGateEntry
- MonoBehaviour
- Container
- Game.Editor
- Bootstrapper
- InteractionService
- DialogueService
- Outline
- uni-tam — chapter-driven, first-person narrative adventure
- ChapterSettings
- Game.Core
- InputMonitor
- Chopstick
- Controller
- unity-mcp
- VirtualJoystick
- BallController
- GameManager
- CardInventory
- ContentYAMLParser.cs
- .StartRound
- CardEntry
- PromptIcon
- ContentTools
- ActionPromptPanel
- DialogueEntry
- ChapterState
- .RunImport
- ToastOverlay
- Services
- ContentYAMLExporter
- ProgressStore
- GameState
- ChapterEntry

## God Nodes (most connected - your core abstractions)
1. `ActionPromptPanel` - 39 edges
2. `BallController` - 35 edges
3. `GameManager` - 35 edges
4. `DialogueService` - 34 edges
5. `CardInventory` - 33 edges
6. `Chopstick` - 31 edges
7. `InputMonitor` - 29 edges
8. `Outline` - 28 edges
9. `Game.Core` - 26 edges
10. `Bootstrapper` - 24 edges

## Surprising Connections (you probably didn't know these)
- `Bootstrapper` --references--> `Container`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/Bootstrapper.cs → Assets/Scripts/Framework/Container.cs
- `Controller` --references--> `InputMonitor`  [EXTRACTED]
  Assets/Scripts/Runtime/Character/Controller.cs → Assets/Scripts/Runtime/Input/InputMonitor.cs
- `WorldTag` --references--> `CardSettings`  [EXTRACTED]
  Assets/Scripts/Runtime/Interaction/WorldTag.cs → Assets/Scripts/Runtime/Content/CardSettings.cs
- `CardSelectionMenu` --references--> `CardSettings`  [EXTRACTED]
  Assets/Scripts/Runtime/UI/CardSelectionMenu.cs → Assets/Scripts/Runtime/Content/CardSettings.cs
- `ToastOverlay` --references--> `CardSettings`  [EXTRACTED]
  Assets/Scripts/Runtime/UI/ToastOverlay.cs → Assets/Scripts/Runtime/Content/CardSettings.cs

## Import Cycles
- None detected.

## Communities (36 total, 2 thin omitted)

### Community 0 - "ChapterGateEntry"
Cohesion: 0.29
Nodes (7): ChapterGateEntry, Chapter, Conditions, ProgressConditionEntry, ActorId, CardId, Type

### Community 1 - "MonoBehaviour"
Cohesion: 0.06
Nodes (20): ActorBinding, DebugMenu, IDisposable, Transform, ActorBinding, ChapterSpawner, Interactable, Id (+12 more)

### Community 2 - "Container"
Cohesion: 0.10
Nodes (18): Dictionary, List, Container, ReactiveProperty, InputSettings, MouseSensitivity, List, Test (+10 more)

### Community 3 - "Game.Editor"
Cohesion: 0.25
Nodes (3): MenuItem, ChapterDebug, Game.Editor

### Community 4 - "Bootstrapper"
Cohesion: 0.12
Nodes (15): InputSettings, Bootstrapper, Cards, Chapter, CurrentLevel, Dialogue, InputSettings, Instance (+7 more)

### Community 5 - "InteractionService"
Cohesion: 0.20
Nodes (6): HashSet, IEnumerable, List, InteractionService, Test, InteractionServiceTest

### Community 6 - "DialogueService"
Cohesion: 0.12
Nodes (18): DialogueEntry, Dictionary, List, DialogueEntry, DialogueLine, DialogueSettings, HashSet, IReadOnlyCollection (+10 more)

### Community 7 - "Outline"
Cohesion: 0.09
Nodes (22): Color, HashSet, List, Renderer, Vector3, ListVector3, Mode, OutlineAll (+14 more)

### Community 8 - "uni-tam — chapter-driven, first-person narrative adventure"
Cohesion: 0.14
Nodes (13): Assemblies, Conventions, Designer & writer onboarding, Importing a minigame (the agent recipe), Layers, Minigames, Patterns, Ported: Banh Đũa (Chơi Chuyền) — rounds 1–3 (+5 more)

### Community 9 - "ChapterSettings"
Cohesion: 0.13
Nodes (18): AnimationClip, ChapterEntry, List, ContentAssetFactory, Created, Skipped, Updated, Dictionary (+10 more)

### Community 10 - "Game.Core"
Cohesion: 0.06
Nodes (27): MinigameEntry, Dictionary, List, Card, CardSettings, MorphTargetValue, Dictionary, List (+19 more)

### Community 11 - "InputMonitor"
Cohesion: 0.05
Nodes (24): InputSettings, Vector2, InputMonitor, Vector2, VirtualInput, Button, GameObject, IDisposable (+16 more)

### Community 12 - "Chopstick"
Cohesion: 0.06
Nodes (23): Color, IEnumerator, Renderer, SphereCollider, Chopstick, State, ChopstickState, Available (+15 more)

### Community 13 - "Controller"
Cohesion: 0.08
Nodes (21): CharacterController, GameObject, LayerMask, PlayerInput, Controller, _isCurrentDeviceMouse, LayerMask, Pusher (+13 more)

### Community 15 - "VirtualJoystick"
Cohesion: 0.16
Nodes (11): Vector2, TouchscreenInput, VirtualJoystick, PointerDownEvent, PointerEnterEvent, PointerLeaveEvent, PointerMoveEvent, PointerUpEvent (+3 more)

### Community 16 - "BallController"
Cohesion: 0.11
Nodes (13): RaycastHit, Rigidbody, SphereCollider, Transform, Vector3, BallController, EffectiveGravity, LastThrowDirection (+5 more)

### Community 17 - "GameManager"
Cohesion: 0.15
Nodes (7): List, TMP_Text, GameManager, CatchLineY, CollectedThisTurn, RequiredThisTurn, GameState

### Community 18 - "CardInventory"
Cohesion: 0.18
Nodes (10): HashSet, IEnumerable, IReadOnlyCollection, Subject, CardInventory, CardSelectionRequested, Granted, Owned (+2 more)

### Community 19 - "ContentYAMLParser.cs"
Cohesion: 0.15
Nodes (15): ExpressionEntry, Id, MorphTargets, ExpressionsFile, ExpressionDefinitions, MinigameDefinitionsFile, MinigameDefinitions, MinigameEntry (+7 more)

### Community 21 - "CardEntry"
Cohesion: 0.25
Nodes (8): CardEntry, CardId, Description, DisplayName, IconPath, TargetActorIds, CardsFile, CardDefinitions

### Community 22 - "PromptIcon"
Cohesion: 0.08
Nodes (29): Color32, Vector2, VertexHelper, ArrowIcon, Dir, Down, DownLeft, DownRight (+21 more)

### Community 23 - "ContentTools"
Cohesion: 0.27
Nodes (3): MenuItem, ContentTools, StalenessChecker

### Community 24 - "ActionPromptPanel"
Cohesion: 0.12
Nodes (16): Canvas, Canvas, Color, RectTransform, TMP_Text, Transform, Vector2, ActionPromptPanel (+8 more)

### Community 25 - "DialogueEntry"
Cohesion: 0.20
Nodes (10): DialogueEntry, CardId, Lines, MinigameId, DialogueLineEntry, Duration, ExpressionId, Text (+2 more)

### Community 26 - "ChapterState"
Cohesion: 0.25
Nodes (4): List, ReactiveProperty, ChapterState, CurrentChapter

### Community 27 - ".RunImport"
Cohesion: 0.32
Nodes (6): List, ChaptersFile, ChapterAdvances, ChapterEntries, ContentYAMLParser, IDeserializer

### Community 28 - "ToastOverlay"
Cohesion: 0.22
Nodes (5): IDisposable, List, Toast, ToastOverlay, Toast

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

## Knowledge Gaps
- **132 isolated node(s):** `C:\Users\usaree\.unity\relay\relay_win.exe`, `OutlineAll`, `OutlineVisible`, `OutlineHidden`, `OutlineAndSilhouette` (+127 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 264 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **2 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Bootstrapper` connect `Bootstrapper` to `MonoBehaviour`, `Container`, `InteractionService`, `DialogueService`, `Game.Core`, `CardInventory`, `ChapterState`, `ProgressStore`?**
  _High betweenness centrality (0.230) - this node is a cross-community bridge._
- **Why does `ActionPromptPanel` connect `ActionPromptPanel` to `MonoBehaviour`, `Chopstick`, `GameManager`, `.StartRound`, `PromptIcon`?**
  _High betweenness centrality (0.196) - this node is a cross-community bridge._
- **Why does `GameManager` connect `GameManager` to `GameState`, `MonoBehaviour`, `.BeginTurn`, `.Update`, `Chopstick`, `BallController`, `.StartRound`, `ActionPromptPanel`?**
  _High betweenness centrality (0.119) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `CardInventory` (e.g. with `.GrantId_AddsOnce_AndFires()` and `.GrantId_EmptyOrNull_IsIgnored()`) actually correct?**
  _`CardInventory` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `C:\Users\usaree\.unity\relay\relay_win.exe`, `OutlineAll`, `OutlineVisible` to the rest of the system?**
  _132 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `MonoBehaviour` be split into smaller, more focused modules?**
  _Cohesion score 0.0627177700348432 - nodes in this community are weakly interconnected._
- **Should `Container` be split into smaller, more focused modules?**
  _Cohesion score 0.0967741935483871 - nodes in this community are weakly interconnected._