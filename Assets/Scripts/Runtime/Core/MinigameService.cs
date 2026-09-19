using Game.Content;
using UnityEngine;

namespace Game.Core
{
	// Minigame host: the single place that knows how a minigame scene starts and ends.
	// Scenes load additively on the persistent core (Bootstrapper.LoadLevel); the
	// service bridges the scene's outcome back into the narrative - a win grants the
	// entry's reward card (which is how chapter gates and persistence already see it,
	// no new save data), then returns to the level the minigame interrupted.
	public sealed class MinigameService
	{
		private readonly MinigameSettings _settings;
		private readonly CardInventory _cards;

		private string _previousLevel;

		public MinigameService(CardInventory cards, MinigameSettings settings)
		{
			_cards = cards;
			_settings = settings;
		}

		/// <summary>Launches the minigame scene additively, remembering the level to return to.</summary>
		public void Start(string minigameId)
		{
			MinigameEntry entry = Resolve(minigameId);
			if (entry == null)
			{
				Debug.Log($"[Minigame] unknown id '{minigameId}'");
				return;
			}

			_previousLevel = Bootstrapper.CurrentLevel;
			Bootstrapper.LoadLevel(entry.SceneName);
		}

		/// <summary>
		/// Reports the outcome. A win grants the entry's reward card (duplicates are
		/// no-ops, so a replay cannot double-grant); then returns to the narrative
		/// level that was replaced by the minigame scene.
		/// </summary>
		public void Complete(string minigameId, bool won)
		{
			MinigameEntry entry = Resolve(minigameId);
			if (entry == null)
				return;

			if (won && !string.IsNullOrEmpty(entry.RewardCardId))
				_cards.GrantId(entry.RewardCardId);

			// ponytail: restore = full level reload (fresh spawn). If the narrative
			// ever needs to survive mid-level, switch to an unload-only path.
			if (!string.IsNullOrEmpty(_previousLevel))
				Bootstrapper.LoadLevel(_previousLevel);

			_previousLevel = null;
		}

		private MinigameEntry Resolve(string minigameId)
		{
			if (_settings == null || _settings.Entries == null) return null;
			return _settings.Entries.TryGetValue(minigameId, out MinigameEntry entry) ? entry : null;
		}
	}
}