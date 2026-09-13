using System;
using UnityEngine;

namespace Game.Core
{
	// Minimum persistence: cards, conducted conversations, and the current chapter,
	// as platform-prefs strings/ints. Upgrade to a file format the day a save grows.
	public sealed class ProgressStore
	{
		private const String PrefsPrefix = "game.progress.";

		/// <summary>Persisted chapter key — shared with editor CLI helpers.</summary>
		public const String ChapterPrefsKey = PrefsPrefix + "chapter";

		private CardInventory _cards;
		private DialogueService _dialogue;
		private ChapterState _chapter;

		public ProgressStore(CardInventory cards, DialogueService dialogue, ChapterState chapter)
		{
			_cards = cards;
			_dialogue = dialogue;
			_chapter = chapter;
		}

		private static String[] Csv(String value) => value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

		public void Save()
		{
			PlayerPrefs.SetString(PrefsPrefix + "cards", string.Join(",", _cards.Owned));
			PlayerPrefs.SetString(PrefsPrefix + "conversations", string.Join(",", _dialogue.Conducted));
			PlayerPrefs.SetInt(PrefsPrefix + "chapter", _chapter.CurrentChapter.Value);
		}

		public void Load()
		{
			_cards.GrantIds(Csv(PlayerPrefs.GetString(PrefsPrefix + "cards", "")));
			_dialogue.RestoreConducted(Csv(PlayerPrefs.GetString(PrefsPrefix + "conversations", "")));
			_chapter.Restore(PlayerPrefs.GetInt(PrefsPrefix + "chapter", 1));
		}

		public void Clear()
		{
			PlayerPrefs.DeleteKey(PrefsPrefix + "cards");
			PlayerPrefs.DeleteKey(PrefsPrefix + "conversations");
			PlayerPrefs.DeleteKey(PrefsPrefix + "chapter");
		}
	}
}