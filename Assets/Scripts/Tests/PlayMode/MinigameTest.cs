using Game.Content;
using Game.Core;
using Game.Minigames.Rat;
using NUnit.Framework;
using UnityEngine;

namespace Game.Minigames.Tests
{
	// Banh Đũa minigame port checks: the round-rules math that was slimmed down from
	// rice/rat (turns-per-round pattern, final round) and the win -> reward contract
	// of the minigame host (win grants the configured card, lose grants nothing).
	public sealed class MinigameTest
	{
		// Every round scatters 10 chopsticks; round r takes r per turn, remainder last
		// (round 3 runs 3 + 3 + 3 + 1). These are the traditional rules the slice ships.
		[Test]
		public void TurnPatternsFollowTheTradition()
		{
			Assert.That(RoundRules.TotalFor(1), Is.EqualTo(10));
			Assert.That(RoundRules.FinalRound, Is.EqualTo(3));

			Assert.That(RoundRules.TurnsForRound(1), Is.EquivalentTo(new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }));
			Assert.That(RoundRules.TurnsForRound(2), Is.EquivalentTo(new int[] { 2, 2, 2, 2, 2 }));
			Assert.That(RoundRules.TurnsForRound(3), Is.EquivalentTo(new int[] { 3, 3, 3, 1 }));

			Assert.That(RoundRules.RequiredForTurn(3, 10), Is.EqualTo(3));
			Assert.That(RoundRules.RequiredForTurn(3, 0), Is.EqualTo(0));
		}

		// The minigame host bridges the scene's outcome into the card economy:
		// a win grants the entry's reward card, a loss grants nothing.
		[Test]
		public void WinningGrantsTheRewardCard()
		{
			var settings = ScriptableObject.CreateInstance<MinigameSettings>();
			settings.Entries["rat"] = new MinigameEntry
			{
				SceneName = "Assets/Scenes/Rat.unity",
				RewardCardId = "card_chopsticks"
			};

			var cards = new CardInventory();
			var service = new MinigameService(cards, settings);

			service.Complete("rat", false);
			Assert.That(cards.Owns("card_chopsticks"), Is.False);

			service.Complete("rat", true);
			Assert.That(cards.Owns("card_chopsticks"), Is.True);

			// Unknown ids are no-ops, not crashes.
			service.Complete("does_not_exist", true);
			Assert.That(cards.Owns("does_not_exist"), Is.False);
		}
	}
}