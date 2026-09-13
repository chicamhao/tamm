using UnityEngine;
using System;
using System.Collections.Generic;

namespace Game.Core
{
	// Routes interaction ids: a known card id grants the card, a known actor id opens
	// the card selection menu. Scene objects only carry ids; what an id means lives here
	// (registered in Bootstrapper) so future chapter/spawn systems can drive scenes by id.
	public sealed class InteractionService : IDisposable
	{
		private readonly CardInventory _inventory;
		private readonly HashSet<string> _cardIds = new();
		private readonly HashSet<string> _actorIds = new();

		public InteractionService(CardInventory inventory, string[] cardIds, string[] actorIds)
		{
			_inventory = inventory;
			foreach (string id in cardIds ?? Array.Empty<string>()) _cardIds.Add(id);
			foreach (string id in actorIds ?? Array.Empty<string>()) _actorIds.Add(id);
		}

		/// <summary>
		/// Derive distinct actor ids from the content settings: dialogue keys
		/// ("cardId_actorId[_chapter]") strip the card prefix + optional chapter suffix,
		/// chapter keys ("actorId_chapter") strip the chapter suffix. Pure — unit-testable.
		/// </summary>
		public static List<string> DeriveActorIds(IEnumerable<string> dialogueKeys, IEnumerable<string> chapterKeys, IEnumerable<string> cardIds)
		{
			var actors = new HashSet<string>();

			foreach (string key in dialogueKeys ?? Array.Empty<string>())
			{
				string actor = StripCardPrefix(key, cardIds);
				if (actor != null) actors.Add(StripTrailingChapter(actor));
			}

			foreach (string key in chapterKeys ?? Array.Empty<string>())
			{
				actors.Add(StripTrailingChapter(key));
			}

			var sorted = new List<string>(actors);
			sorted.Sort(StringComparer.Ordinal);
			return sorted;
		}

		// Longest card id that prefixes the key ("card_cam_cam" + cards ["card_cam"] -> "cam");
		// null when no known card prefixes it (malformed key, skip).
		private static string StripCardPrefix(string key, IEnumerable<string> cardIds)
		{
			string best = null;
			foreach (string cardId in cardIds ?? Array.Empty<string>())
			{
				if (string.IsNullOrEmpty(cardId)) continue;
				string prefix = cardId + "_";
				if (key.StartsWith(prefix, StringComparison.Ordinal) && prefix.Length > (best != null ? best.Length + 1 : 0))
					best = cardId;
			}
			return best == null ? null : key.Substring(best.Length + 1);
		}

		// Drops a trailing "_digits" chapter suffix: "cam_2" -> "cam".
		private static string StripTrailingChapter(string s)
		{
			int sep = s.LastIndexOf('_');
			if (sep <= 0 || sep == s.Length - 1) return s;

			for (int i = sep + 1; i < s.Length; i++)
			{
				if (!char.IsDigit(s[i])) return s;
			}
			return s.Substring(0, sep);
		}

		public void Interact(string id)
		{
			if (_cardIds.Contains(id))
			{
				_inventory.GrantId(id);
				return;
			}

			if (_actorIds.Contains(id))
			{
				_inventory.CardSelectionRequested.OnNext(id);
				return;
			}

			Debug.LogWarning($"Interact: unknown id '{id}' — not a registered card or actor id");
		}

		public void Dispose() { }
	}
}