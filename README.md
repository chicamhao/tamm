# tamm
Chapter-driven, first-person narrative adventure.

The loop: **interact with objects to collect cards, use cards to converse with NPCs,
progress chapters, and reshape the world around you.**

The card-interaction scaffold in `Game.Core`/`Game.Interaction`/`Game.UI` is the
**reference loop** — copy its shape (interact → service rule → reactive UI).

## Quick start

1. Open the project in Unity **6000.7.0b1**.
2. `Assets → Import Content from YAML` — content is YAML-first
   ([docs/content-pipeline.md](docs/content-pipeline.md)).
3. Open `Assets/Scenes/Bootstrapper.unity` — the core scene — and press Play. It boots the
   services and loads the `Playground` level additively. (Pressing Play from `Playground`
   alone runs the level without the core: every UI leaf logs a warning and nothing works.)
   Settings gear → Save / Load / New Game; **F2** opens the dev chapter stepper.

## The invariant

`Services.*` is never null at runtime — services are built once in `Bootstrapper.Awake` and leaves
subscribe in `Start`. `Assets/Scenes/Bootstrapper.unity` is the persistent **core scene**: its
`Bootstrapper` GameObject is `DontDestroyOnLoad` and owns every service; level scenes hold only
content. Never call `SceneManager.LoadScene` directly — use `Bootstrapper.LoadLevel("...")`
([docs/architecture.md](docs/architecture.md)).

## Docs

| Doc | For | Covers |
|---|---|---|
| [.pi/memories/content-pipeline.md](content-pipeline.md) | designers & writers | YAML editing, import, playtest loop, ids |
| [.pi/memories/changelog.md](changelog.md) | engineers & designer | shipped work, append-only (ports, backlogs) |
| [.pi/memories/architecture.md](architecture.md) | engineers | stack, assemblies, layers, scene setup, patterns, minigame system |
| [.pi/memories/testing.md](testing.md) | engineers | what is tested and how |
| [.pi/memories/deploy-gh-pages.sh](pages.md)| engineers | web build & deploy runbooks (executable) |
| `conventions.yaml` | engineers | naming / class structure / null-checking rules (sealed by default) |

## Tests

`Game.Runtime.Tests` (Edit Mode — rule engines) + `Game.Runtime.PlayTests` (Play Mode —
character/camera) — [docs/testing.md](docs/testing.md).

## Build & deploy (web)

CLI-driven, no editor clicks: `./scripts/build-web.sh` then `./scripts/deploy-gh-pages.sh`.
Site: <https://chicamhao.github.io/tamm/> — first deploy ~1 min; browsers cache the build
for 10 min, so hard-refresh (or incognito) after a deploy.
