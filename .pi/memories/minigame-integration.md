# Minigame integration

How a minigame repo is imported, wired, and updated with minimum friction. The
contract exists so a minigame is **one self-contained folder + one scene + one
data entry**; the core discovers it, never enumerates its internals.

See [architecture.md](architecture.md) for the service's runtime hand-off; this
file is the recipe for adding and updating a minigame.

## The contract (the only API a minigame must implement)

Everything lives in `Game.Runtime`, namespace `Game.Core`; a minigame assembly
references `Game.Runtime` and implements two interfaces:

| Interface | Shape | Who |
|---|---|---|
| `IMinigame` | `void OnStart(IMinigameInput input)` | one root `MonoBehaviour` in the minigame scene |
| `IMinigameInput` | `bool GetPointerDown(out Vector2 pos)`, `GetPointerUp(...)`, `Vector2 GetPointerPosition()` | polled, never raises events |

Flow: `MinigameService.Start(id)` → overlay loads → `Bootstrapper.Update` pumps
`MinigameService.Pump()`, which scans the **active** MonoBehaviours (core/level
roots are parked, so only the overlay is visible) for `IMinigame` → calls
`OnStart(input)`. Outcome goes back through the static locator:
`Services.Minigame.Complete(id, won)`.

**The minigame must not read `UnityEngine.InputSystem` (or legacy input)
directly** — all pointer input goes through the `IMinigameInput` it was handed
at `OnStart`. The core owns mouse/touch mapping centrally
(`MinigameInputProvider`); a minigame repo stays device-agnostic.

## When the original repo can't see our interface (adapter pattern)

An external minigame repo never compiles against `Game.Core` — it doesn't know
`IMinigame` exists and must not. The repo stays 100% self-contained
(self-starts in `Start()`, reads whatever input it likes, exits however it
likes); **we** write a thin adapter in our project that seals it into the
contract. The upstream ask is just a tiny public surface to hook:

```csharp
// THEIR repo — plain MonoBehaviour, no Game.Core, no IMinigame
public class GameManager : MonoBehaviour
{
    public void StartGame()           // optional — skip if their Start() already boots it
    public bool IsFinished { get; }   // polled each frame by our adapter
    public bool Won { get; }          // the result, read once IsFinished
}
```

Most repos already have these (a state enum + a result flag), so upstream
changes **zero files**. Our sealing piece, in the minigame assembly:

```csharp
// Assets/Minigames/<Name>/<Name>Adapter.cs — sealed, implements IMinigame
public sealed class SomeNameAdapter : MonoBehaviour, IMinigame
{
    [SerializeField] private GameManager _game;   // their component, wired in the scene

    public void OnStart(IMinigameInput input)
    {
        _game.StartGame();                        // only if they don't self-start
    }

    private void Update()
    {
        if (!_game.IsFinished || _reported) return;
        _reported = true;
        Services.Minigame.Complete("some_name", _game.Won);
    }
}
```

### Input, three honesty levels

Their gesture reader reads `UnityEngine.InputSystem` directly. How much of our
`IMinigameInput` we force depends on the seam their reader exposes:

| Level | What we do | Why |
|---|---|---|
| **Hook nothing** (laziest) | Adapter ignores input; their reader keeps working | InputSystem is global — the core frees the pointer, their `Mouse.current` reads still work. We lose central device mapping, that's all |
| **Feed if they allow** | If they expose a public pointer setter, the adapter forwards `GetPointerDown/Up` | Clean, but only if their reader has a seam |
| **Refactor their reader** | Rewrite their gesture file to poll `IMinigameInput` (like Rat's GestureInput) | Full ownership — usually ~20 lines, done once at import |

Rule of thumb: **upstream implements a start hook + a finished/result signal;
we implement everything that knows about `IMinigame`.** Their repo never
compiles against our code — that's what keeps `git submodule update`
frictionless.

## Folder layout & class-name isolation

```
Assets/Minigames/<Name>/          ← git submodule (or plain folder for in-repo)
├── <Name>.asmdef                 ← isolates every class in this minigame
├── *.cs                          ← the whole repo, namespaces untouched
└── (scenes/prefabs/art)
```

Isolation is **assembly boundaries, not renames**: each minigame gets its own
`.asmdef`, so a `GameManager` in minigame A and a `GameManager` in minigame B
compile into different assemblies and can never collide. Do not touch the
imported repo's namespaces or class names — that is the update-friction killer.

Minimal asmdef (model: `Assets/Minigames/Rat/Rat.asmdef`):

```json
{
  "name": "Game.<Name>",
  "references": ["Game.Runtime", "Unity.TextMeshPro", "Unity.InputSystem"],
  "autoReferenced": true
}
```

Only reference what the repo actually uses; `Unity.InputSystem` is only needed
if the repo's own code (non-gesture parts) uses it.

## Importing a new minigame

1. **Check out the repo** at `Assets/Minigames/<Name>/` (submodule URL, or copy
   for in-repo work), then add `<Name>.asmdef` per above.
     - Verify the parent actually has the submodule **gitlink** in its index
       (`git ls-files -s Assets/Minigames/<Name>` → mode `160000`). A checkout can
       leave the folder initialized without the index entry; `git add` the folder
       once to register it, then commit the parent.
     - Hand-written `.meta` files (asmdef/cs) must **end with a trailing newline**;
       Unity's parser silently rejects the file without one and regenerates a new
       GUID, orphaning any scene reference you already wrote against the old GUID.
     - Rename the repo's top-level classes only if a name clashes **with
       Game.Runtime** (assembly isolation handles A-vs-B clashes for free).
2. **Convert the entry point**: pick the scene's root manager component, make it
   implement `IMinigame`; move whatever its `Start()` did into
   `OnStart(IMinigameInput)`. Keep a standalone fallback if devs run the scene
   alone: `if (Services.Minigame == null) OnStart(new MinigameInputProvider());`
   (mirrors `RatManager.Start`).
3. **Route gestures through the channel**: replace raw `Mouse.current` /
   `Touchscreen.current` reads with `input.GetPointerDown/Up/GetPointerPosition`
   from the `OnStart` argument.
4. **Report the outcome**: call `Services.Minigame.Complete(id, won)` at win/lose
   (id must match the `minigames.yaml` key). Nothing else — the core grants the
   reward card (gated on `reward_card_id`), drops the overlay, and restores the
   interrupted level.
5. **Scene**: additive level under `Assets/Scenes/` (or inside the minigame
   folder) with its **own camera and AudioListener** (the core's are parked
   while it runs). Must be **enabled in Build Profiles**, or
   `SceneManager.LoadScene` rejects it.
6. **Data entry**: add `id: { scene_name, reward_card_id }` to
   `Assets/Settings/Game/YAML/minigames.yaml`, run *Assets → Import Content
   from YAML*.
7. **Trigger**: `MinigameTrigger` on a collider prop (its `_id` is the minigame
   id), a `minigame_id:` on a dialogues.yaml entry, or
   `Services.Minigame.Start("id")` from scene code.

No core system file is touched per minigame.

## Updating when the original repo changes

1. `git submodule update` (or re-copy) — the update pulls in new code, scenes,
   assets as-is.
2. **Expect to re-fix only the seam files**: the `IMinigame` entry component
   and any gesture file that drifted back to raw Input System. Because the repo
   sits in its own assembly and namespace, upstream class renames, new internal
   classes, or a second `GameManager` never require edits here.
3. If upstream refactored its own input reader, adapt it back to `IMinigameInput`
   — the three methods are the whole surface.

## Known boundaries (ponytail notes)

- The input channel is polled, not evented — a minigame whose controller is
  event-driven must poll in `Update`.
- `Complete` is guarded by `ActiveId` (stale-outcome rejection): a delayed
  `Complete` from an aborted run is silently dropped.
- ESC-abort, reward gating, scene parking/restore are all core-owned — a
  minigame never implements its own "leave" path.
- If a minigame needs input beyond pointers (keyboard, gamepad), extend
  `IMinigameInput` — add methods only when a real minigame needs them.