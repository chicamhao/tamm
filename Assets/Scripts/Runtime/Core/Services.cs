namespace Game.Core
{
	// Typed access for scene MonoBehaviours (buttons, HUD) that must reach a service.
	// Only leaf rendering may use this — service-to-service wiring lives in Bootstrapper.Awake.
	// ponytail: static locator = hidden coupling; fine at this size, inject explicitly
	// if the service graph outgrows the scene leaves.
	public static class Services
	{
		public static CardInventory Cards => Bootstrapper.Instance?.Cards;
		public static InteractionService Interactions => Bootstrapper.Instance?.Interactions;
		public static DialogueService Dialogue => Bootstrapper.Instance?.Dialogue;
		public static ChapterState Chapter => Bootstrapper.Instance?.Chapter;
		public static InputSettings InputSettings => Bootstrapper.Instance?.InputSettings;
		public static PauseState Pause => Bootstrapper.Instance?.Pause;
		public static ProgressStore Progress => Bootstrapper.Instance?.Progress;
	}
}