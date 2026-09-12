using NUnit.Framework;

namespace Game.Core
{
	public sealed class GameSessionTest
	{
		[Test] public void Evaluate_InProgress_WhileCollectingWithinTime()
			=> Assert.That(GameSession.Evaluate(1, 3, 45.0f), Is.EqualTo(GameResult.InProgress));

		[Test] public void Evaluate_Won_WhenAllCollected()
			=> Assert.That(GameSession.Evaluate(3, 3, 30.0f), Is.EqualTo(GameResult.Won));

		[Test] public void Evaluate_Lost_WhenTimeRunsOut()
			=> Assert.That(GameSession.Evaluate(2, 3, 0.0f), Is.EqualTo(GameResult.Lost));

		[Test] public void Evaluate_CollectedTakesPrecedence_AtTheBoundary()
			// UpdateResult's InProgress lock decides the simultaneous case; this documents the reducer's order.
			=> Assert.That(GameSession.Evaluate(3, 3, 0.0f), Is.EqualTo(GameResult.Won));

		// --- RestoreProgress ---

		[Test] public void Restore_Partial_KeepsGameInProgress()
			{
				GameSession s = NewSession();
				s.RestoreProgress(2);
				Assert.That(s.Collected.Value, Is.EqualTo(2));
				Assert.That(s.Result.Value, Is.EqualTo(GameResult.InProgress));
				s.Dispose();
			}

		[Test] public void Restore_ClampsToTotal_AndCompletesGame()
			{
				GameSession s = NewSession();
				s.RestoreProgress(99);
				Assert.That(s.Collected.Value, Is.EqualTo(3));
				Assert.That(s.Result.Value, Is.EqualTo(GameResult.Won));
				s.Dispose();
			}

		[Test] public void Restore_Ignored_WhenGameFinished()
			{
				GameSession s = NewSession();
				s.RestoreProgress(3); // -> Won
				s.RestoreProgress(0);
				Assert.That(s.Collected.Value, Is.EqualTo(3), "a finished session must not be rewound");
				s.Dispose();
			}

		// --- Reset ---

		[Test] public void Reset_FromLost_ReturnsToInProgressZeroed()
			{
				GameSession s = NewSession();
				s.RestoreProgress(99); // -> Won
				s.Reset();
				Assert.That(s.Result.Value, Is.EqualTo(GameResult.InProgress));
				Assert.That(s.Collected.Value, Is.EqualTo(0));
				Assert.That(s.Remaining.Value, Is.EqualTo(s.Settings.TimeLimitSeconds));
				s.Dispose();
			}

		[Test] public void Reset_KeepsConfiguredTotal()
			{
				GameSession s = NewSession();
				s.Reset();
				Assert.That(s.Total, Is.EqualTo(3));
				s.Dispose();
			}

		private static GameSession NewSession()
			{
				GameSettings settings = new GameSettings();
				return new GameSession(settings);
			}
	}
}