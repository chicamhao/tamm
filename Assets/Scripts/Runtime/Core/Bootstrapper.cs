using Game.Content;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.Core
{
	// Composition root — the ONE MonoBehaviour that knows every service.
	// Persistent Core Model: this GameObject is marked DontDestroyOnLoad in Awake,
	// so it survives every scene transition. Levels load additively on top via
	// LoadLevel; the core is never teared down. OnDestroy only fires on app quit.
	[Unity.Scripting.LifecycleManagement.AutoStaticsCleanup]
	public sealed partial class Bootstrapper : MonoBehaviour
	{
		public static Bootstrapper Instance { get; private set; }

		private string _currentLevel; // additive level scene currently loaded, if any
		private string _overlayLevel; // minigame scene layered over the level, if any (popped first by UnloadLevel)

		[SerializeField] private GameSettings _settings; // hub: content refs + input default

		/// <summary>Level loaded additively on boot (the world the player starts in).</summary>
		[SerializeField] private string _initialLevel = "Playground";

		// Services tear down in reverse construction order (see Awake).
		private readonly List<System.IDisposable> _disposables = new();

		public CardInventory Cards { get; private set; }
		public InteractionService Interactions { get; private set; }
		public DialogueService Dialogue { get; private set; }
		public MinigameService Minigames { get; private set; }
		public ChapterState Chapter { get; private set; }
		public InputSettings InputSettings { get; private set; }
		public PauseState Pause { get; private set; }
		public ProgressStore Progress { get; private set; }

		private void Awake()
		{
			Object.DontDestroyOnLoad(gameObject); // persistent core: survives level transitions
			Instance = this;
			Assert.IsNotNull(_settings, "Bootstrapper requires a GameSettings asset assigned");
			Assert.IsNotNull(_settings.Cards, "GameSettings requires CardSettings assigned");
			Assert.IsNotNull(_settings.Dialogues, "GameSettings requires DialogueSettings assigned");

			// Construction order == dependency order; add Gui, Save, etc. here (deps first).
			var inputSettings = new InputSettings(_settings);
			_disposables.Add(inputSettings);
			InputSettings = inputSettings;

			Pause = new PauseState();
			_disposables.Add(Pause);

			Cards = new CardInventory();
			_disposables.Add(Cards);

			Interactions = new InteractionService(
				Cards,
				new List<string>(_settings.Cards.Entries.Keys).ToArray(),
				InteractionService.DeriveActorIds(
					_settings.Dialogues.Entries.Keys,
					_settings.Chapters != null ? _settings.Chapters.Entries.Keys : null,
					_settings.Cards.Entries.Keys).ToArray());
			_disposables.Add(Interactions);

			Minigames = new MinigameService(Cards, _settings.Minigames);

			Dialogue = new DialogueService(Cards, _settings.Dialogues, Minigames);
			_disposables.Add(Dialogue);

			Chapter = new ChapterState(Cards, Dialogue, _settings.Chapters);
			_disposables.Add(Chapter);

			Progress = new ProgressStore(Cards, Dialogue, Chapter);

			Progress.Load(); // resume persisted progress on boot (cards, conversations, chapter)
		}

		// Boot the world level additively on top of the core once every service is up.
		// Start (not Awake): the scene is fully activated, and the level's own Awake/Start
		// run next frame after the additive load, so nothing races the core's construction.
		private void Start()
		{
			LoadLevel(_initialLevel);
		}

		public static string CurrentLevel => Instance != null ? Instance._currentLevel : null;

		// ESC while a minigame runs abandons it (no reward, back to the world).
		// The poll lives here (the one persistent core Mono) while the policy lives
		// in MinigameService — every minigame gets the escape hatch for free.
		private void Update()
		{
			if (Instance != this || Minigames == null) return; // core not initialized; leaf guards handle the UI
			if (Minigames.ActiveId != null &&
				Keyboard.current != null &&
				Keyboard.current.escapeKey.wasPressedThisFrame)
			{
				Minigames.Abort();
			}
		}

		// Loads a level scene additively on top of the persistent core, unloading the
		// previous level. Use this instead of SceneManager.LoadScene — a plain (non-additive)
		// load would destroy the Bootstrapper scene; additive keeps the core alive.
		// The stored level name is normalized (path or plain name both accepted) because
		// UnloadSceneAsync only takes a scene NAME, never an asset path.
		// A request for a different level while the previous load is still in flight is
		// dropped (LoadScene is async; stacking a second additively would wedge two levels
		// in the hierarchy). Retry once the scene is loaded (GetSceneByName returns a
		// non-loaded Scene while the request is in flight or after it fails). The async
		// unload stays fire-and-forget; unloads are one frame, loads are the slow path.
		public static void LoadLevel(string name)
		{
			Assert.IsNotNull(Instance, "LoadLevel requires the core scene to be running");
			string level = System.IO.Path.GetFileNameWithoutExtension(name);
			if (level.Equals(Instance._currentLevel)) return; // already there, no-op
			// Already in the hierarchy = staged in the editor scene stack (or a recent
			// load that hasn't been tracked). Adopt it instead of additive-loading a duplicate.
			Scene scene = SceneManager.GetSceneByName(level);
			if (scene != null && scene.isLoaded)
			{
				Instance._currentLevel = level;
				return;
			}
			if (Instance._currentLevel != null && !SceneManager.GetSceneByName(Instance._currentLevel).isLoaded)
				return; // previous load in flight; the scene isn't in the hierarchy yet
			if (Instance._currentLevel != null) SceneManager.UnloadSceneAsync(Instance._currentLevel);
			SceneManager.LoadScene(name, LoadSceneMode.Additive);
			Instance._currentLevel = level;
		}

		// Loads a minigame scene additively over the current level WITHOUT unloading
		// it, so the level's state survives the overlay and resumes on unload.
		public static void LoadOverlay(string name)
		{
			Assert.IsNotNull(Instance, "LoadOverlay requires the core scene to be running");
			Assert.IsNull(Instance._overlayLevel, "LoadOverlay: a minigame overlay is already loaded");
			SceneManager.LoadScene(name, LoadSceneMode.Additive);
			Instance._overlayLevel = System.IO.Path.GetFileNameWithoutExtension(name);
		}

		// Drops the top of the scene stack: an overlay (minigame) when one is loaded,
		// otherwise the level itself.
		public static void UnloadLevel()
		{
			Assert.IsNotNull(Instance, "UnloadLevel requires the core scene to be running");
			if (Instance._overlayLevel != null)
			{
				SceneManager.UnloadSceneAsync(Instance._overlayLevel);
				Instance._overlayLevel = null;
				return;
			}
			if (Instance._currentLevel == null)
				return;
			SceneManager.UnloadSceneAsync(Instance._currentLevel);
			Instance._currentLevel = null;
		}

		private void OnDestroy()
		{
			if (Instance == this) Instance = null;
			for (int i = _disposables.Count - 1; i >= 0; --i)
				_disposables[i].Dispose();
			_disposables.Clear();
		}
	}
}
