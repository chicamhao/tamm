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
		/// Import a card definition into the CardSettings dictionary (key: card id).
		/// Creates Assets/Settings/Game/CardSettings.asset if needed.
		/// </summary>
		public void ImportCard(CardEntry data)
		{
			string path = "Assets/Settings/Game/CardSettings.asset";

			CardSettings cards;

			if (File.Exists(Path.GetFullPath(path)))
			{
				cards = AssetDatabase.LoadAssetAtPath<CardSettings>(path);
			}
			else
			{
				cards = ScriptableObject.CreateInstance<CardSettings>();
				AssetDatabase.CreateAsset(cards, path);
				Created++;
				Updated--;
			}

			if (cards == null)
			{
				Debug.LogError("[ContentAssetFactory] CardSettings is null after load/create — skipping.");
				Skipped++;
				return;
			}

			string key = data.CardId;

			bool isNew = !cards.Entries.ContainsKey(key);
			if (isNew)
				Created++;
			else
				Updated++;

			// Icon is a Texture2D reference — skipped from YAML (no path mapping)
			cards.Entries[key] = new Card
			{
				DisplayName = data.DisplayName,
				Description = data.Description,
				Icon = null,
				TargetActorIDs = data.TargetActorIds.Select(id => new Identifier(id)).ToList()
			};

			EditorUtility.SetDirty(cards);
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
					ExpressionId = l.ExpressionId
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
		/// Import an expression definition into the ExpressionSettings dictionary (key: expression id).
		/// Creates Assets/Settings/Game/ExpressionSettings.asset if needed.
		/// </summary>
		public void ImportExpression(ExpressionEntry data)
		{
			string path = "Assets/Settings/Game/ExpressionSettings.asset";

			ExpressionSettings expressions;

			if (File.Exists(Path.GetFullPath(path)))
			{
				expressions = AssetDatabase.LoadAssetAtPath<ExpressionSettings>(path);
			}
			else
			{
				expressions = ScriptableObject.CreateInstance<ExpressionSettings>();
				AssetDatabase.CreateAsset(expressions, path);
				Created++;
				Updated--;
			}

			if (expressions == null)
			{
				Debug.LogError("[ContentAssetFactory] ExpressionSettings is null after load/create — skipping.");
				Skipped++;
				return;
			}

			string key = data.Id;

			bool isNew = !expressions.Entries.ContainsKey(key);
			if (isNew)
				Created++;
			else
				Updated++;

			expressions.Entries[key] = new Expression
			{
				MorphTargets = data.MorphTargets.Select(mt => new MorphTargetValue
				{
					name = mt.Name,
					value = mt.Value,
					blendInTime = mt.BlendInTime
				}).ToList()
			};

			EditorUtility.SetDirty(expressions);
		}
	}
}