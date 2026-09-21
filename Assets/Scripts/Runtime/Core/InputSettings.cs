using R3;
using System;
using UnityEngine;

namespace Game.Core
{
	// Runtime-adjustable input settings: slider -> ReactiveProperty -> consumers.
	// Defaults are data-driven from GameSettings; the UI overwrites at runtime.
	// PrefsKey is a constant — never cleaned between scenes.
	public sealed class InputSettings : IDisposable
	{
		private static readonly string PrefsKey = "game.input.mouseSensitivity";

		public ReactiveProperty<float> MouseSensitivity { get; } = new(1.0f);

		public InputSettings(GameSettings settings)
		{
			// Saved runtime override wins over the data-driven default; the override
			// persists on every change (subscribe comes after the seed, so no initial write).
			MouseSensitivity.Value = PlayerPrefs.GetFloat(PrefsKey, settings.MouseSensitivity);
			MouseSensitivity.Subscribe(sensitivity => PlayerPrefs.SetFloat(PrefsKey, sensitivity));
		}

		public void Dispose() => MouseSensitivity.Dispose();
	}
}
