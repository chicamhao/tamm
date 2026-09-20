using Game.Core;
using Game.Input;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Game.UI
{
	// Pause menu: the hub's Pause action toggles it. Opening pauses the game
	// (timeScale 0) and frees the cursor through the hub; closing resumes.
	// The panel holds the sensitivity slider + save/load buttons.
	// Attach this to a persistent GameObject; the SettingsScreen element toggles.
	public sealed class SettingsMenu : MonoBehaviour
	{
		[SerializeField] private InputMonitor _input;
		[SerializeField] private float _min = 0.25f;
		[SerializeField] private float _max = 4.0f;
		[SerializeField] private RuntimeUI _runtimeUI;

		private VisualElement _screen;
		private Slider _sensitivitySlider;
		private Label _sensitivityValue;
		private Button _saveButton;
		private Button _loadButton;
		private Button _newGameButton;

		private PauseState _pause;
		private InputSettings _inputSettings;
		private ProgressStore _progress;

		private void Start()
		{
			Assert.IsNotNull(_input, "SettingsMenu requires the player's InputMonitor assigned");

			_pause = Game.Core.Services.Pause;
			_inputSettings = Game.Core.Services.InputSettings;
			_progress = Game.Core.Services.Progress;

			AssignElements();

			if (_screen != null) _screen.style.display = DisplayStyle.None;
			if (_sensitivitySlider == null) return;

			_sensitivitySlider.lowValue = _min;
			_sensitivitySlider.highValue = _max;
			_sensitivitySlider.value = _inputSettings.MouseSensitivity.Value;
			_sensitivitySlider.RegisterValueChangedCallback(evt => OnSensitivityChanged(evt.newValue));
			if (_sensitivityValue != null) _sensitivityValue.text = FormatSensitivity(_inputSettings.MouseSensitivity.Value);

			if (_saveButton != null) _saveButton.clicked += () => _progress.Save();
			if (_loadButton != null) _loadButton.clicked += () => _progress.Load();
			if (_newGameButton != null) _newGameButton.clicked += NewGame;
		}

		private void AssignElements()
		{
			RuntimeUI ui = RuntimeUI.Resolve(_runtimeUI);
			if (ui == null) return;

			_screen = ui.Q("SettingsScreen");
			_sensitivitySlider = ui.Q<Slider>("SensitivitySlider");
			_sensitivityValue = ui.Q<Label>("SensitivityValue");
			_saveButton = ui.Q<Button>("SaveButton");
			_loadButton = ui.Q<Button>("LoadButton");
			_newGameButton = ui.Q<Button>("NewGameButton");
		}

		private void Update()
		{
			if (!_input.GetPauseInputDown()) return;

			if (_screen.style.display == DisplayStyle.Flex) CloseMenu();
			else OpenMenu();
		}

		private void OpenMenu()
		{
			_screen.style.display = DisplayStyle.Flex;
			_pause.Toggle();
			_input.DisableInput(); // freeze Look/Move so the camera can't rotate under the menu
			_input.SetCursorState(false);
		}

		private void CloseMenu()
		{
			_screen.style.display = DisplayStyle.None;
			_pause.Toggle();
			_input.EnableInput();
			_input.SetCursorState(true);
		}

		private void NewGame()
		{
			_progress.Clear();
			Game.Core.Services.Cards.Clear();
		}

		private void OnSensitivityChanged(float sensitivity)
		{
			_inputSettings.MouseSensitivity.Value = sensitivity;
			if (_sensitivityValue != null) _sensitivityValue.text = FormatSensitivity(sensitivity);
		}

		private static String FormatSensitivity(float sensitivity)
		{
			int hundredths = (int)(sensitivity * 100.0f);
			return (hundredths / 100.0f).ToString() + "x";
		}
	}
}