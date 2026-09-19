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

		private Container _container;

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

			_container = new Container();
			// Provide order == construction order; add Gui, Save, etc. here (deps first).
			_container.Provide(_settings);
			_container.Provide(new InputSettings(_settings));
			_container.Provide(new PauseState());
			_container.Provide(new CardInventory());
			_container.Provide(g => new InteractionService(
				g.Grab<CardInventory>(),
				new List<string>(_settings.Cards.Entries.Keys).ToArray(),
				InteractionService.DeriveActorIds(
					_settings.Dialogues.Entries.Keys,
					_settings.Chapters != null ? _settings.Chapters.Entries.Keys : null,
					_settings.Cards.Entries.Keys).ToArray()));
			_container.Provide(g => new MinigameService(g.Grab<CardInventory>(), _settings.Minigames));
			_container.Provide(g => new DialogueService(g.Grab<CardInventory>(), _settings.Dialogues, g.Grab<MinigameService>()));
			_container.Provide(g => new ChapterState(g.Grab<CardInventory>(), g.Grab<DialogueService>(), _settings.Chapters));
			_container.Provide(g => new ProgressStore(g.Grab<CardInventory>(), g.Grab<DialogueService>(), g.Grab<ChapterState>()));

			// Pull order == wiring order. Add Gui, Save, etc. here as they appear.
			Cards = _container.Grab<CardInventory>();
			Interactions = _container.Grab<InteractionService>();
			Dialogue = _container.Grab<DialogueService>();
			Minigames = _container.Grab<MinigameService>();
			Chapter = _container.Grab<ChapterState>();
			InputSettings = _container.Grab<InputSettings>();
			Pause = _container.Grab<PauseState>();
			Progress = _container.Grab<ProgressStore>();

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
			_container?.Dispose();
		}
	}
}
