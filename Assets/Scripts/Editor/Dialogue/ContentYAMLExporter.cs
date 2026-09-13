using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Game.Content;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Game.Editor
{
	/// <summary>
	/// Reads ScriptableObject assets and writes them back to YAML content files
	/// in Assets/Settings/Game/YAML/. The reverse of ContentAssetFactory + ContentYAMLParser.
	/// </summary>
	public sealed class ContentYAMLExporter
	{
		private readonly ISerializer _serializer;

		// ---------------------------------------------------------------
		// Counts returned after an export batch
		// ---------------------------------------------------------------

		public int Exported { get; private set; }
		public int Skipped { get; private set; }

		public ContentYAMLExporter()
		{
			_serializer = new SerializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.Build();
		}

		// ---------------------------------------------------------------
		// Cards
		// ---------------------------------------------------------------

		/// <summary>
		/// Export all CardDefinition assets from Assets/Settings/Game/Cards/ to cards.yaml.
		/// </summary>
		public void ExportCards(string outputPath)
		{
			string folder = "Assets/Settings/Game/Cards";
			var cardAssets = AssetDatabase.FindAssets("t:CardDefinition", new[] { folder })
				.Select(guid => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
				.Where(c => c != null)
				.ToList();

			var entries = new List<CardEntry>();
			foreach (var card in cardAssets)
			{
				entries.Add(new CardEntry
				{
					CardId = card.CardID,
					DisplayName = card.DisplayName,
					Description = card.Description,
					IconPath = string.Empty,
					TargetActorIds = card.TargetActorIDs?.Select(id => id.ID).ToList() ?? new()
				});
			}

			var file = new CardsFile { CardDefinitions = entries };
			string yaml = _serializer.Serialize(file);
			File.WriteAllText(outputPath, yaml);
			Exported += entries.Count;
		}

		// ---------------------------------------------------------------
		// Dialogues
		// ---------------------------------------------------------------

		/// <summary>
		/// Export dialogue entries from Assets/Settings/Game/DialogueSettings.asset to dialogues.yaml.
		/// Expression references are skipped (not representable in YAML).
		/// </summary>
		public void ExportDialogues(string outputPath)
		{
			string path = "Assets/Settings/Game/DialogueSettings.asset";
			var dialogue = AssetDatabase.LoadAssetAtPath<DialogueSettings>(path);

			if (dialogue == null || dialogue.Entries == null)
			{
				Skipped++;
				return;
			}

			var entries = new List<DialogueEntry>();
			foreach (var kv in dialogue.Entries)
			{
				// Key is "CardId_ActorId" or "CardId_ActorId_ChapterId" — actor and optional
				// chapter stay encoded in it, so the key is written back as-is.
				string cardId = kv.Key;
				var entry = kv.Value;

				var lines = new List<DialogueLineEntry>();
				if (entry.Lines != null)
				{
					foreach (var line in entry.Lines)
					{
						lines.Add(new DialogueLineEntry
						{
							Text = line.Line,
							Duration = line.DisplayDuration,
							ExpressionId = string.Empty // can't serialize ExpressionDefinition ref
						});
					}
				}

				entries.Add(new DialogueEntry
				{
					CardId = cardId,
					Lines = lines
				});
			}

			var file = new DialoguesFile { DialogueEntries = entries };
			string yaml = _serializer.Serialize(file);
			File.WriteAllText(outputPath, yaml);
			Exported += entries.Count;
		}

		// ---------------------------------------------------------------
		// Chapters
		// ---------------------------------------------------------------

		/// <summary>
		/// Export chapter entries from Assets/Settings/Game/ChapterSettings.asset to chapters.yaml.
		/// AnimationClip references are skipped (not representable in YAML).
		/// </summary>
		public void ExportChapters(string outputPath)
		{
			string path = "Assets/Settings/Game/ChapterSettings.asset";
			var chapters = AssetDatabase.LoadAssetAtPath<ChapterSettings>(path);

			if (chapters == null || chapters.Entries == null)
			{
				Skipped++;
				return;
			}

			var entries = new List<ChapterEntry>();
			foreach (var kv in chapters.Entries)
			{
				// Key format: "ActorID_Chapter" — split on last '_' since Chapter is an int
				string key = kv.Key;
				int sep = key.LastIndexOf('_');
				string actorId = key.Substring(0, sep);
				int chapter = int.Parse(key.Substring(sep + 1));
				var entry = kv.Value;

				entries.Add(new ChapterEntry
				{
					ActorId = actorId,
					Chapter = chapter,
					SpawnPointId = entry.SpawnPointID,
					IsVisible = entry.IsVisible
				});
			}

			var file = new ChaptersFile { ChapterEntries = entries };
			string yaml = _serializer.Serialize(file);
			File.WriteAllText(outputPath, yaml);
			Exported += entries.Count;
		}

		// ---------------------------------------------------------------
		// Expressions
		// ---------------------------------------------------------------

		/// <summary>
		/// Export all ExpressionDefinition assets from Assets/Settings/Game/Expressions/ to expressions.yaml.
		/// </summary>
		public void ExportExpressions(string outputPath)
		{
			string folder = "Assets/Settings/Game/Expressions";
			var expressionAssets = AssetDatabase.FindAssets("t:ExpressionDefinition", new[] { folder })
				.Select(guid => AssetDatabase.LoadAssetAtPath<ExpressionDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
				.Where(e => e != null)
				.ToList();

			var entries = new List<ExpressionEntry>();
			foreach (var expr in expressionAssets)
			{
				var morphTargets = expr.MorphTargets?
					.Select(mt => new MorphTargetEntry
					{
						Name = mt.name,
						Value = mt.value,
						BlendInTime = mt.blendInTime
					})
					.ToList() ?? new();

				entries.Add(new ExpressionEntry
				{
					Id = expr.name,
					MorphTargets = morphTargets
				});
			}

			var file = new ExpressionsFile { ExpressionDefinitions = entries };
			string yaml = _serializer.Serialize(file);
			File.WriteAllText(outputPath, yaml);
			Exported += entries.Count;
		}

		// ---------------------------------------------------------------
		// Batch export
		// ---------------------------------------------------------------

		/// <summary>
		/// Export all content types to the YAML folder.
		/// Returns a summary string: "Exported: X\nSkipped: Y".
		/// </summary>
		public string ExportAll(string yamlFolder)
		{
			string folderFullPath = Path.GetFullPath(yamlFolder);
			Directory.CreateDirectory(folderFullPath);

			ExportCards(Path.Combine(folderFullPath, "cards.yaml"));
			ExportDialogues(Path.Combine(folderFullPath, "dialogues.yaml"));
			ExportChapters(Path.Combine(folderFullPath, "chapters.yaml"));
			ExportExpressions(Path.Combine(folderFullPath, "expressions.yaml"));

			AssetDatabase.Refresh();

			string summary = $"Exported: {Exported}\nSkipped: {Skipped}";
			Debug.Log($"[ContentYAMLExporter] Export complete. {summary}");
			return summary;
		}
	}
}