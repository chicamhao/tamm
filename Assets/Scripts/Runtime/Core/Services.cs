namespace Game.Core
{
	// Typed access for scene MonoBehaviours (buttons, HUD) that must reach a service.
	// Only leaf rendering may use this — service-to-service wiring lives in Bootstrapper.Awake.
	// ponytail: static locator = hidden coupling; fine at this size, inject explicitly
	// if the service graph outgrows the scene leaves.
	public static class Services
	{
		public static GameSession Session => Bootstrapper.Instance?.Session;
		public static InputSettings InputSettings => Bootstrapper.Instance?.InputSettings;
		public static PauseState Pause => Bootstrapper.Instance?.Pause;
		public static ProgressStore Progress => Bootstrapper.Instance?.Progress;
	}
}