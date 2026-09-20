using Game.Core;
using Game.Minigames.Rat;
using Game.UI;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.UI.Tests
{
	// Rat overlay smoke test: the migration's Phase 3 gate. Boots the real entry
	// path (core -> Playground level -> Rat minigame overlay) and asserts the
	// rebuilt Rat scene hosts its UI in the shared UITK panel:
	//   - the Rat scene owns a RuntimeUI of its own (two hosts are alive under the
	//     overlay; the Rat leaf must bind to the Rat document, not the core's),
	//   - every subtree RatHud queries by name resolves,
	//   - the rewired RatManager drives the HUD (round start = THROW prompt visible).
	// The uGUI leftovers are gone from the scene: no Canvas / EventSystem roots.
	public sealed class RatUITest
	{
		[UnityTest]
		public IEnumerator RatHud_BootsWithTheCoreSceneAsOverlay()
		{
			// Batch players trip two documented pre-existing scene noises during
			// every world-scene load (QuickOutline's smooth-normal uv4 write on the
			// RobotKile mesh hits import-settings read-only, + legacy missing-script
			// warnings). The additive Rat load delays that activation past any fixed
			// frame window, so this overlay test suppresses failing logs for all its
			// frames and relies on the structural asserts below as the real gate
			// (the same rationale as RuntimeUITest's load-window suppression).
			LogAssert.ignoreFailingMessages = true;

			SceneManager.LoadScene("Assets/Scenes/Bootstrapper.unity", LoadSceneMode.Single);
			yield return null;
			yield return null;

			SceneManager.LoadScene("Assets/Scenes/Playground.unity", LoadSceneMode.Additive);
			yield return null;
			yield return null;
			yield return null;
			SceneManager.LoadScene("Assets/Scenes/Rat.unity", LoadSceneMode.Additive);
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			LogAssert.ignoreFailingMessages = false;

			Assert.IsNotNull(Bootstrapper.Instance, "Bootstrapper.Instance not set after booting the core scene");
			Assert.IsNotNull(RatManager.Instance, "RatManager.Instance not set — Rat overlay did not boot");

			// 1. The rewired manager found its leaf.
			Assert.IsNotNull(RatManager.Instance.Hud, "RatManager has no RatHud");

			// 2. The Rat scene presents its own host; the leaf must bind there.
			Game.UI.RuntimeUI ratHost = FindSceneHost("Rat");
			Assert.IsNotNull(ratHost, "Rat scene hosts no RuntimeUI");
			Assert.IsNotNull(ratHost.Q("RatHud"), "Rat document is missing its RatHud subtree");

			// Phase 5 gate: the Rat host is on the canonical PanelRenderer + UIDocument pair.
			Assert.IsNotNull(ratHost.gameObject.GetComponent<UnityEngine.UIElements.PanelRenderer>(),
				"Rat RuntimeUI host must carry a PanelRenderer (everything-on-PanelRenderer)");
			Assert.IsNotNull(ratHost.gameObject.GetComponent<UnityEngine.UIElements.UIDocument>(),
				"Rat RuntimeUI host must carry a UIDocument scripting anchor");

			// 3. Every subtree RatHud queries in Start resolves by name.
			AssertScreen(ratHost, "PromptRoot");
			AssertScreen(ratHost, "ArrowIcon");
			AssertScreen(ratHost, "HandIcon");
			AssertScreen(ratHost, "Ripple");
			AssertScreen(ratHost, "SweepHint");
			AssertScreen(ratHost, "PromptLabel");
			AssertScreen(ratHost, "PromptCount");
			AssertScreen(ratHost, "ChipRow");
			AssertScreen(ratHost, "RoundText");
			AssertScreen(ratHost, "HeartsText");
			AssertScreen(ratHost, "InstructionText");
			AssertScreen(ratHost, "ProgressText");
			AssertScreen(ratHost, "ResultText");
			for (int i = 0; i < 3; i++)
			{
				AssertScreen(ratHost, "Chip" + i);
				AssertScreen(ratHost, "ChipCaption" + i);
			}

			// 4. Round boot drives the HUD to the THROW prompt (ShowThrow called
			//    from RatManager.Start -> StartGame -> BeginTurn).
			Game.Minigames.Rat.RatManager.GameState state = RatManager.Instance.State;
			string shown = ratHost.Q("PromptRoot").style.display.ToString();
			Assert.That(shown, Is.EqualTo(DisplayStyle.Flex.ToString()),
				"RatHud did not show the THROW prompt on boot (State=" + state.ToString() + ", display=" + shown + ")");
			Assert.That(state.ToString(), Is.EqualTo("WaitingForThrow"));

			// 5. The uGUI overlay is gone from the Rat scene roots.
			Assert.IsNull(FindSceneRoot("Rat", "Canvas"), "Rat scene still hosts a uGUI Canvas");
			Assert.IsNull(FindSceneRoot("Rat", "EventSystem"), "Rat scene still hosts an EventSystem");
		}

		private static Game.UI.RuntimeUI FindSceneHost(string sceneName)
		{
			GameObject root = FindSceneRoot(sceneName, "RuntimeUI");
			return root != null ? root.GetComponent<Game.UI.RuntimeUI>() : null;
		}

		private static GameObject FindSceneRoot(string sceneName, string rootName)
		{
			Scene scene = SceneManager.GetSceneByName(sceneName);
			if (scene == null || !scene.isLoaded)
				return null;

			GameObject[] roots = scene.GetRootGameObjects();
			for (int i = 0; i < roots.Length; i++)
			{
				if (roots[i].name.Equals(rootName))
					return roots[i];
			}
			return null;
		}

		private static void AssertScreen(Game.UI.RuntimeUI ui, string name)
		{
			Assert.IsNotNull(ui.Q(name), "Rat document missing element '" + name + "'");
		}
	}
}