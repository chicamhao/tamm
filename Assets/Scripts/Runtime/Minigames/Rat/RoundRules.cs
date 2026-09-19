using UnityEngine;

namespace Game.Minigames.Rat
{
	/// <summary>
	/// Stage 1 round rules for Banh Đũa, rounds 1-3 (Nhăt Một / Hai / Ba).
	/// Ported from rice/rat with the balance table removed: these are the built-in
	/// traditional rules the prototype falls back to when no table is authored.
	/// </summary>
	public static class RoundRules
	{
		/// <summary>Chopsticks on the table at the start of every round.</summary>
		public const int TotalChopsticks = 10;

		/// <summary>Last playable round (the current slice stops at Nhăt Ba).</summary>
		public const int FinalRound = 3;

		/// <summary>Chopsticks on the table at the start of this round.</summary>
		public static int TotalFor(int round) => TotalChopsticks;

		/// <summary>
		/// How many chopsticks this turn takes. Traditionally round r takes r at a time
		/// until fewer than r remain, then the remainder - so round 3 runs 3 + 3 + 3 + 1.
		/// </summary>
		public static int RequiredForTurn(int round, int remaining)
		{
			if (remaining <= 0) return 0;
			return Mathf.Min(Mathf.Max(round, 1), remaining);
		}

		/// <summary>The full turn breakdown, e.g. round 3 -> [3, 3, 3, 1].</summary>
		public static int[] TurnsForRound(int round)
		{
			var turns = new System.Collections.Generic.List<int>();

			int remaining = TotalChopsticks;

			while (remaining > 0)
			{
				int take = RequiredForTurn(round, remaining);

				if (take <= 0) break;

				turns.Add(take);

				remaining -= take;
			}

			return turns.ToArray();
		}

		/// <summary>Whether drag-to-collect is unlocked. Tapping is always available.</summary>
		public static bool AllowSweep(int round) => false;

		/// <summary>Verb for the prompt, e.g. TAP.</summary>
		public static string PromptVerb(int round) => "TAP";

		/// <summary>Traditional Vietnamese name.</summary>
		public static string RoundName(int round)
		{
			switch (round)
			{
				case 1: return "NHẶT MỘT";
				case 2: return "NHẶT HAI";
				case 3: return "NHẶT BA";
				default: return "ROUND " + round;
			}
		}

		/// <summary>"3 + 3 + 3 + 1", for the round intro.</summary>
		public static string TurnBreakdown(int round) => string.Join(" + ", TurnsForRound(round));
	}
}