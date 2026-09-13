namespace Game.Interaction
{
	// Crosshair-ray interaction target. The Interactor dispatches Interact() to the
	// hit target — a CardPickup (object, grants a card) or an NpcInteractable (actor).
	public abstract class Interactable : UnityEngine.MonoBehaviour
	{
		public abstract void Interact();
	}
}