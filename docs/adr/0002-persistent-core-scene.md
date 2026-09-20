# 0002 — Persistent core scene owns all services

Status: accepted · 2026-09-20

## Context

Levels switch by loading additively over a base scene. If services lived in any level
scene they'd die on every transition.

## Decision

`Playground.unity` is the **core scene**: it holds the `Bootstrapper` GameObject
(`DontDestroyOnLoad`, so it survives every transition) plus the `GameSettings` asset,
and owns every service. Level scenes hold only content — spawners, interactables, UI
leaves. Switch levels via `Bootstrapper.LoadLevel("LevelName")` (loads additively over
the core, unloading the previous level); never call `SceneManager.LoadScene` directly.

## Consequences

- `Services.*` is never null at runtime — **no null guards on service access** (see `conventions.yaml`).
- Service wiring exists in exactly one place (Bootstrapper), and minigames reuse the
  same world because they never leave the core.
- `LoadScene` direct calls are a code-review red flag.