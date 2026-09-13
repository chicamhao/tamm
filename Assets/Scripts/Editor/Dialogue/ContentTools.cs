using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	/// <summary>
	/// Menu-item-based Import / Export tools for the YAML content pipeline.
	/// </summary>
	public static class ContentTools
	{
		private const string YamlFolder = "Assets/Settings/Game/YAML";
		private const string FingerprintKey = "UNI_TAM_YamlImportFingerprint";

		// ---------------------------------------------------------------
		// Import
		// ---------------------------------------------------------------

		[MenuItem("Assets/Import Content from YAML")]
		private static void ImportFromYAML()
		{
			string summary = RunImport();
			Debug.Log($"[ContentTools] Import complete. {summary}");
			EditorUtility.DisplayDialog("Import Content from YAML — Complete", summary, "OK");
		}

		[MenuItem("Assets/Import Content from YAML", true)]
		private static bool ValidateImportFromYAML() => YamlFolderExists();

		// ---------------------------------------------------------------
		// Export
		// ---------------------------------------------------------------

		[MenuItem("Assets/Export Content to YAML")]
		private static void ExportToYAML()
		{
			var exporter = new ContentYAMLExporter();
			string summary = exporter.ExportAll(YamlFolder);
			Debug.Log($"[ContentTools] Export complete. {summary}");
			EditorUtility.DisplayDialog("Export Content to YAML — Complete", summary, "OK");
		}

		[MenuItem("Assets/Export Content to YAML", true)]
		private static bool ValidateExportToYAML() => YamlFolderExists();

		// ---------------------------------------------------------------
		// Import engine (public for programmatic use)
		// ---------------------------------------------------------------

		public static string RunImport()
		{
			var parser = new ContentYAMLParser();
			var factory = new ContentAssetFactory();

			string folderFullPath = Path.GetFullPath(YamlFolder);
			if (!Directory.Exists(folderFullPath))
			{
				Debug.LogError($"[ContentTools] YAML folder not found: {YamlFolder}");
				return $"ERROR: YAML folder not found:\n{YamlFolder}";
			}

			string[] yamlFiles = Directory.GetFiles(folderFullPath, "*.yaml");
			if (yamlFiles.Length == 0)
			{
				Debug.LogWarning("[ContentTools] No .yaml files found in Assets/Settings/Game/YAML/");
				return $"No .yaml files found in Assets/Settings/Game/YAML/";
			}

			foreach (string fullPath in yamlFiles)
			{
				string relativePath = GetRelativePath(fullPath);
				string fileName = Path.GetFileNameWithoutExtension(fullPath);
				string yamlText = File.ReadAllText(fullPath);

				try
				{
					switch (fileName.ToLowerInvariant())
					{
						case "cards":
							var cards = parser.ParseCards(yamlText);
							foreach (var card in cards)
								factory.ImportCard(card);
							break;

						case "dialogues":
							var dialogues = parser.ParseDialogues(yamlText);
							foreach (var dialogue in dialogues)
								factory.ImportDialogue(dialogue);
							break;

						case "chapters":
							var chapters = parser.ParseChapters(yamlText);
							foreach (var chapter in chapters)
								factory.ImportChapter(chapter);
							break;

						case "expressions":
							var expressions = parser.ParseExpressions(yamlText);
							foreach (var expression in expressions)
								factory.ImportExpression(expression);
							break;

						default:
							Debug.LogWarning($"[ContentTools] Unknown YAML file '{fileName}.yaml' — skipping.");
							break;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError($"[ContentTools] Failed to process {relativePath}: {ex.GetType().Name}: {ex.Message}");
				}
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			StoreFingerprint();

			return $"Created: {factory.Created}\nUpdated: {factory.Updated}\nSkipped: {factory.Skipped}";
		}

		// ---------------------------------------------------------------
		// Staleness detection (InitializeOnLoad)
		// ---------------------------------------------------------------

		[InitializeOnLoad]
		internal static class StalenessChecker
		{
			static StalenessChecker()
			{
				EditorApplication.delayCall += Check;
			}

			private static void Check()
			{
				string saved = EditorPrefs.GetString(FingerprintKey, string.Empty);
				string current = ComputeFingerprint();

				if (string.IsNullOrEmpty(saved))
				{
					EditorPrefs.SetString(FingerprintKey, current);
					return;
				}

				if (saved != current)
				{
					Debug.LogWarning(
						"[ContentTools] YAML content files have been modified since the last import. " +
						"Use Assets → Import Content from YAML to update.");
				}
			}
		}

		// ---------------------------------------------------------------
		// Helpers
		// ---------------------------------------------------------------

		private static bool YamlFolderExists()
		{
			return Directory.Exists(Path.GetFullPath(YamlFolder));
		}

		private static string ComputeFingerprint()
		{
			string folderFullPath = Path.GetFullPath(YamlFolder);
			if (!Directory.Exists(folderFullPath))
				return string.Empty;

			string[] files = Directory.GetFiles(folderFullPath, "*.yaml");
			if (files.Length == 0)
				return string.Empty;

			var sb = new System.Text.StringBuilder();
			foreach (string file in files)
			{
				string fileName = Path.GetFileName(file);
				DateTime lastWrite = File.GetLastWriteTimeUtc(file);
				sb.Append(fileName).Append(':').Append(lastWrite.Ticks).Append(';');
			}
			return sb.ToString();
		}

		private static void StoreFingerprint()
		{
			EditorPrefs.SetString(FingerprintKey, ComputeFingerprint());
		}

		private static string GetRelativePath(string fullPath)
		{
			string full = Path.GetFullPath(fullPath);
			string project = Path.GetFullPath(Application.dataPath + "/../");
			if (full.StartsWith(project, StringComparison.OrdinalIgnoreCase))
				return full.Substring(project.Length).Replace("\\", "/");
			return fullPath;
		}
	}
}