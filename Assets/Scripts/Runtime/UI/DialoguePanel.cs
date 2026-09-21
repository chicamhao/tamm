using Game.Core;
using Game.Input;
using R3;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Game.UI
{
	// Leaf view: shows the current dialogue line; advances on Interact press or after
	// DisplayDuration (whichever comes first). Pacing lives here, state lives in DialogueService.
	public sealed class DialoguePanel : MonoBehaviour
	{
		[SerializeField] private InputMonitor _input;
		[SerializeField] private RuntimeUI _runtimeUI;

		private VisualElement _screen;
		private Label _speakerText;
		private Label _lineText;
		private IDisposable _onPlaying;
		private IDisposable _onLine;
		private float _lineTime;

		private void Start()
		{
			Assert.IsNotNull(_input, "DialoguePanel requires the player's InputMonitor assigned");
			if (Bootstrapper.Instance == null)
			{
				Debug.LogWarning("DialoguePanel: core scene not running — open Assets/Scenes/Bootstrapper.unity and press Play (it loads the Playground level)");
				return;
			}

			RuntimeUI ui = RuntimeUI.Resolve(_runtimeUI);
			Assert.IsNotNull(ui, "DialoguePanel requires a RuntimeUI in the scene");

			_screen = ui.Q("DialogueScreen");
			_speakerText = ui.Q<Label>("SpeakerText");
			_lineText = ui.Q<Label>("LineText");
			Assert.IsNotNull(_lineText, "DialoguePanel requires a LineText element");

			DialogueService dialogue = Services.Dialogue;

			_screen.style.display = DisplayStyle.None;
			_onPlaying = dialogue.IsPlaying.Subscribe(playing =>
			{
				_screen.style.display = playing ? DisplayStyle.Flex : DisplayStyle.None;
				if (playing) _lineTime = 0;
			});
			_onLine = dialogue.CurrentLine.Subscribe(line =>
			{
				if (!dialogue.IsPlaying.Value) return;
				_speakerText.text = dialogue.SpeakerName.Value;
				_lineText.text = line.Line;
				_lineTime = 0;
			});
		}

		private void Update()
		{
			DialogueService dialogue = Services.Dialogue;
			if (dialogue == null || !dialogue.IsPlaying.Value) return;

			_lineTime += Time.deltaTime;
			bool pressed = _input.GetInteractInputDown();
			float duration = dialogue.CurrentLine.Value.DisplayDuration;
			if (pressed || (duration > 0 && _lineTime >= duration)) dialogue.Advance();
		}

		private void OnDestroy()
		{
			_onPlaying?.Dispose();
			_onLine?.Dispose();
		}
	}
}