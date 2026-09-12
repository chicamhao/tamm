using R3;
using System;
using UnityEngine;

namespace Game.Core
{
	// The whole game: collect `total` objects before the clock runs out.
	// One service owns state + rules; R3 drives the countdown on the Update loop.
	public sealed class GameSession : IDisposable
	{
		public readonly GameSettings Settings;

		public int Total => Settings.ObjectCount;
		public String WinMessage => Settings.WinMessage;
		public String LoseMessage => Settings.LoseMessage;

		public ReactiveProperty<int> Collected { get; } = new(0);
		public ReactiveProperty<float> Remaining { get; } = new(0.0f);
		public ReactiveProperty<GameResult> Result { get; } = new(GameResult.InProgress);

		private IDisposable _countdown;

		public GameSession(GameSettings settings)
		{
			Settings = settings;

			Remaining.Value = settings.TimeLimitSeconds;
			_countdown = StartCountdown();
		}

		// Rebuilds the session to a fresh start: same settings, zero progress, clock re-armed.
		public void Reset()
		{
			_countdown?.Dispose();
			Remaining.Value = Settings.TimeLimitSeconds;
			Collected.Value = 0;
			Result.Value = GameResult.InProgress;
			_countdown = StartCountdown();
		}

		private IDisposable StartCountdown()
			=> Observable.IntervalFrame(1).Subscribe(_ => Tick());

		private void Tick()
		{
			float next = Remaining.Value - Time.deltaTime;
			Remaining.Value = next > 0 ? next : 0;
			UpdateResult();
		}

		// Called by Collectible leaves, exactly once per object.
		public void Collect()
		{
			Collected.Value += 1;
			UpdateResult();
		}

		// Restores a saved count. Finished games don't resume; the count is clamped to Total.
		public void RestoreProgress(int collected)
		{
			if (Result.Value != GameResult.InProgress) return;
			Collected.Value = Mathf.Clamp(collected, 0, Total);
			UpdateResult();
		}

		// Rule reducer — pure, so the win/lose edges are unit-testable.
		public static GameResult Evaluate(int collected, int total, float remaining)
			=> collected >= total ? GameResult.Won
			 : remaining <= 0 ? GameResult.Lost
			 : GameResult.InProgress;

		private void UpdateResult()
		{
			if (Result.Value != GameResult.InProgress) return; // result is final; the clock is stopped

			GameResult next = Evaluate(Collected.Value, Total, Remaining.Value);
			if (next != GameResult.InProgress)
			{
				Result.Value = next;
				_countdown.Dispose(); // stop the clock on Win or Lose
			}
		}

		public void Dispose()
		{
			_countdown.Dispose();
			Collected.Dispose();
			Remaining.Dispose();
			Result.Dispose();
		}
	}

	public enum GameResult
	{
		InProgress,
		Won,
		Lost,
	}
}