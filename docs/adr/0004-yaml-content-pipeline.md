# 0004 — Content is YAML-first, ScriptableObjects are generated

Status: accepted · 2026-09-20

## Context

Cards, dialogues, chapters, expressions, and minigames are all id-keyed dictionaries.
Hand-editing `.asset` files is error-prone, and designers/writers shouldn't touch Unity
serialization at all.

## Decision

Authoring happens in `Assets/Settings/Game/YAML/*.yaml`. `Assets → Import Content from YAML`
(`Game.Editor` assembly) regenerates the id-keyed ScriptableObject dictionaries
(`CardSettings`, `DialogueSettings`, `ChapterSettings`, `ExpressionSettings`, `MinigameSettings`);
Export is the reverse. A staleness check warns when YAML changes without an import.

## Consequences

- Designers/writers edit text only; new ids "just work" with no code changes.
- The .asset files are build artifacts — regenerate, don't hand-edit.
- The staleness warning is the tripwire when an import was forgotten.