using Game.Content;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
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

		[SerializeField] private GameSettings _settings; // hub: content refs + input default

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

		public static string CurrentLevel => Instance != null ? Instance._currentLevel : null;

		// Loads a level scene additively on top of the persistent core, unloading the
		// previous level. Use this instead of SceneManager.LoadScene — a plain (non-additive)
		// load would destroy the Bootstrapper scene; additive keeps the core alive.
		// ponytail: fire-and-forget async unload, fine while transitions are one per user action.
		public static void LoadLevel(string name)
		{
			Assert.IsNotNull(Instance, "LoadLevel requires the core scene to be running");
			if (name.Equals(Instance._currentLevel)) return; // already there, no-op
			if (Instance._currentLevel != null) SceneManager.UnloadSceneAsync(Instance._currentLevel);
			SceneManager.LoadScene(name, LoadSceneMode.Additive);
			Instance._currentLevel = name;
		}

		private void OnDestroy()
		{
			if (Instance == this) Instance = null;
			_container?.Dispose();
		}
	}
}