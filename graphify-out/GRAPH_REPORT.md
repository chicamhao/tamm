# Graph Report - starter  (2026-09-12)

## Corpus Check
- 14 files · ~208,152 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 126 nodes · 189 edges · 8 communities (7 shown, 1 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 7 edges (avg confidence: 0.81)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `34d53b4a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MonoBehaviour
- Pusher
- Container
- unity-mcp
- GameRoot
- InputMonitor
- Controller
- VirtualJoystick

## God Nodes (most connected - your core abstractions)
1. `Controller` - 18 edges
2. `InputMonitor` - 16 edges
3. `Container` - 12 edges
4. `VirtualJoystick` - 9 edges
5. `GameRoot` - 8 edges
6. `TouchscreenInput` - 8 edges
7. `Game.Core` - 7 edges
8. `VirtualInput` - 7 edges
9. `ContainerTest` - 7 edges
10. `Progression` - 6 edges

## Surprising Connections (you probably didn't know these)
- `Controller` --references--> `InputMonitor`  [EXTRACTED]
  Assets/Scripts/Runtime/Character/Controller.cs → Assets/Scripts/Runtime/Input/InputMonitor.cs
- `GameRoot` --references--> `Container`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/GameRoot.cs → Assets/Scripts/Runtime/Core/Container.cs
- `GameRoot` --references--> `Progression`  [EXTRACTED]
  Assets/Scripts/Runtime/Core/GameRoot.cs → Assets/Scripts/Runtime/Core/Game.cs
- `VirtualInput` --references--> `InputMonitor`  [EXTRACTED]
  Assets/Scripts/Runtime/Input/VirtualInput.cs → Assets/Scripts/Runtime/Input/InputMonitor.cs

## Import Cycles
- None detected.

## Communities (8 total, 1 thin omitted)

### Community 0 - "MonoBehaviour"
Cohesion: 0.15
Nodes (8): PlayerInput, MobileDisableAutoSwitchControls, IDisposable, ScoreHud, Game.UI, Game.Mobile.Utilities, MonoBehaviour, TMP_Text

### Community 1 - "Pusher"
Cohesion: 0.22
Nodes (5): LayerMask, Pusher, ControllerColliderHit, Game.Character, Game.Input

### Community 2 - "Container"
Cohesion: 0.14
Nodes (14): List, Container, List, ContainerTest, DisposableService, UsesDep, Dictionary, DisposableService (+6 more)

### Community 4 - "GameRoot"
Cohesion: 0.10
Nodes (12): Game, Progression, Progression, GameRoot, Instance, Progression, Progression, Score (+4 more)

### Community 11 - "InputMonitor"
Cohesion: 0.20
Nodes (5): Vector2, InputMonitor, Vector2, VirtualInput, InputValue

### Community 13 - "Controller"
Cohesion: 0.18
Nodes (6): LayerMask, PlayerInput, Controller, _isCurrentDeviceMouse, CharacterController, GameObject

### Community 15 - "VirtualJoystick"
Cohesion: 0.16
Nodes (11): Vector2, TouchscreenInput, VirtualJoystick, PointerDownEvent, PointerEnterEvent, PointerLeaveEvent, PointerMoveEvent, PointerUpEvent (+3 more)

## Knowledge Gaps
- **7 isolated node(s):** `C:\Users\usaree\.unity\relay\relay_win.exe`, `_isCurrentDeviceMouse`, `Instance`, `Progression`, `Score` (+2 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 36 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `GameRoot` connect `GameRoot` to `MonoBehaviour`, `Container`?**
  _High betweenness centrality (0.425) - this node is a cross-community bridge._
- **Why does `Container` connect `Container` to `GameRoot`?**
  _High betweenness centrality (0.283) - this node is a cross-community bridge._
- **Why does `Controller` connect `Controller` to `MonoBehaviour`, `Pusher`, `InputMonitor`?**
  _High betweenness centrality (0.228) - this node is a cross-community bridge._
- **What connects `C:\Users\usaree\.unity\relay\relay_win.exe`, `_isCurrentDeviceMouse`, `Instance` to the rest of the system?**
  _7 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Container` be split into smaller, more focused modules?**
  _Cohesion score 0.1422924901185771 - nodes in this community are weakly interconnected._
- **Should `GameRoot` be split into smaller, more focused modules?**
  _Cohesion score 0.10276679841897234 - nodes in this community are weakly interconnected._