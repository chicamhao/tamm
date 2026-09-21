using Game.Content;
using Game.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
	// Minigame host: the single place that knows how a minigame scene starts and ends,
	// and how the core scene hands itself over while one runs.
	// The minigame loads additively as an overlay on the persistent core
	// (Bootstrapper.LoadOverlay) and drops without touching the level, so the
	// playground's state survives the interruption; the service bridges the scene's
	// outcome back into the narrative - a win grants the entry's reward card (which
	// is how chapter gates and persistence already see it, no new save data), then
	// the overlay drops and the interrupted level resumes where it was.
	// frozen (InputMonitor gate, same lever as SettingsMenu/CardSelectionMenu), the
	// pointer is freed for gestures, and the core scene's cameras / audio listener /
	// event system are disabled so the minigame scene owns the screen and audio.
	public sealed class MinigameService
	{
		private readonly MinigameSettings _settings;
		private readonly CardInventory _cards;

		private InputMonitor _playerInput;

		/// <summary>Id of the minigame currently running (null when none). Drives
		/// ESC-abort and stale-outcome rejection.</summary>
		public string ActiveId { get; private set; }

		/// <summary>Roots parked while a minigame owns the world (set back active in Complete).</summary>
		private readonly List<GameObject> _parkedRoots = new();

		/// <summary>ESC abort wants the re-lock deferred (web only, see RestoreCoreScene).</summary>
		private bool _deferRelockOnRestore;

        public MinigameService(CardInventory cards, MinigameSettings settings)
        {
            _cards = cards;
            _settings = settings;
        }

		/// <summary>Launches the minigame scene as an overlay.</summary>
		public void Start(string minigameId)
		{
			MinigameEntry entry = Resolve(minigameId);
			if (entry == null)
			{
				Debug.Log($"[Minigame] unknown id '{minigameId}'");
				return;
			}

			ActiveId = minigameId;

			// Hand the screen and input to the minigame BEFORE the scene loads, so its
			// Awake/Start phases see a free pointer and Camera.main resolves to its own
			// camera (the core's are already off, not fighting for the lookup).
			TakeOverCoreScene();

			Bootstrapper.LoadOverlay(entry.SceneName);
		}

		/// <summary>
		/// Reports the outcome. A win grants the entry's reward card (duplicates are
		/// no-ops, so a replay cannot double-grant); then drops to the overlay
		/// and hand the core back, resuming the interrupted level where it was.
		/// </summary>
		public void Complete(string minigameId, bool won)
		{
			if (minigameId != ActiveId)
				return; // stale outcome: already aborted/completed (nested Complete can only happen once)
			ActiveId = null;

			MinigameEntry entry = Resolve(minigameId);
			if (entry == null)
				return;

			if (won && !string.IsNullOrEmpty(entry.RewardCardId))
				_cards.GrantId(entry.RewardCardId);

            Bootstrapper.UnloadLevel();
			RestoreCoreScene();
		}

		/// <summary>Abandons the active minigame: no reward, straight back to the narrative level.</summary>
		public void Abort()
		{
			if (ActiveId != null)
				Complete(ActiveId, false);
		}

		// =========================================
		// CORE SCENE TAKEOVER / RESTORE
		// =========================================

		private void TakeOverCoreScene()
		{
			// Player input: freeze Move/Look/Jump/Sprint/Interact and free the pointer.
			// Null when played standalone (Bootstrapper + minigame, no player prefab).
			_playerInput = Object.FindAnyObjectByType<InputMonitor>();
			if (_playerInput != null)
			{
				_playerInput.DisableInput();
				_playerInput.SetCursorState(false);
			}

			// Park the core/level world: the minigame loads additively into the SAME world,
			// so its camera would still draw every playground mesh in its frustum - hiding
			// just the camera wasn't enough. Disable every loaded scene's root objects but
			// the persistent Bootstrapper; the minigame's own roots (not loaded yet) stay
			// active. This also covers cameras, audio listeners, event systems and the
			// player in one sweep. Roots already inactive are left alone so restore doesn't
			// fight scene-authored states; destroyed roots (level unload/reload) are
			// skipped by the null guard in RestoreCoreScene.
			_parkedRoots.Clear();

			GameObject keeper = Bootstrapper.Instance != null ? Bootstrapper.Instance.gameObject : null;

			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene scene = SceneManager.GetSceneAt(i);
				if (!scene.isLoaded) continue;

				foreach (GameObject root in scene.GetRootGameObjects())
				{
					if (root == keeper || !root.activeSelf) continue;
					_parkedRoots.Add(root);
					root.SetActive(false);
				}
			}
		}

		private void RestoreCoreScene()
		{
			if (_playerInput != null)
			{
				_playerInput.EnableInput();
				_playerInput.SetCursorState(true);
				_playerInput = null;
			}

			foreach (GameObject root in _parkedRoots)
			{
				if (root != null) root.SetActive(true);
			}
			_parkedRoots.Clear();
		}

		private MinigameEntry Resolve(string minigameId)
		{
			if (_settings == null || _settings.Entries == null) return null;
			return _settings.Entries.TryGetValue(minigameId, out MinigameEntry entry) ? entry : null;
		}
	}
}
