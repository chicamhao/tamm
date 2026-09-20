# Changelog

Append-only history of shipped work. Reference material (how the systems work) lives in
[docs/](docs/); this file only records *what happened and when*.

Newest first. Each entry: date, what shipped, where it lives.

## 2026-09-20 — Rat port (rounds 1–3)

Ported from `rice/rat` prototype, cut to minimum-functionality tap-through:

| Shipped | Location |
|---|---|
| Ball / chopstick / pool physics | `Runtime/Minigames/Rat/` (BallController, Chopstick, ChopstickManager) — ported 1:1, new-physics API matches this project's build (6000.7.0a6) |
| Round rules (Nhăt Một / Hai / Ba, 10 sticks, take r per turn) | `RoundRules.cs` — balance table removed, built-in traditional rules kept |
| State machine + heart loss + miss detection | `RatManager.cs` — score/TuningHud stripped; drives the prompt panel; win/lose call `Services.Minigame.Complete` |
| Tap + swipe-up throw input (mobile + desktop) | `GestureInput.cs` — sweep gesture and swipe steering stripped |
| Prompt HUD (icons, chips, badges) | `Runtime/Minigames/Rat/UI/` — ActionPromptPanel + 4 procedural icons (PromptIcon, ArrowIcon, TapIcon, CircleIcon), ported 1:1; scene config GO restored |
| Scene, prefabs, HUD | `Scenes/Rat.unity`, `Prefabs/Chopsticks.prefab`, `Prefabs/Table.prefab` — panel config GO restored, instruction/progress texts blanked (the panel renders live labels) |
| Data + first hook | `minigames.yaml` (`rat` entry) + `minigame_id: rat` on the `card_chopsticks_tao` dialogue; win reward: `card_chopsticks` |
| Trigger | Sandbox path: `MinigameTrigger` (`_id: rat`) on any prop with a collider in a level. Committed entry: the `minigame_id: rat` dialogue hook (`card_chopsticks_tao`) |

**Not ported (deferred)** — sweep/drag-to-collect affordance, rounds 4+, the balance
table tuning tier, score display, `TuningHud` (debug overlay, not player UI).

Playtested in-editor (Playground → disc → play → ESC/win/lose → back to the world):
controller freezes and the pointer frees during the minigame, the playground hides,
offset returns and gameplay resumes cleanly.