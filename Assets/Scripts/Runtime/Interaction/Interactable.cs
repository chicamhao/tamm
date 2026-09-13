using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Interaction
{
	// A single interactable: carries only an id. The InteractionService decides what the
	// id means — card-granting object, or an NPC that opens the card selection menu.
	public sealed class Interactable : MonoBehaviour
	{
		[SerializeField] private string _id;

		public string Id => _id;

		private void Awake()
		{
			Assert.IsFalse(string.IsNullOrEmpty(_id), "Interactable requires an id");
		}

		public void Interact() => Game.Core.Services.Interactions.Interact(_id);
	}
}