using UnityEngine;

namespace Game.Core
{
	// The minigame contract: every minigame scene must root exactly one
	// MonoBehaviour implementing IMinigame. The core finds it after the overlay
	// loads (MinigameService.Pump), hands it the shared input channel, and the
	// minigame drives everything after that — play, outcome, exit. Outcome is
	// reported back through Services.Minigame.Complete, same as before.
	// The interface (not a base class) keeps the minigame assembly free of core
	// implementation details: it only depends on the two contracts and the static
	// Services locator, so a minigame repo can be updated in place without the
	// core knowing its internal class names.
	public interface IMinigame
	{
		/// <summary>Called once the minigame scene is loaded and owns the screen.</summary>
		void OnStart(IMinigameInput input);
	}
}