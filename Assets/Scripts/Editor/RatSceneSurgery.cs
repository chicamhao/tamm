using Game.Minigames.Rat;
using Game.Minigames.Rat.UI;
using Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Phase 3 of the UI Toolkit migration (docs/ui-toolkit-migration.md): rebuild the
// Rat level scene around the UITK runtime host, mirroring the Phase 1 Playground
// surgery. Idempotent — safe to re-run.
//
// Usage: Unity -batchmode -quit -projectPath <proj> -executeMethod RatSceneSurgery.Run
//
// NOTE: written against this repo's Dart-flavored runtime surface (Unity 6.7.0b1):
// GameObject.Find / Object.DestroyImmediate / SerializedObject.FindProperty are the
// supported entry points — the classic name-list + SetProperty APIs are absent.
public static class RatSceneSurgery
{
	private static readonly string[] UiLeafNames =
	{
		"ActionPromptPanel",
		"resultText",
		"roundText",
		"InstructionText",
		"ProgressText",
		"heartsText",
		"Canvas",
		"EventSystem"
	};

	public static void Run()
	{
		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			Return("user aborted scene save");

		Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Rat.unity");
		if (scene == null)
			Return("failed to open Assets/Scenes/Rat.unity");

		// 1. Strip the uGUI subtree + legacy leaf GOs from the scene.
		for (int i = 0; i < UiLeafNames.Length; i++)
			DestroyIfPresent(UiLeafNames[i]);

		// 2. Rebuild the RatManager component so its serialized fields match the
		//    rewired script (old scene carried stale TMP text refs: RoundText,
		//    InstructionText, ProgressText, HeartsText, ResultText).
		RebuildRatManager();

		// 3. Add the UITK host + RatHud leaf, mirroring the Playground surgery.
		RuntimeUI host = AddRuntimeUIHost();
		AddRatHud(host);

		EditorSceneManager.SaveScene(scene);
		Debug.Log("[RatSceneSurgery] Rat.unity rebuilt: uGUI stripped, UITK host + RatHud wired");
	}

	private static void Return(string why)
	{
		Debug.LogError("[RatSceneSurgery] " + why);
		EditorApplication.Exit(1);
	}

	private static void DestroyIfPresent(string name)
	{
		GameObject go = GameObject.Find(name);
		if (go == null)
		{
			Debug.Log("[RatSceneSurgery] " + name + ": already gone, skipping");
			return;
		}
		Object.DestroyImmediate(go);
		Debug.Log("[RatSceneSurgery] destroyed " + name);
	}

	/// <summary>
	/// Drops the scene's stale RatManager component (it still serialized the five
	/// TMP text refs) and re-adds the current script, re-wiring its public fields.
	/// </summary>
	private static void RebuildRatManager()
	{
		GameObject go = GameObject.Find("RatManager");
		if (go == null)
		{
			Debug.LogError("[RatSceneSurgery] RatManager GO not found");
			return;
		}

		RatManager old = go.GetComponent<RatManager>();
		if (old != null)
			Object.DestroyImmediate(old);

		RatManager fresh = go.AddComponent<RatManager>();
		GameObject ball = GameObject.Find("Ball");
		if (ball != null)
			fresh.Ball = ball.GetComponent<BallController>();
		if (fresh.Ball == null)
			Debug.LogWarning("[RatSceneSurgery] BallController not found; RatManager will look it up at Awake");
	}

	/// <summary>Creates the RuntimeUI root: Transform + UIDocument + Game.UI.RuntimeUI.</summary>
	private static RuntimeUI AddRuntimeUIHost()
	{
		VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Runtime/UI/RatUI.uxml");
		ScriptableObject panel = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Scripts/Runtime/UI/RuntimeUIPanelSettings.asset");
		if (tree == null || panel == null)
			Return("RatUI.uxml or RuntimeUIPanelSettings.asset missing");

		GameObject go = new GameObject("RuntimeUI");
		go.AddComponent<UIDocument>();
		RuntimeUI runtimeUI = go.AddComponent<RuntimeUI>();

		// Point the host's document at the Rat tree + shared panel settings, mirroring
		// the Playground host's serialized field set field-for-field.
		UIDocument doc = go.GetComponent<UIDocument>();
		SetObjectReference(doc, "sourceAsset", tree);
		SetObjectReference(doc, "m_PanelSettings", panel);
		SetInt(doc, "m_Position", 0);
		SetEnum(doc, "m_WorldSpaceSizeMode", 1); // mirror Playground host's serialized value
		SetFloat(doc, "m_WorldSpaceWidth", 1920f);
		SetFloat(doc, "m_WorldSpaceHeight", 1080f);
		SetBool(doc, "m_GenerateAccessibilityHierarchy", true);

		return runtimeUI;
	}

	/// <summary>Creates the RatHud leaf bound to the Rat level's own RuntimeUI.</summary>
	private static void AddRatHud(RuntimeUI host)
	{
		GameObject go = new GameObject("RatHud");
		RatHud hud = go.AddComponent<RatHud>();

		// Rat is an additive overlay over the core Playground, so two RuntimeUI
		// hosts can be alive at once — the leaf must bind to THIS scene's host.
		SerializedObject so = new SerializedObject(hud);
		SerializedProperty p = so.FindProperty("_runtimeUI");
		if (p == null)
			Return("RatHud._runtimeUI property not found");
		p.objectReferenceValue = host;
		so.ApplyModifiedProperties();
	}

	// =====================================================
	// SerializedObject value helpers (Dart-flavored surface:
	// no SetProperty — write through SerializedProperty props)
	// =====================================================

	private static void SetObjectReference(Component target, string property, Object value)
	{
		SerializedObject so = new SerializedObject(target);
		SerializedProperty p = so.FindProperty(property);
		if (p == null)
		{
			Debug.LogWarning("[RatSceneSurgery] property '" + property + "' not found on " + target.GetType().Name);
			return;
		}
		p.objectReferenceValue = value;
		so.ApplyModifiedProperties();
	}

	private static void SetInt(Component target, string property, int value)
	{
		SerializedObject so = new SerializedObject(target);
		SerializedProperty p = so.FindProperty(property);
		if (p == null)
		{
			Debug.LogWarning("[RatSceneSurgery] property '" + property + "' not found on " + target.GetType().Name);
			return;
		}
		p.intValue = value;
		so.ApplyModifiedProperties();
	}

	private static void SetEnum(Component target, string property, int value)
	{
		SerializedObject so = new SerializedObject(target);
		SerializedProperty p = so.FindProperty(property);
		if (p == null)
		{
			Debug.LogWarning("[RatSceneSurgery] property '" + property + "' not found on " + target.GetType().Name);
			return;
		}
		p.enumValueIndex = value;
		so.ApplyModifiedProperties();
	}

	private static void SetFloat(Component target, string property, float value)
	{
		SerializedObject so = new SerializedObject(target);
		SerializedProperty p = so.FindProperty(property);
		if (p == null)
		{
			Debug.LogWarning("[RatSceneSurgery] property '" + property + "' not found on " + target.GetType().Name);
			return;
		}
		p.floatValue = value;
		so.ApplyModifiedProperties();
	}

	private static void SetBool(Component target, string property, bool value)
	{
		SerializedObject so = new SerializedObject(target);
		SerializedProperty p = so.FindProperty(property);
		if (p == null)
		{
			Debug.LogWarning("[RatSceneSurgery] property '" + property + "' not found on " + target.GetType().Name);
			return;
		}
		p.boolValue = value;
		so.ApplyModifiedProperties();
	}
}