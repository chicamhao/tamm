using Game.Content;
using System.Collections.Generic;
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

		[SerializeField] private GameSettings _settings; // hub: content refs + input default

		private Container _container;

		public CardInventory Cards { get; private set; }
		public InteractionService Interactions { get; private set; }
		public InputSettings InputSettings { get; private set; }
		public PauseState Pause { get; private set; }
		public ProgressStore Progress { get; private set; }

		private void Awake()
		{
			Instance = this;
			Assert.IsNotNull(_settings, "Bootstrapper requires a GameSettings asset assigned");
			Assert.IsNotNull(_settings.Cards, "GameSettings requires CardSettings assigned");
			Assert.IsNotNull(_settings.Dialogues, "GameSettings requires DialogueSettings assigned");

			_container = new Container();
			// Provide order == construction order; add Gui, Save, etc. here (deps first).
			_container.Provide(_settings);
			_container.Provide(new InputSettings(_settings));
			_container.Provide(new PauseState());
			_container.Provide(new CardInventory());
			_container.Provide(g => new InteractionService(
				g.Grab<CardInventory>(),
				new List<string>(_settings.Cards.Entries.Keys).ToArray(),
				InteractionService.DeriveActorIds(
					_settings.Dialogues.Entries.Keys,
					_settings.Chapters != null ? _settings.Chapters.Entries.Keys : null,
					_settings.Cards.Entries.Keys).ToArray()));
			_container.Provide(g => new ProgressStore(g.Grab<CardInventory>()));

			// Pull order == wiring order. Add Gui, Save, etc. here as they appear.
			Cards = _container.Grab<CardInventory>();
			Interactions = _container.Grab<InteractionService>();
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