using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Game.Content;

namespace Game.Editor
{
	/// <summary>
	/// Takes parsed YAML content data and creates/updates ScriptableObject .asset files.
	/// Creates required folders on demand.
	/// </summary>
	public sealed class ContentAssetFactory
	{
		// ---------------------------------------------------------------
		// Counts returned after an import batch
		// ---------------------------------------------------------------

		public int Created { get; private set; }
		public int Updated { get; private set; }
		public int Skipped { get; private set; }

		// ---------------------------------------------------------------
		// Card definitions
		// ---------------------------------------------------------------

		/// <summary>
		/// Import a card definition. Looks for existing .asset by CardID in
		/// Assets/Settings/Game/Cards/. Creates a new CardDefinition if not found.
		/// </summary>
		public void ImportCard(CardEntry data)
		{
			string folder = "Assets/Settings/Game/Cards";
			EnsureFolder(folder);

			// Look for existing asset by CardID
			string guid = FindAssetGUID<CardDefinition>(folder, data.CardId);
			CardDefinition card;

			if (!string.IsNullOrEmpty(guid))
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
				Updated++;
			}
			else
			{
				card = ScriptableObject.CreateInstance<CardDefinition>();
				string fileName = SanitizeFileName(data.CardId) + ".asset";
				string path = Path.Combine(folder, fileName);
				AssetDatabase.CreateAsset(card, path);
				Created++;
			}

			// Update fields
			SerializedObject so = new SerializedObject(card);
			so.FindProperty("CardID").stringValue = data.CardId;
			so.FindProperty("DisplayName").stringValue = data.DisplayName;
			so.FindProperty("Description").stringValue = data.Description;
			// Icon is a Texture2D reference — we skip setting from YAML (no path mapping)
			so.FindProperty("Icon").objectReferenceValue = null;

			// TargetActorIDs — List<Identifier>
			SerializedProperty targetList = so.FindProperty("TargetActorIDs");
			targetList.ClearArray();
			targetList.arraySize = data.TargetActorIds.Count;
			for (int i = 0; i < data.TargetActorIds.Count; i++)
			{
				SerializedProperty elem = targetList.GetArrayElementAtIndex(i);
				SerializedProperty actorIdProp = elem.FindPropertyRelative("_id");
				SerializedProperty displayNameProp = elem.FindPropertyRelative("_displayName");
				if (actorIdProp != null)
					actorIdProp.stringValue = data.TargetActorIds[i];
				if (displayNameProp != null)
					displayNameProp.stringValue = string.Empty;
			}

			so.ApplyModifiedPropertiesWithoutUndo();
			EditorUtility.SetDirty(card);
		}

		// ---------------------------------------------------------------
		// Dialogue entries
		// ---------------------------------------------------------------

		/// <summary>
		/// Import a dialogue entry into the DialogueSettings dictionary (key: CardID_ActorID).
		/// Creates Assets/Settings/Game/DialogueSettings.asset if needed.
		/// </summary>
		public void ImportDialogue(DialogueEntry data)
		{
			string path = "Assets/Settings/Game/DialogueSettings.asset";

			DialogueSettings dialogue;

			if (File.Exists(Path.GetFullPath(path)))
			{
				dialogue = AssetDatabase.LoadAssetAtPath<DialogueSettings>(path);
			}
			else
			{
				dialogue = ScriptableObject.CreateInstance<DialogueSettings>();
				AssetDatabase.CreateAsset(dialogue, path);
				Created++;
				Updated--;
			}

			if (dialogue == null)
			{
				Debug.LogError("[ContentAssetFactory] DialogueSettings is null after load/create — skipping.");
				Skipped++;
				return;
			}

			string key = data.CardId;

			bool isNew = !dialogue.Entries.ContainsKey(key);
			if (isNew)
				Created++;
			else
				Updated++;

			var entry = new Game.Content.DialogueEntry
			{
				Lines = data.Lines.Select(l => new DialogueLine
				{
					Line = l.Text,
					DisplayDuration = l.Duration,
					Expression = null
				}).ToList()
			};

			dialogue.Entries[key] = entry;

			EditorUtility.SetDirty(dialogue);
		}

		// ---------------------------------------------------------------
		// Chapter entries
		// ---------------------------------------------------------------

		/// <summary>
		/// Import a chapter entry into the ChapterSettings dictionary (key: ActorID_Chapter).
		/// Loads or creates Assets/Settings/Game/ChapterSettings.asset.
		/// </summary>
		public void ImportChapter(ChapterEntry data)
		{
			if (data == null)
			{
				Debug.LogError("[ContentAssetFactory] ImportChapter received null data!");
				Skipped++;
				return;
			}

			string path = "Assets/Settings/Game/ChapterSettings.asset";
			ChapterSettings chapters;

			if (File.Exists(Path.GetFullPath(path)))
			{
				chapters = AssetDatabase.LoadAssetAtPath<ChapterSettings>(path);
			}
			else
			{
				chapters = ScriptableObject.CreateInstance<ChapterSettings>();
				AssetDatabase.CreateAsset(chapters, path);
				Created++;
				Updated--;
			}

			if (chapters == null)
			{
				Debug.LogError("[ContentAssetFactory] ChapterSettings is null after load/create — skipping.");
				Skipped++;
				return;
			}

			string key = $"{data.ActorId}_{data.Chapter}";

			bool isNew = !chapters.Entries.ContainsKey(key);
			if (isNew)
				Created++;
			else
				Updated++;

			chapters.Entries[key] = new Game.Content.ChapterEntry
			{
				SpawnPointID = data.SpawnPointId,
				IsVisible = data.IsVisible,
				Anim = null
			};

			EditorUtility.SetDirty(chapters);
		}

		// ---------------------------------------------------------------
		// Expression definitions
		// ---------------------------------------------------------------

		/// <summary>
		/// Import an expression definition. Creates an ExpressionDefinition .asset
		/// by id in Assets/Settings/Game/Expressions/. Updates existing if found.
		/// </summary>
		public void ImportExpression(ExpressionEntry data)
		{
			string folder = "Assets/Settings/Game/Expressions";
			EnsureFolder(folder);

			string fileName = SanitizeFileName(data.Id) + ".asset";
			string path = Path.Combine(folder, fileName);

			ExpressionDefinition expression;

			if (File.Exists(Path.GetFullPath(path)))
			{
				expression = AssetDatabase.LoadAssetAtPath<ExpressionDefinition>(path);
				Updated++;
			}
			else
			{
				expression = ScriptableObject.CreateInstance<ExpressionDefinition>();
				AssetDatabase.CreateAsset(expression, path);
				Created++;
			}

			SerializedObject so = new SerializedObject(expression);
			SerializedProperty morphTargetsProp = so.FindProperty("MorphTargets");
			morphTargetsProp.ClearArray();
			morphTargetsProp.arraySize = data.MorphTargets.Count;
			for (int i = 0; i < data.MorphTargets.Count; i++)
			{
				SerializedProperty elem = morphTargetsProp.GetArrayElementAtIndex(i);
				elem.FindPropertyRelative("name").stringValue = data.MorphTargets[i].Name;
				elem.FindPropertyRelative("value").floatValue = data.MorphTargets[i].Value;
				elem.FindPropertyRelative("blendInTime").floatValue = data.MorphTargets[i].BlendInTime;
			}

			so.ApplyModifiedPropertiesWithoutUndo();
			EditorUtility.SetDirty(expression);
		}

		// ---------------------------------------------------------------
		// Helpers
		// ---------------------------------------------------------------

		/// <summary>Ensure a folder exists under Assets, creating parent folders as needed.</summary>
		private void EnsureFolder(string folderPath)
		{
			if (AssetDatabase.IsValidFolder(folderPath))
				return;

			// Split and create recursively
			string parent = Path.GetDirectoryName(folderPath).Replace("\\", "/");
			string leaf = Path.GetFileName(folderPath);
			if (!AssetDatabase.IsValidFolder(parent))
				EnsureFolder(parent);
			AssetDatabase.CreateFolder(parent, leaf);
		}

		/// <summary>
		/// Search for an existing asset of type T under <paramref name="folder"/>
		/// whose CardID / property matches <paramref name="id"/>.
		/// Returns the asset GUID or null.
		/// </summary>
		private string FindAssetGUID<T>(string folder, string id) where T : ScriptableObject
		{
			string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				T asset = AssetDatabase.LoadAssetAtPath<T>(path);
				if (asset != null)
				{
					// Check if it's the one we're looking for by reading CardID via SerializedObject
					SerializedObject so = new SerializedObject(asset);
					SerializedProperty prop = so.FindProperty("CardID");
					if (prop != null && prop.stringValue == id)
						return guid;
				}
			}
			return null;
		}

		/// <summary>Replace invalid filename characters.</summary>
		private string SanitizeFileName(string name)
		{
			if (string.IsNullOrEmpty(name))
				return "Unnamed";
			char[] invalid = Path.GetInvalidFileNameChars();
			var sanitized = new System.Text.StringBuilder(name.Length);
			foreach (char c in name)
			{
				sanitized.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);
			}
			return sanitized.ToString();
		}
	}
}