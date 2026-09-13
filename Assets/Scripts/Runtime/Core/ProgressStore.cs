using System;
using UnityEngine;

namespace Game.Core
{
	// Minimum persistence: the player's card collection in platform prefs.
	// ponytail: comma-joined PlayerPrefs string, not a file saver — upgrade to a save-file
	// format the day a save holds dialogue flags and chapter progress too.
	public sealed class ProgressStore
	{
		private const String PrefsPrefix = "game.progress.";

		private CardInventory _cards;

		public ProgressStore(CardInventory cards) => _cards = cards;

		public void Save() => PlayerPrefs.SetString(PrefsPrefix + "cards", string.Join(",", _cards.Owned));

		public void Load() => _cards.GrantIds(PlayerPrefs.GetString(PrefsPrefix + "cards", "")
			.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

		public void Clear() => PlayerPrefs.DeleteKey(PrefsPrefix + "cards");
	}
}