# Graph Report - uni-tam  (2026-09-13)

## Corpus Check
- 31 files · ~216,730 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 341 nodes · 596 edges · 14 communities
- Extraction: 91% EXTRACTED · 9% INFERRED · 0% AMBIGUOUS · INFERRED: 52 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `e35a00a7`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- ContentYAMLParser.cs
- MonoBehaviour
- Container
- ContentAssetFactory
- Bootstrapper
- GameSession
- .BuildPlayer
- .RunImport
- uni-tam — chapter-driven, first-person narrative adventure
- GameHud
- InputSettings
- InputMonitor
- Controller
- VirtualJoystick

## God Nodes (most connected - your core abstractions)
1. `InputMonitor` - 27 edges
2. `GameSession` - 25 edges
3. `Controller` - 20 edges
4. `SettingsMenu` - 16 edges
5. `Game.Core` - 14 edges
6. `Bootstrapper` - 14 edges
7. `ContentAssetFactory` - 12 edges
8. `Container` - 12 edges
9. `ContentTools` - 11 edges
10. `CardEntry` - 11 edges

## Surprising Connections (you probably didn't know these)
- `Bootstrapper` --references--> `Container`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/Bootstrapper.cs → Assets/Scripts/Framework/Container.cs
- `Controller` --references--> `InputMonitor`  [EXTRACTED]
  Assets/Scripts/Runtime/Character/Controller.cs → Assets/Scripts/Runtime/Input/InputMonitor.cs
- `ExpressionDefinition` --references--> `MorphTargetValue`  [EXTRACTED]
  Assets/Scripts/Runtime/Content/ExpressionDefinition.cs → Assets/Scripts/Runtime/Content/DialogueSettings.cs
- `Bootstrapper` --references--> `GameSession`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/Bootstrapper.cs → Assets/Scripts/Runtime/Core/GameSession.cs
- `InputMonitor` --references--> `InputSettings`  [EXTRACTED]
  Assets/Scripts/Runtime/Input/InputMonitor.cs → Assets/Scripts/Runtime/Core/Bootstrapper.cs

## Import Cycles
- None detected.

## Communities (14 total, 0 thin omitted)

### Community 0 - "ContentYAMLParser.cs"
Cohesion: 0.07
Nodes (40): ContentYAMLExporter, Exported, Skipped, List, CardEntry, CardId, Description, DisplayName (+32 more)

### Community 1 - "MonoBehaviour"
Cohesion: 0.08
Nodes (14): LayerMask, Pusher, ClickCollector, Collectible, PlayerInput, MobileDisableAutoSwitchControls, Camera, ControllerColliderHit (+6 more)

### Community 2 - "Container"
Cohesion: 0.13
Nodes (14): Dictionary, List, Container, List, Test, ContainerTest, DisposableService, UsesDep (+6 more)

### Community 3 - "ContentAssetFactory"
Cohesion: 0.07
Nodes (27): AnimationClip, ChapterEntry, DialogueEntry, ContentAssetFactory, Created, Skipped, Updated, List (+19 more)

### Community 4 - "Bootstrapper"
Cohesion: 0.07
Nodes (21): InputSettings, Bootstrapper, InputSettings, Instance, Pause, Progress, Session, String (+13 more)

### Community 5 - "GameSession"
Cohesion: 0.12
Nodes (16): ReactiveProperty, String, GameResult, InProgress, Lost, Won, GameSession, Collected (+8 more)

### Community 6 - ".BuildPlayer"
Cohesion: 0.29
Nodes (8): CharacterController, PlayerInput, CharacterControllerTest, BoxCollider, IEnumerator, Player, Rigidbody, UnityTest

### Community 7 - ".RunImport"
Cohesion: 0.30
Nodes (3): ContentTools, StalenessChecker, MenuItem

### Community 8 - "uni-tam — chapter-driven, first-person narrative adventure"
Cohesion: 0.20
Nodes (9): Assemblies, Conventions, Layers, Patterns, Porting checklist, Scene setup (reference: Playground), Stack, Tests (+1 more)

### Community 9 - "GameHud"
Cohesion: 0.33
Nodes (4): IDisposable, String, TMP_Text, GameHud

### Community 10 - "InputSettings"
Cohesion: 0.33
Nodes (4): ReactiveProperty, String, InputSettings, MouseSensitivity

### Community 11 - "InputMonitor"
Cohesion: 0.10
Nodes (12): Vector2, InputMonitor, Vector2, VirtualInput, GameObject, String, TMP_Text, SettingsMenu (+4 more)

### Community 13 - "Controller"
Cohesion: 0.16
Nodes (8): CharacterController, GameObject, LayerMask, PlayerInput, Controller, _isCurrentDeviceMouse, GameObject, Player

### Community 15 - "VirtualJoystick"
Cohesion: 0.16
Nodes (11): Vector2, TouchscreenInput, VirtualJoystick, PointerDownEvent, PointerEnterEvent, PointerLeaveEvent, PointerMoveEvent, PointerUpEvent (+3 more)

## Knowledge Gaps
- **60 isolated node(s):** `Created`, `Updated`, `Skipped`, `Exported`, `Skipped` (+55 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 120 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `GameSettings` connect `Bootstrapper` to `ContentAssetFactory`, `GameSession`?**
  _High betweenness centrality (0.419) - this node is a cross-community bridge._
- **Why does `Bootstrapper` connect `Bootstrapper` to `MonoBehaviour`, `Container`, `GameSession`?**
  _High betweenness centrality (0.383) - this node is a cross-community bridge._
- **Why does `GameSession` connect `GameSession` to `MonoBehaviour`, `Bootstrapper`?**
  _High betweenness centrality (0.194) - this node is a cross-community bridge._
- **What connects `Created`, `Updated`, `Skipped` to the rest of the system?**
  _60 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ContentYAMLParser.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.06901960784313725 - nodes in this community are weakly interconnected._
- **Should `MonoBehaviour` be split into smaller, more focused modules?**
  _Cohesion score 0.08266129032258064 - nodes in this community are weakly interconnected._
- **Should `Container` be split into smaller, more focused modules?**
  _Cohesion score 0.13405797101449277 - nodes in this community are weakly interconnected._