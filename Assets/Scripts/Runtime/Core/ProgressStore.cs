using System;
using UnityEngine;

namespace Game.Core
{
	// Minimum persistence: save/load session progress to platform prefs.
	// ponytail: PlayerPrefs key/value, not a file saver — upgrade to a save-file
	// format the day a save holds more than a counter.
	public sealed class ProgressStore
	{
		private const String PrefsPrefix = "game.progress.";

		private GameSession _session;
		private CardInventory _cards;

		public ProgressStore(GameSession session, CardInventory cards)
		{
			_session = session;
			_cards = cards;
		}

		public void Save()
		{
			PlayerPrefs.SetInt(PrefsPrefix + "collected", _session.Collected.Value);
			PlayerPrefs.SetString(PrefsPrefix + "cards", string.Join(",", _cards.Owned));
		}

		public void Load()
		{
			_session.RestoreProgress(PlayerPrefs.GetInt(PrefsPrefix + "collected", 0));
			_cards.GrantIds(PlayerPrefs.GetString(PrefsPrefix + "cards", "")
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
		}

		public void Clear()
		{
			PlayerPrefs.DeleteKey(PrefsPrefix + "collected");
			PlayerPrefs.DeleteKey(PrefsPrefix + "cards");
		}
	}
}