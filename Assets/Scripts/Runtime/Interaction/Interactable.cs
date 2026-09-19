using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Interaction
{
	// A single interactable: carries only an id. The InteractionService decides what the
	// id means — card-granting object, or an NPC that opens the card selection menu.
	// Not sealed: leaf scripts may subclass to replace Interact() (see MinigameTrigger).
	public class Interactable : MonoBehaviour
	{
		[SerializeField] private string _id;

		public string Id => _id;

		private void Awake()
		{
			Assert.IsFalse(string.IsNullOrEmpty(_id), "Interactable requires an id");
		}

		public virtual void Interact() => Game.Core.Services.Interactions.Interact(_id);
	}
}