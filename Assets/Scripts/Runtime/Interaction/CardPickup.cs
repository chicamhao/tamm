using Game.Content;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Interaction
{
	// Object: grants its card once, then disappears.
	public sealed class CardPickup : Interactable
	{
		[SerializeField] private CardDefinition _card;

		private bool _taken;

		private void Awake()
		{
			Assert.IsNotNull(_card, "CardPickup requires a CardDefinition assigned");
		}

		public override void Interact()
		{
			if (_taken) return;
			_taken = true;
			Game.Core.Services.Cards.Grant(_card);
			gameObject.SetActive(false);
		}
	}
}