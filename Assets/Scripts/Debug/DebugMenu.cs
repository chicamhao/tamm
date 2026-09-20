using Game.Core;
using Game.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Debug
{
	// Dev overlay: F2 toggles, arrows step the chapter unconditionally (gates bypassed).
	// A UI Toolkit leaf shared by every level's runtime panel — no scene wiring beyond
	// dropping this component on a GameObject.
	// Lives in Game.Debug (editor/dev builds only — see Debug.asmdef).
	// ponytail: direct Input reads = dev-only deviation from the "InputMonitor owns devices"
	// invariant; fold into InputMonitor actions the day debug input ships.
	public sealed class DebugMenu : MonoBehaviour
	{
		[SerializeField] private RuntimeUI _runtimeUI;

		private VisualElement _screen;
		private Label _chapterLabel;
		private Label _cardsLabel;
		private bool _open;

		private void Start()
		{
			RuntimeUI ui = RuntimeUI.Resolve(_runtimeUI);
			if (ui == null)
			{
				enabled = false;
				return;
			}

			_screen = ui.Q("DebugScreen");
			_chapterLabel = ui.Q<Label>("DebugChapter");
			_cardsLabel = ui.Q<Label>("DebugCards");
			_screen.style.display = DisplayStyle.None;
		}

		private void Update()
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.F2)) _open = !_open;
			if (!_open)
			{
				if (_screen.style.display != DisplayStyle.None)
					_screen.style.display = DisplayStyle.None;
				return;
			}

			if (_screen.style.display != DisplayStyle.Flex)
				_screen.style.display = DisplayStyle.Flex;

			ChapterState chapter = Services.Chapter;
			if (chapter == null) return;

			CardInventory cards = Services.Cards;
			_chapterLabel.text = "Chapter: " + chapter.CurrentChapter.Value;
			_cardsLabel.text = "Cards:   " + (cards != null ? string.Join(", ", cards.Owned) : "—");

			if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
				chapter.ForceAdvance();
			else if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
				chapter.Restore(chapter.CurrentChapter.Value - 1);
		}

	}
}