# 0003 — Runtime UI is UI Toolkit only

Status: accepted · 2026-09-20

## Context

Runtime UI had grown mixed: uGUI Canvases + TextMeshPro texts in scenes, IMGUI leaves,
a runtime-built minigame HUD. Two rendering stacks means two theming systems and scene
layout state that can't be version-controlled safely.

## Decision

TextMeshPro and uGUI are fully removed (`com.unity.ugui` dropped from the manifest).
Every screen is UI Toolkit (UXML/USS) hosted by the level's `RuntimeUI` panel; no scene
UI objects. Each leaf is a MonoBehaviour that resolves its screen subtree from the
level's runtime document by element name. IMGUI leaves (dev overlays) persist only in
`Game.Debug`.

See [ui-toolkit-migration.md](../ui-toolkit-migration.md) — the in-flight plan with the
inventory and phase boundaries.

## Consequences

- One styling system (USS), one layout model (flex).
- Scene files carry no UI state anymore.
- IMGUI is a deliberately narrow exception for dev-only overlays.