# Content pipeline — designers & writers

Content is **text files first** — you never touch script code. The YAML files are the
source of truth; the ScriptableObject dictionaries they populate are generated
(`Assets → Import Content from YAML`, implemented by the `Game.Editor` assembly).

## Your daily loop

1. **Edit a YAML file** under `Assets/Settings/Game/YAML/` while Unity is open
   (also valid in VS Code — refresh Unity after).
   - `cards.yaml` — the collectible cards: `card_id`, `display_name`, `description`, `target_actor_ids`.
   - `dialogues.yaml` — NPC reactions, keyed by card + actor: `card_id` is `cardId_actorId`
     (add `_2` → that line only plays in chapter 2). Each line: `text`, `duration`, optional `expression_id`.
   - `chapters.yaml` — per-actor scene state per chapter (`spawn_point_id`, `is_visible`), plus
     `chapter_advances` gates that unlock the next chapter from progress (e.g. `owns_card` / `had_conversation`).
   - `expressions.yaml` — facial-expression morph targets (id-referenced by dialogue lines).
2. **Import:** `Assets → Import Content from YAML`. A console warning appears if you edit YAML
   without re-importing.
3. **Play test** the Playground scene. World tags show what each object/NPC is; aim glow marks
   what's interactable; **F2** opens the dev chapter stepper (↑/↓ moves chapters, gates bypassed);
   the Settings gear has Save / Load / New Game (boot auto-restores your save).
4. **Iterate ids:** object ids grant cards, actor ids (e.g. `cam`, `sm`) open the card menu. New
   card ids in `cards.yaml` just work; new *actor* ids also just work (derived from dialogue keys).

## Pipeline mechanics

Import populates four id-keyed dictionaries — `CardSettings.asset`, `DialogueSettings.asset`,
`ChapterSettings.asset` (incl. `chapter_advances` gates), `ExpressionSettings.asset`.
Export is the reverse. A staleness check warns when YAML changes without an import.

`minigames.yaml` feeds `MinigameSettings` the same way — `id → { scene_name, reward_card_id }`
(see the minigame recipe in [architecture.md](architecture.md)).

## Scene wiring (inspector work)

Wiring an object/NPC into a scene — `Interactable` id + collider, `Outline`, `WorldLabel`,
UI Toolkit panel setup — is inspector work done once per entity: see
[Scene setup](architecture.md#scene-setup-reference-playground) in architecture.md.