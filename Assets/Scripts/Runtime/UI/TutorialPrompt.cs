using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
	// First-run hint overlay (top-left). Shows only in exported builds — in-editor
	// play is the dev loop and skips it (devs have F2 DebugMenu instead). Auto-fades.
	public sealed class TutorialPrompt : MonoBehaviour
	{
		[SerializeField] private float _lifespan = 7.0f;
		[SerializeField] private RuntimeUI _runtimeUI;

		private VisualElement _box;
		private float _remaining;

		private void Start()
		{
			if (Application.isEditor)
			{
				enabled = false; // build-only prompt
				return;
			}

			RuntimeUI ui = RuntimeUI.Resolve(_runtimeUI);
			if (ui == null)
			{
				enabled = false;
				return;
			}

			_box = ui.Q("TutorialBox");
			if (_box == null)
			{
				enabled = false;
				return;
			}

			_box.style.display = DisplayStyle.Flex;
			_remaining = _lifespan;
		}

		private void Update()
		{
			_remaining -= Time.deltaTime;
			if (_remaining <= 0f)
				_box.style.display = DisplayStyle.None;
		}
	}
}