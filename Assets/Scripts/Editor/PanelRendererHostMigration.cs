using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Phase 5 of the UI Toolkit migration (docs/ui-toolkit-migration.md): consolidate every
// UITK runtime host onto the same component pair. Unity 6.7 split the old UIDocument
// into a renderer (PanelRenderer) and a scripting anchor (UIDocument): a host that has
// only a UIDocument still works (an implicit renderer is used), but a host that has only
// a PanelRenderer has NO rootVisualElement and cannot be scripted. The editor auto-
// migrated Playground's UIDocument -> PanelRenderer on a save, breaking the Playground
// UI (smoke test regression). This script adds whichever component each host is missing
// so every host ends with BOTH (PanelRenderer + UIDocument) sharing panelSettings and
// visualTreeAsset — the canonical, scriptable arrangement. Idempotent — safe to re-run.
//
// Usage: Unity -batchmode -quit -projectPath <proj> -executeMethod PanelRendererHostMigration.Run
public static class PanelRendererHostMigration
{
	private static readonly string[] ScenePaths =
	{
		"Assets/Scenes/Playground.unity",
		"Assets/Scenes/Rat.unity"
	};

	private const string TouchPrefabPath = "Assets/Prefabs/Mobile/UI_TouchScreenInput.prefab";

	public static void Run()
	{
		foreach (string scenePath in ScenePaths)
		{
			Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
			if (scene == null)
			{
				Debug.LogError("PanelRendererHostMigration: could not open " + scenePath);
				continue;
			}

			GameObject host = GameObject.Find("RuntimeUI");
			if (host == null)
			{
				Debug.LogError("PanelRendererHostMigration: no 'RuntimeUI' GameObject in " + scenePath);
				continue;
			}

			EnsurePair(host, scenePath);
			EditorSceneManager.SaveScene(scene);
			Debug.Log("PanelRendererHostMigration: saved " + scenePath);
		}

		EnsurePrefabPair();
		Debug.Log("PanelRendererHostMigration: done");
	}

	private static void EnsurePair(GameObject host, string label)
	{
		PanelRenderer pr = host.GetComponent<PanelRenderer>();
		UIDocument doc = host.GetComponent<UIDocument>();

		if (pr != null && doc == null)
		{
			// Playground: editor migrated UIDocument -> PanelRenderer; restore the anchor.
			UIDocument added = host.AddComponent<UIDocument>();
			added.panelSettings = pr.panelSettings;
			added.visualTreeAsset = pr.visualTreeAsset;
			Debug.Log(label + ": added UIDocument beside PanelRenderer");
		}
		else if (doc != null && pr == null)
		{
			// Rat: has the anchor but no explicit renderer; add the PanelRenderer.
			PanelRenderer added = host.AddComponent<PanelRenderer>();
			added.panelSettings = doc.panelSettings;
			added.visualTreeAsset = doc.visualTreeAsset;
			Debug.Log(label + ": added PanelRenderer beside UIDocument");
		}
		else
		{
			Debug.Log(label + ": already has both (PanelRenderer=" + (pr != null) + " UIDocument=" + (doc != null) + ")");
		}
	}

	private static void EnsurePrefabPair()
	{
		GameObject root = PrefabUtility.LoadPrefabContents(TouchPrefabPath);
		if (root == null)
		{
			Debug.LogError("PanelRendererHostMigration: could not load " + TouchPrefabPath);
			return;
		}

		PanelRenderer pr = root.GetComponent<PanelRenderer>();
		UIDocument doc = root.GetComponent<UIDocument>();
		if (pr != null && doc == null)
		{
			UIDocument added = root.AddComponent<UIDocument>();
			added.panelSettings = pr.panelSettings;
			added.visualTreeAsset = pr.visualTreeAsset;
		}
		else if (doc != null && pr == null)
		{
			PanelRenderer added = root.AddComponent<PanelRenderer>();
			added.panelSettings = doc.panelSettings;
			added.visualTreeAsset = doc.visualTreeAsset;
		}

		PrefabUtility.SaveAsPrefabAsset(root, TouchPrefabPath);
		PrefabUtility.UnloadPrefabContents(root);
		Debug.Log("PanelRendererHostMigration: prefab " + TouchPrefabPath + " updated");
	}
}
