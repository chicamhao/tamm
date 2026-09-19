using Game.Core;

namespace Game.Interaction
{
	// A MinigameTrigger is an Interactable whose id doubles as the minigame id:
	// interacting launches the core MinigameService instead of the interaction
	// rule table. Reward, persistence and the return path are all owned by
	// MinigameService; Interactor handles it through the base class.
	public sealed class MinigameTrigger : Interactable
	{
		public override void Interact() => Services.Minigame?.Start(Id);
	}
}