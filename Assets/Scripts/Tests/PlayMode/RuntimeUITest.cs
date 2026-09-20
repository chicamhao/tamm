using Game.Core;
using Game.UI;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.UI.Tests
{
	// UITK runtime smoke test: boots the real entry path and asserts the full
	// runtime-UI chain that replaced uGUI —
	//   Bootstrapper (the persistent core scene) provides every service,
	//   the level (Playground) presents the RuntimeUI host + shared document tree,
	//   every screen subtree the leaves bind to resolves by name,
	//   and a leaf (ToastOverlay) reacts to a real service event by adding a view.
	public sealed class RuntimeUITest
	{
		[UnityTest]
		public IEnumerator UitkPanel_BootsWithTheCoreScene()
		{
			// The PlayMode runner boots a blank scene — services only exist after the
			// core loads. Levels load additively over it (Bootstrapper.LoadLevel).
			SceneManager.LoadScene("Assets/Scenes/Bootstrapper.unity", LoadSceneMode.Single);
			yield return null;
			yield return null;

			// The world scene trips QuickOutline's smooth-normal pass in batch players
			// (mesh read/write is disabled in import settings) and carries two legacy
			// missing-script warnings — pre-existing scene noise, out of this test's
			// remit, so it is suppressed only for the world's load window.
			LogAssert.ignoreFailingMessages = true;
			SceneManager.LoadScene("Assets/Scenes/Playground.unity", LoadSceneMode.Additive);
			yield return null;
			yield return null;
			yield return null;
			LogAssert.ignoreFailingMessages = false;

			// 1. Composition root: without this every leaf NREs (services are null).
			Assert.IsNotNull(Bootstrapper.Instance, "Bootstrapper.Instance not set after booting the core scene");
			Assert.IsNotNull(Game.Core.Services.Cards, "Bootstrapper did not provide services (Awake threw)");

			// 2. The UITK host + its document must be present and resolved.
			Game.UI.RuntimeUI ui = Object.FindFirstObjectByType<Game.UI.RuntimeUI>();
			Assert.IsNotNull(ui, "No RuntimeUI host in Playground");
			Assert.IsNotNull(ui.Root, "RuntimeUI host exposes no root element");

			// Phase 5 gate: every host is consolidated onto the PanelRenderer (+ UIDocument)
			// pair — the scriptable-anchor UIDocument holds the tree, the PanelRenderer is
			// the renderer. A host that lost its anchor (UIDocument) can't be scripted.
			Assert.IsNotNull(ui.gameObject.GetComponent<UnityEngine.UIElements.PanelRenderer>(),
				"RuntimeUI host must carry a PanelRenderer (everything-on-PanelRenderer)");
			Assert.IsNotNull(ui.gameObject.GetComponent<UnityEngine.UIElements.UIDocument>(),
				"RuntimeUI host must carry a UIDocument scripting anchor");

			// 3. Every screen subtree the leaves bind to must exist by name.
			AssertScreen(ui, "SettingsScreen");
			AssertScreen(ui, "CardScreen");
			AssertScreen(ui, "DialogueScreen");
			AssertScreen(ui, "ToastsHost");
			AssertScreen(ui, "TutorialBox");
			AssertScreen(ui, "CardList");
			Assert.IsNotNull(ui.Q<UnityEngine.UIElements.Slider>("SensitivitySlider"), "SensitivitySlider missing");
			Assert.IsNotNull(ui.Q<UnityEngine.UIElements.Button>("SaveButton"), "SaveButton missing");

			// 4. World tags: every world-tagged Interactable's WorldLabel appends a
			//    screen-space label under the shared WorldLabels layer (Phase 4 gate
			//    — Playground currently carries six tagged Interactables).
			Assert.That(ui.Q("WorldLabels").childCount, Is.EqualTo(6),
				"expected one screen-space label per world-tagged Interactable");

			// 5. A leaf reacting to a live service event produces a visible view
			//    (ToastOverlay.OnGranted -> Label added under ToastsHost).
			Game.Core.Services.Cards.Granted.OnNext("smoke_card");
			yield return null;
			Assert.That(ui.Q("ToastsHost").childCount, Is.GreaterThan(0), "ToastOverlay did not render the granted card");
		}

		private static void AssertScreen(Game.UI.RuntimeUI ui, string name)
		{
			Assert.IsNotNull(ui.Q(name), "runtime panel missing element '" + name + "'");
		}
	}
}