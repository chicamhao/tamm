using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// One-click Web release settings (Tools/Apply Web Release Settings).
///
/// IMPORTANT for this project: builds go through the "Web" Build Profile
/// (Assets/Settings/Build Profiles/Web.asset), which OVERRIDES the global
/// Player Settings at build time. Running this menu item alone is not enough —
/// the profile must carry the same values (compression, mesh stripping, wasm
/// code optimization). Use the ApplyWebProfileReleaseSettings() helper on the
/// profile asset as well, then rebuild with the profile (unity build --profile Web).
/// </summary>
public static class WebOptimizer
{
    [MenuItem("Tools/Apply Web Release Settings")]
    public static void Optimize()
    {
        var target = NamedBuildTarget.WebGL;
        PlayerSettings.SetIl2CppCodeGeneration(target, Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.SetManagedStrippingLevel(target, ManagedStrippingLevel.Medium);
        PlayerSettings.stripUnusedMeshComponents = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        // Explicitly Thrown Only — the game relies on try/catch at runtime.
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        PlayerSettings.WebGL.wasm2023 = true;
        UnityEditor.WebGL.UserBuildSettings.codeOptimization =
            UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;

        // Persist. Without this the settings are applied to the in-memory objects only:
        // every read-back below returns the new value, the run reports success, and nothing
        // reaches ProjectSettings/ProjectSettings.asset.
        AssetDatabase.SaveAssets();

        Debug.Log(
            "Web release settings applied and saved:\n"
            + $"  il2cppCodeGeneration = {PlayerSettings.GetIl2CppCodeGeneration(target)}\n"
            + $"  managedStrippingLevel = {PlayerSettings.GetManagedStrippingLevel(target)}\n"
            + $"  stripUnusedMeshComponents = {PlayerSettings.stripUnusedMeshComponents}\n"
            + $"  dataCaching = {PlayerSettings.WebGL.dataCaching}\n"
            + $"  compressionFormat = {PlayerSettings.WebGL.compressionFormat}\n"
            + $"  exceptionSupport = {PlayerSettings.WebGL.exceptionSupport}\n"
            + $"  debugSymbolMode = {PlayerSettings.WebGL.debugSymbolMode}\n"
            + $"  wasm2023 = {PlayerSettings.WebGL.wasm2023}\n"
            + $"  codeOptimization = {UnityEditor.WebGL.UserBuildSettings.codeOptimization}");
    }

    /// <summary>
    /// Mirror the release settings into the active WebGL Build Profile's PlayerSettings
    /// snapshot, so a build driven by the profile ships them even on machines/CI where
    /// the global Player Settings differ or UserBuildSettings (per-machine) is unset.
    /// Uses SerializedObject so the written values land in the profile asset and can be
    /// verified by reading the .asset file back.
    /// </summary>
    public static void ApplyWebProfileReleaseSettings()
    {
        var profile = UnityEditor.Build.Profile.BuildProfile.GetActiveBuildProfile();
        if (profile == null)
        {
            Debug.LogWarning("No active Build Profile; profile settings not touched.");
            return;
        }

        var playerField = profile.GetType().GetField(
            "m_PlayerSettings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var ps = playerField?.GetValue(profile) as PlayerSettings;
        if (ps == null)
        {
            Debug.LogWarning("Could not resolve the profile's PlayerSettings snapshot.");
            return;
        }

        // NOTE: on Unity 6000 the serialized enum is {Brotli=0, Gzip=1, Disabled=2}
        // (NOT the legacy {None, Gzip, Brotli} order) — always go through the enum, never a raw index.
        var so = new SerializedObject(ps);
        so.FindProperty("webGLCompressionFormat").enumValueIndex = (int)WebGLCompressionFormat.Brotli;
        so.FindProperty("StripUnusedMeshComponents").boolValue = true;
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        Debug.Log("Web profile settings applied and saved.");

        var profileSo = new SerializedObject(profile);
        var codeOpt = profileSo.FindProperty("m_PlatformBuildProfile.m_CodeOptimization");
        if (codeOpt != null)
        {
            codeOpt.intValue = (int)UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;
            codeOpt.serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("Web profile code optimization = DiskSizeLTO.");
        }
    }
}