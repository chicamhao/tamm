using R3;
using System;
using UnityEngine;

namespace Game.Core
{
	// Single owner of game speed. Pausing freezes simulation time (Time.timeScale);
	// the UI and loop-ticking subscriptions keep running — they just read deltaTime = 0.
	public sealed class PauseState : IDisposable
	{
		public ReactiveProperty<bool> IsPaused { get; } = new(false);

		public PauseState()
		{
			IsPaused.Subscribe(paused => Time.timeScale = paused ? 0.0f : 1.0f);
		}

		public void Toggle() => IsPaused.Value = !IsPaused.Value;

		public void Dispose() => IsPaused.Dispose();
	}
}