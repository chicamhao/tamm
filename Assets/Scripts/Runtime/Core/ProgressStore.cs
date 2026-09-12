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

		public ProgressStore(GameSession session) => _session = session;

		public void Save() => PlayerPrefs.SetInt(PrefsPrefix + "collected", _session.Collected.Value);

		public void Load() => _session.RestoreProgress(PlayerPrefs.GetInt(PrefsPrefix + "collected", 0));

		public void Clear() => PlayerPrefs.DeleteKey(PrefsPrefix + "collected");
	}
}