using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Core
{
	// Composition root — the ONE MonoBehaviour that knows every service.
	// It owns the Container, builds the service graph in Awake, and tears it
	// down in OnDestroy. New services are added here; no other file constructs them.
	// Static Instance is auto-cleaned on scene unload; OnDestroy also nulls it explicitly.
	[Unity.Scripting.LifecycleManagement.AutoStaticsCleanup]
	public sealed partial class Bootstrapper : MonoBehaviour
	{
		public static Bootstrapper Instance { get; private set; }

		[SerializeField] private GameSettings _settings;

		private Container _container;

		public GameSession Session { get; private set; }
		public InputSettings InputSettings { get; private set; }
		public PauseState Pause { get; private set; }
		public ProgressStore Progress { get; private set; }

		private void Awake()
		{
			Instance = this;
			Assert.IsNotNull(_settings, "Bootstrapper requires a GameSettings asset assigned");

			_container = new Container();
			// Provide order == construction order; add Gui, Save, etc. here (deps first).
			_container.Provide(_settings);
			_container.Provide(new InputSettings(_settings));
			_container.Provide(new GameSession(_settings));
			_container.Provide(new PauseState());
			_container.Provide(g => new ProgressStore(g.Grab<GameSession>()));

			// Pull order == wiring order. Add Gui, Save, etc. here as they appear.
			Session = _container.Grab<GameSession>();
			InputSettings = _container.Grab<InputSettings>();
			Pause = _container.Grab<PauseState>();
			Progress = _container.Grab<ProgressStore>();
		}

		private void OnDestroy()
		{
			if (Instance == this) Instance = null;
			_container?.Dispose();
		}
	}
}