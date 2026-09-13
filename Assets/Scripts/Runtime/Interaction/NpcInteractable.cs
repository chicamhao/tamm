using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Interaction
{
	// Actor: opens the card selection menu so the player can use a card on this NPC.
	// _actorId must match the ids in the DialogueSettings keys (e.g. "cam", "meKe").
	public sealed class NpcInteractable : Interactable
	{
		[SerializeField] private string _actorId;

		private void Awake()
		{
			Assert.IsFalse(string.IsNullOrEmpty(_actorId), "NpcInteractable requires an actor id");
		}

		public override void Interact() => Game.Core.Services.Cards.CardSelectionRequested.OnNext(_actorId);
	}
}