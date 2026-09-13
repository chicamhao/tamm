# Graph Report - uni-tam  (2026-09-13)

## Corpus Check
- 33 files · ~217,124 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 354 nodes · 618 edges · 13 communities
- Extraction: 89% EXTRACTED · 11% INFERRED · 0% AMBIGUOUS · INFERRED: 67 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `66d74b11`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .RunImport
- MonoBehaviour
- Container
- ContentAssetFactory
- Bootstrapper
- CardInventory
- .BuildPlayer
- ContentTools
- uni-tam — chapter-driven, first-person narrative adventure
- CardSelectionMenu
- InputMonitor
- Controller
- VirtualJoystick

## God Nodes (most connected - your core abstractions)
1. `InputMonitor` - 27 edges
2. `CardInventory` - 24 edges
3. `Controller` - 20 edges
4. `Bootstrapper` - 16 edges
5. `SettingsMenu` - 16 edges
6. `Game.Core` - 15 edges
7. `InteractionService` - 15 edges
8. `CardSelectionMenu` - 13 edges
9. `Container` - 12 edges
10. `ContentTools` - 11 edges

## Surprising Connections (you probably didn't know these)
- `Bootstrapper` --references--> `Container`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/Bootstrapper.cs → Assets/Scripts/Framework/Container.cs
- `Controller` --references--> `InputMonitor`  [EXTRACTED]
  Assets/Scripts/Runtime/Character/Controller.cs → Assets/Scripts/Runtime/Input/InputMonitor.cs
- `Bootstrapper` --references--> `CardInventory`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/Bootstrapper.cs → Assets/Scripts/Runtime/Core/CardInventory.cs
- `Bootstrapper` --references--> `GameSettings`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/Bootstrapper.cs → Assets/Scripts/Runtime/Core/GameSettings.cs
- `Bootstrapper` --references--> `InteractionService`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/Bootstrapper.cs → Assets/Scripts/Runtime/Core/InteractionService.cs

## Import Cycles
- None detected.

## Communities (13 total, 0 thin omitted)

### Community 0 - ".RunImport"
Cohesion: 0.07
Nodes (40): ContentYAMLExporter, Exported, Skipped, List, CardEntry, CardId, Description, DisplayName (+32 more)

### Community 1 - "MonoBehaviour"
Cohesion: 0.08
Nodes (14): LayerMask, Pusher, Interactable, Interactor, PlayerInput, MobileDisableAutoSwitchControls, Camera, ControllerColliderHit (+6 more)

### Community 2 - "Container"
Cohesion: 0.10
Nodes (19): Dictionary, List, Container, ReactiveProperty, String, InputSettings, MouseSensitivity, List (+11 more)

### Community 3 - "ContentAssetFactory"
Cohesion: 0.07
Nodes (32): AnimationClip, ChapterEntry, DialogueEntry, ContentAssetFactory, Created, Skipped, Updated, Dictionary (+24 more)

### Community 4 - "Bootstrapper"
Cohesion: 0.06
Nodes (26): Bootstrapper, Cards, InputSettings, Instance, Interactions, Pause, Progress, ReactiveProperty (+18 more)

### Community 5 - "CardInventory"
Cohesion: 0.10
Nodes (17): InputSettings, HashSet, IEnumerable, Subject, CardInventory, CardSelectionRequested, Granted, Owned (+9 more)

### Community 6 - ".BuildPlayer"
Cohesion: 0.29
Nodes (8): CharacterController, PlayerInput, CharacterControllerTest, BoxCollider, IEnumerator, Player, Rigidbody, UnityTest

### Community 7 - "ContentTools"
Cohesion: 0.27
Nodes (3): ContentTools, StalenessChecker, MenuItem

### Community 8 - "uni-tam — chapter-driven, first-person narrative adventure"
Cohesion: 0.20
Nodes (9): Assemblies, Conventions, Layers, Patterns, Porting checklist, Scene setup (reference: Playground), Stack, Tests (+1 more)

### Community 9 - "CardSelectionMenu"
Cohesion: 0.18
Nodes (9): Button, GameObject, IDisposable, List, Subject, TMP_Text, CardSelectionMenu, CardSelected (+1 more)

### Community 11 - "InputMonitor"
Cohesion: 0.13
Nodes (6): Vector2, InputMonitor, Vector2, VirtualInput, InputAction, InputValue

### Community 13 - "Controller"
Cohesion: 0.16
Nodes (8): CharacterController, GameObject, LayerMask, PlayerInput, Controller, _isCurrentDeviceMouse, GameObject, Player

### Community 15 - "VirtualJoystick"
Cohesion: 0.16
Nodes (11): Vector2, TouchscreenInput, VirtualJoystick, PointerDownEvent, PointerEnterEvent, PointerLeaveEvent, PointerMoveEvent, PointerUpEvent (+3 more)

## Knowledge Gaps
- **57 isolated node(s):** `Created`, `Updated`, `Skipped`, `Exported`, `Skipped` (+52 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 127 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Bootstrapper` connect `Bootstrapper` to `MonoBehaviour`, `Container`, `ContentAssetFactory`, `CardInventory`?**
  _High betweenness centrality (0.477) - this node is a cross-community bridge._
- **Why does `GameSettings` connect `ContentAssetFactory` to `Bootstrapper`?**
  _High betweenness centrality (0.332) - this node is a cross-community bridge._
- **Why does `InputMonitor` connect `InputMonitor` to `MonoBehaviour`, `Bootstrapper`, `Controller`, `.BuildPlayer`?**
  _High betweenness centrality (0.179) - this node is a cross-community bridge._
- **Are the 6 inferred relationships involving `CardInventory` (e.g. with `.GrantId_AddsOnce_AndFires()` and `.GrantId_EmptyOrNull_IsIgnored()`) actually correct?**
  _`CardInventory` has 6 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Created`, `Updated`, `Skipped` to the rest of the system?**
  _57 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.RunImport` be split into smaller, more focused modules?**
  _Cohesion score 0.07137254901960784 - nodes in this community are weakly interconnected._
- **Should `MonoBehaviour` be split into smaller, more focused modules?**
  _Cohesion score 0.08172043010752689 - nodes in this community are weakly interconnected._