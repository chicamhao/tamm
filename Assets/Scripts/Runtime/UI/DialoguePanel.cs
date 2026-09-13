using Game.Core;
using Game.Input;
using R3;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.UI
{
	// Leaf view: shows the current dialogue line; advances on Interact press or after
	// DisplayDuration (whichever comes first). Pacing lives here, state lives in DialogueService.
	public sealed class DialoguePanel : MonoBehaviour
	{
		[SerializeField] private GameObject _panel;
		[SerializeField] private TMP_Text _speakerText;
		[SerializeField] private TMP_Text _lineText;
		[SerializeField] private InputMonitor _input;

		private IDisposable _onPlaying;
		private IDisposable _onLine;
		private float _lineTime;

		private void Start()
		{
			Assert.IsNotNull(_panel, "DialoguePanel requires _panel");
			Assert.IsNotNull(_speakerText, "DialoguePanel requires _speakerText");
			Assert.IsNotNull(_lineText, "DialoguePanel requires _lineText");
			Assert.IsNotNull(_input, "DialoguePanel requires the player's InputMonitor assigned");

			DialogueService dialogue = Services.Dialogue;
			if (dialogue == null) return; // outside a Bootstrapper scene

			_panel.SetActive(false);
			_onPlaying = dialogue.IsPlaying.Subscribe(playing =>
			{
				_panel.SetActive(playing);
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