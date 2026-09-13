using Game.Content;
using R3;
using System;
using System.Collections.Generic;

namespace Game.Core
{
	// The player's card collection: grant-once ownership, plus the card-use request
	// fired when the player interacts with an NPC. The dialogue system consumes
	// CardSelected next; until then the selection menu is the end of the flow.
	public sealed class CardInventory : IDisposable
	{
		private readonly HashSet<string> _owned = new();

		public IReadOnlyCollection<string> Owned => _owned;

		/// <summary>Fires with the cardId whenever the player acquires a card.</summary>
		public Subject<string> Granted { get; } = new();

		/// <summary>Fires with the actorId when the player uses a card on an NPC.</summary>
		public Subject<string> CardSelectionRequested { get; } = new();

		public bool Owns(string cardId) => _owned.Contains(cardId);

		public void Grant(CardDefinition card)
		{
			if (card == null || !_owned.Add(card.CardID)) return;
			Granted.OnNext(card.CardID);
		}

		// Restore from save: adds without firing, so a boot doesn't spam the UI.
		public void GrantIds(IEnumerable<string> ids)
		{
			foreach (string id in ids) _owned.Add(id);
		}

		public void Dispose()
		{
			Granted.Dispose();
			CardSelectionRequested.Dispose();
		}
	}
}