# Graph Report - starter  (2026-09-12)

## Corpus Check
- 11 files · ~208,095 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 112 nodes · 173 edges · 9 communities (7 shown, 2 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 3 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `eaa81c18`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- MonoBehaviour
- Game.Input
- Container
- unity-mcp
- GameRoot
- Progression
- InputMonitor
- Controller
- VirtualJoystick

## God Nodes (most connected - your core abstractions)
1. `Controller` - 18 edges
2. `Container` - 17 edges
3. `InputMonitor` - 16 edges
4. `VirtualJoystick` - 9 edges
5. `GameRoot` - 8 edges
6. `TouchscreenInput` - 8 edges
7. `VirtualInput` - 7 edges
8. `FixtureB` - 6 edges
9. `Progression` - 6 edges
10. `Pusher` - 5 edges

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

## Communities (9 total, 2 thin omitted)

### Community 0 - "MonoBehaviour"
Cohesion: 0.21
Nodes (7): LayerMask, Pusher, PlayerInput, MobileDisableAutoSwitchControls, ControllerColliderHit, Game.Mobile.Utilities, MonoBehaviour

### Community 2 - "Container"
Cohesion: 0.17
Nodes (12): Container, FixtureA, FixtureB, Dictionary, FixtureA, FixtureB, Func, IDisposable (+4 more)

### Community 4 - "GameRoot"
Cohesion: 0.18
Nodes (7): Game, Progression, Progression, GameRoot, Instance, Progression, Game.Core

### Community 5 - "Progression"
Cohesion: 0.40
Nodes (3): Progression, Score, ReactiveProperty

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
- **6 isolated node(s):** `C:\Users\usaree\.unity\relay\relay_win.exe`, `_isCurrentDeviceMouse`, `Instance`, `Progression`, `Score` (+1 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 29 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **2 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `GameRoot` connect `GameRoot` to `MonoBehaviour`, `Container`?**
  _High betweenness centrality (0.460) - this node is a cross-community bridge._
- **Why does `Container` connect `Container` to `GameRoot`?**
  _High betweenness centrality (0.340) - this node is a cross-community bridge._
- **Why does `Controller` connect `Controller` to `MonoBehaviour`, `Game.Input`, `InputMonitor`?**
  _High betweenness centrality (0.252) - this node is a cross-community bridge._
- **What connects `C:\Users\usaree\.unity\relay\relay_win.exe`, `_isCurrentDeviceMouse`, `Instance` to the rest of the system?**
  _6 weakly-connected nodes found - possible documentation gaps or missing edges._