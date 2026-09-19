using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

// Temporary CLI entry: headless WebGL export via the project's Web build profile.
// Usage: Unity -batchmode -quit -projectPath <proj> -executeMethod BuildWebCLI.Build
public static class BuildWebCLI
{
	public static void Build()
	{
		BuildProfile profile = BuildProfile.GetBuildProfileAtPath("Assets/Settings/Build Profiles/Web.asset");
		if (profile == null)
		{
			Debug.LogError("BuildWebCLI: Web profile not found");
			return;
		}

		BuildPlayerWithProfileOptions options = new BuildPlayerWithProfileOptions();
		options.buildProfile = profile;
		options.locationPathName = "Builds/WebGL";

		Debug.Log("[BuildWebCLI] building Web profile -> Builds/WebGL");
		BuildPipeline.BuildPlayer(options);
		Debug.Log("[BuildWebCLI] done");
	}
}