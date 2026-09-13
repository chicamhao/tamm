using UnityEngine;

namespace Game.Editor
{
	/// <summary>
	/// Batch-mode/dev command: force-advance the persisted chapter, unconditionally —
	/// bypasses all gates. The next boot auto-loads it (Bootstrapper calls Progress.Load()).
	///
	/// CLI: Unity.exe -batchmode -quit -projectPath &lt;path&gt; -executeMethod Game.Editor.ChapterDebug.AdvanceChapter
	/// </summary>
	public static class ChapterDebug
	{
		[UnityEditor.MenuItem("Tools/Advance Chapter (persisted)")]
		public static void AdvanceChapter()
		{
			int chapter = PlayerPrefs.GetInt(Game.Core.ProgressStore.ChapterPrefsKey, 1);
			PlayerPrefs.SetInt(Game.Core.ProgressStore.ChapterPrefsKey, chapter + 1);
			PlayerPrefs.Save(); // immediate flush for batch mode
			Debug.Log($"[ChapterDebug] Chapter advanced to {chapter + 1} (unconditional)");
		}
	}
}