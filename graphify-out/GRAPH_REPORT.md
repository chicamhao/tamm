# Graph Report - uni-tam  (2026-09-17)

## Corpus Check
- 46 files · ~221,639 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 518 nodes · 967 edges · 16 communities (15 shown, 1 thin omitted)
- Extraction: 87% EXTRACTED · 13% INFERRED · 0% AMBIGUOUS · INFERRED: 125 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `49112e13`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .RunImport
- MonoBehaviour
- Container
- Game.Core
- Bootstrapper
- CardInventory
- DialogueService
- Outline
- uni-tam — chapter-driven, first-person narrative adventure
- ChapterSettings
- ToastOverlay
- InputMonitor
- ChapterSpawner
- Controller
- unity-mcp
- VirtualJoystick

## God Nodes (most connected - your core abstractions)
1. `DialogueService` - 33 edges
2. `CardInventory` - 31 edges
3. `InputMonitor` - 29 edges
4. `Outline` - 28 edges
5. `Game.Core` - 23 edges
6. `Bootstrapper` - 21 edges
7. `Controller` - 20 edges
8. `ChapterState` - 18 edges
9. `Game.Content` - 17 edges
10. `SettingsMenu` - 16 edges

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

## Communities (16 total, 1 thin omitted)

### Community 0 - ".RunImport"
Cohesion: 0.05
Nodes (51): MenuItem, ContentTools, StalenessChecker, ContentYAMLExporter, Exported, Skipped, List, CardEntry (+43 more)

### Community 1 - "MonoBehaviour"
Cohesion: 0.07
Nodes (19): DebugMenu, Interactable, Id, Camera, Color, Interactor, Camera, TMP_Text (+11 more)

### Community 2 - "Container"
Cohesion: 0.10
Nodes (18): Dictionary, List, Container, ReactiveProperty, InputSettings, MouseSensitivity, List, Test (+10 more)

### Community 3 - "Game.Core"
Cohesion: 0.06
Nodes (23): MenuItem, ChapterDebug, Dictionary, List, Card, CardSettings, MorphTargetValue, Dictionary (+15 more)

### Community 4 - "Bootstrapper"
Cohesion: 0.06
Nodes (28): InputSettings, Bootstrapper, Cards, Chapter, Dialogue, InputSettings, Instance, Interactions (+20 more)

### Community 5 - "CardInventory"
Cohesion: 0.11
Nodes (16): HashSet, IEnumerable, IReadOnlyCollection, Subject, CardInventory, CardSelectionRequested, Granted, Owned (+8 more)

### Community 6 - "DialogueService"
Cohesion: 0.11
Nodes (18): DialogueEntry, Dictionary, List, DialogueEntry, DialogueLine, DialogueSettings, HashSet, IReadOnlyCollection (+10 more)

### Community 7 - "Outline"
Cohesion: 0.09
Nodes (22): Color, HashSet, List, ListVector3, Mode, OutlineAll, OutlineAndSilhouette, OutlineHidden (+14 more)

### Community 8 - "uni-tam — chapter-driven, first-person narrative adventure"
Cohesion: 0.18
Nodes (10): Assemblies, Conventions, Designer & writer onboarding, Layers, Patterns, Porting checklist, Scene setup (reference: Playground), Stack (+2 more)

### Community 9 - "ChapterSettings"
Cohesion: 0.13
Nodes (18): AnimationClip, ChapterEntry, List, ContentAssetFactory, Created, Skipped, Updated, Dictionary (+10 more)

### Community 10 - "ToastOverlay"
Cohesion: 0.22
Nodes (5): IDisposable, List, Toast, ToastOverlay, Toast

### Community 11 - "InputMonitor"
Cohesion: 0.07
Nodes (20): InputSettings, Vector2, InputMonitor, Vector2, VirtualInput, Button, GameObject, IDisposable (+12 more)

### Community 12 - "ChapterSpawner"
Cohesion: 0.29
Nodes (5): ActorBinding, IDisposable, ActorBinding, ChapterSpawner, Transform

### Community 13 - "Controller"
Cohesion: 0.08
Nodes (21): CharacterController, GameObject, LayerMask, PlayerInput, Controller, _isCurrentDeviceMouse, LayerMask, Pusher (+13 more)

### Community 15 - "VirtualJoystick"
Cohesion: 0.16
Nodes (11): Vector2, TouchscreenInput, VirtualJoystick, PointerDownEvent, PointerEnterEvent, PointerLeaveEvent, PointerMoveEvent, PointerUpEvent (+3 more)

## Knowledge Gaps
- **86 isolated node(s):** `C:\Users\usaree\.unity\relay\relay_win.exe`, `OutlineAll`, `OutlineVisible`, `OutlineHidden`, `OutlineAndSilhouette` (+81 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 190 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Bootstrapper` connect `Bootstrapper` to `MonoBehaviour`, `Container`, `Game.Core`, `CardInventory`, `DialogueService`?**
  _High betweenness centrality (0.220) - this node is a cross-community bridge._
- **Why does `DialogueService` connect `DialogueService` to `Container`, `Game.Core`, `Bootstrapper`, `CardInventory`, `ChapterSettings`?**
  _High betweenness centrality (0.127) - this node is a cross-community bridge._
- **Why does `InputMonitor` connect `InputMonitor` to `MonoBehaviour`, `Game.Core`, `Controller`, `DialogueService`?**
  _High betweenness centrality (0.126) - this node is a cross-community bridge._
- **Are the 9 inferred relationships involving `CardInventory` (e.g. with `.GrantId_AddsOnce_AndFires()` and `.GrantId_EmptyOrNull_IsIgnored()`) actually correct?**
  _`CardInventory` has 9 INFERRED edges - model-reasoned connections that need verification._
- **What connects `C:\Users\usaree\.unity\relay\relay_win.exe`, `OutlineAll`, `OutlineVisible` to the rest of the system?**
  _86 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.RunImport` be split into smaller, more focused modules?**
  _Cohesion score 0.05203442879499218 - nodes in this community are weakly interconnected._
- **Should `MonoBehaviour` be split into smaller, more focused modules?**
  _Cohesion score 0.06538461538461539 - nodes in this community are weakly interconnected._