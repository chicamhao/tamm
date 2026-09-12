using Game.Core;
using Game.Input;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Game.UI
{
	// Pause menu: the hub's Pause action toggles it. Opening pauses the game
	// (timeScale 0) and frees the cursor through the hub; closing resumes.
	// The panel holds the sensitivity slider + save/load buttons.
	// Attach this to a persistent GameObject; _panel toggles the actual UI.
	public sealed class SettingsMenu : MonoBehaviour
	{
		[SerializeField] private InputMonitor _input;
		[SerializeField] private GameObject _panel;
		[SerializeField] private Slider _sensitivitySlider;
		[SerializeField] private TMP_Text _sensitivityText;
		[SerializeField] private Button _saveButton;
		[SerializeField] private Button _loadButton;
		[SerializeField] private Button _newGameButton;
		[SerializeField] private float _min = 0.25f;
		[SerializeField] private float _max = 4.0f;

		private PauseState _pause;
		private InputSettings _inputSettings;
		private ProgressStore _progress;

		private void Start()
		{
			Assert.IsNotNull(_input, "SettingsMenu requires the player's InputMonitor assigned");

			_pause = Game.Core.Services.Pause;
			_inputSettings = Game.Core.Services.InputSettings;
			_progress = Game.Core.Services.Progress;

			if (_panel != null) _panel.SetActive(false);
			if (_inputSettings == null || _sensitivitySlider == null) return; // outside a Bootstrapper scene

			_sensitivitySlider.minValue = _min;
			_sensitivitySlider.maxValue = _max;
			_sensitivitySlider.value = _inputSettings.MouseSensitivity.Value;
			_sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            if (_sensitivityText != null) _sensitivityText.text = FormatSensitivity(_inputSettings.MouseSensitivity.Value);

            _saveButton?.onClick.AddListener(() => _progress?.Save());
			_loadButton?.onClick.AddListener(() => _progress?.Load());
			_newGameButton?.onClick.AddListener(() =>
			{
				_progress?.Clear();
				Game.Core.Services.Session?.Reset();
			});
		}

		private void Update()
		{
			if (!_input.GetPauseInputDown()) return;

			if (_panel.activeInHierarchy) CloseMenu();
			else OpenMenu();
		}

		private void OpenMenu()
		{
			_panel.SetActive(true);
			_pause?.Toggle();
			_input.DisableInput(); // freeze Look/Move so the camera can't rotate under the menu
			_input.SetCursorState(false);
		}

		private void CloseMenu()
		{
			_panel.SetActive(false);
			_pause?.Toggle();
			_input.EnableInput();
			_input.SetCursorState(true);
		}

		private void OnSensitivityChanged(float sensitivity)
		{
			_inputSettings.MouseSensitivity.Value = sensitivity;
			if (_sensitivityText != null) _sensitivityText.text = FormatSensitivity(sensitivity);
		}

		private static String FormatSensitivity(float sensitivity)
		{
			int hundredths = (int)(sensitivity * 100.0f);
			return (hundredths / 100.0f).ToString() + "x";
		}
	}
}