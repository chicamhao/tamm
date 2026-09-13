using Game.Core;
using UnityEngine;

namespace Game.Debug
{
	// Dev overlay: F2 toggles, arrows step the chapter unconditionally (gates bypassed).
	// A pure IMGUI leaf — no prefabs, no scene wiring beyond dropping this on a GameObject.
	// Lives in Game.Debug (editor/dev builds only — see Debug.asmdef).
	// ponytail: direct Input reads = dev-only deviation from the "InputMonitor owns devices"
	// invariant; fold into InputMonitor actions the day debug input ships.
	public sealed class DebugMenu : MonoBehaviour
	{
		private bool _open;

		private void Update()
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.F2)) _open = !_open;
			if (!_open) return;

			ChapterState chapter = Services.Chapter;
			if (chapter == null) return;

			if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
				chapter.ForceAdvance();
			else if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
				chapter.Restore(chapter.CurrentChapter.Value - 1);
		}

		private void OnGUI()
		{
			if (!_open) return;

			ChapterState chapter = Services.Chapter;
			if (chapter == null) return;

			CardInventory cards = Services.Cards;
			string cardList = cards != null ? string.Join(", ", cards.Owned) : "—";

			GUI.Box(new Rect(10, 10, 280, 100), "DEBUG — F2 close");
			GUILayout.BeginArea(new Rect(22, 30, 260, 80));
			GUILayout.Label($"Chapter: {chapter.CurrentChapter.Value}");
			GUILayout.Label($"Cards:   {cardList}");
			GUILayout.Label("↑ advance chapter   ↓ back a chapter");
			GUILayout.EndArea();
		}
	}
}