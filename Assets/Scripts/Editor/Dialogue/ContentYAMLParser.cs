using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Game.Editor
{
	// ---------------------------------------------------------------
	// Typed DTOs matching YAML schema (one file per content type)
	// ---------------------------------------------------------------

	/// <summary>Root shape of cards.yaml.</summary>
	public sealed class CardsFile
	{
		public List<CardEntry> CardDefinitions { get; set; } = new();
	}

	/// <summary>Root shape of dialogues.yaml.</summary>
	public sealed class DialoguesFile
	{
		public List<DialogueEntry> DialogueEntries { get; set; } = new();
	}

	/// <summary>Root shape of chapters.yaml.</summary>
	public sealed class ChaptersFile
	{
		public List<ChapterEntry> ChapterEntries { get; set; } = new();
		public List<ChapterGateEntry> ChapterAdvances { get; set; } = new();
	}

	/// <summary>Raw chapter-advance gate from YAML.</summary>
	public sealed class ChapterGateEntry
	{
		public int Chapter { get; set; } = 1;
		public List<ProgressConditionEntry> Conditions { get; set; } = new();
	}

	/// <summary>Raw progress condition from YAML (type: "owns_card" | "had_conversation").</summary>
	public sealed class ProgressConditionEntry
	{
		public string Type { get; set; } = string.Empty;
		public string CardId { get; set; } = string.Empty;
		public string ActorId { get; set; } = string.Empty;
	}

	/// <summary>Root shape of expressions.yaml.</summary>
	public sealed class ExpressionsFile
	{
		public List<ExpressionEntry> ExpressionDefinitions { get; set; } = new();
	}

	/// <summary>Root shape of minigames.yaml.</summary>
	public sealed class MinigameDefinitionsFile
	{
		public List<MinigameEntry> MinigameDefinitions { get; set; } = new();
	}

	/// <summary>Raw minigame definition from YAML.</summary>
	public sealed class MinigameEntry
	{
		public string MinigameId { get; set; } = string.Empty;
		public string SceneName { get; set; } = string.Empty;
		public string RewardCardId { get; set; } = string.Empty;
	}

	/// <summary>Raw card definition from YAML.</summary>
	public sealed class CardEntry
	{
		public string CardId { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string IconPath { get; set; } = string.Empty;
		public List<string> TargetActorIds { get; set; } = new();
	}

	/// <summary>Raw dialogue entry from YAML. CardId embeds the actor ("cardId_actorId",
	/// optionally "cardId_actorId_chapterId") — the actor and optional chapter are encoded in it.</summary>
	public sealed class DialogueEntry
	{
		public string CardId { get; set; } = string.Empty;
		public List<DialogueLineEntry> Lines { get; set; } = new();
		public string MinigameId { get; set; } = string.Empty;
	}

	/// <summary>Single dialogue line from YAML.</summary>
	public sealed class DialogueLineEntry
	{
		public string Text { get; set; } = string.Empty;
		public float Duration { get; set; } = 2f;
		public string ExpressionId { get; set; } = string.Empty;
	}

	/// <summary>Raw chapter entry from YAML.</summary>
	public sealed class ChapterEntry
	{
		public string ActorId { get; set; } = string.Empty;
		public int Chapter { get; set; } = 1;
		public string SpawnPointId { get; set; } = string.Empty;
		public bool IsVisible { get; set; } = true;
	}

	/// <summary>Raw expression definition from YAML.</summary>
	public sealed class ExpressionEntry
	{
		public string Id { get; set; } = string.Empty;
		public List<MorphTargetEntry> MorphTargets { get; set; } = new();
	}

	/// <summary>Morph target value from YAML.</summary>
	public sealed class MorphTargetEntry
	{
		public string Name { get; set; } = string.Empty;
		public float Value { get; set; } = 0f;
		public float BlendInTime { get; set; } = 0.3f;
	}

	/// <summary>
	/// YAML deserializer for content files using YamlDotNet.
	/// Each typed ParseXxx method deserializes the corresponding YAML file.
	/// </summary>
	public sealed class ContentYAMLParser
	{
		private readonly IDeserializer _deserializer;

		/// <summary>Create a new parser with snake_case naming convention.</summary>
		public ContentYAMLParser()
		{
			_deserializer = new DeserializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.IgnoreUnmatchedProperties()
				.Build();
		}

		/// <summary>Parse cards.yaml content into typed card entries.</summary>
		public List<CardEntry> ParseCards(string yamlText)
		{
			if (string.IsNullOrWhiteSpace(yamlText))
				return new();

			var file = _deserializer.Deserialize<CardsFile>(yamlText);
			return file?.CardDefinitions ?? new();
		}

		/// <summary>Parse dialogues.yaml content into typed dialogue entries.</summary>
		public List<DialogueEntry> ParseDialogues(string yamlText)
		{
			if (string.IsNullOrWhiteSpace(yamlText))
				return new();

			var file = _deserializer.Deserialize<DialoguesFile>(yamlText);
			return file?.DialogueEntries ?? new();
		}

		/// <summary>Parse chapters.yaml content into typed chapter entries.</summary>
		public List<ChapterEntry> ParseChapters(string yamlText)
		{
			if (string.IsNullOrWhiteSpace(yamlText))
				return new();

			var file = _deserializer.Deserialize<ChaptersFile>(yamlText);
			return file?.ChapterEntries ?? new();
		}

		/// <summary>Parse chapters.yaml content into typed chapter-advance gates.</summary>
		public List<ChapterGateEntry> ParseChapterAdvances(string yamlText)
		{
			if (string.IsNullOrWhiteSpace(yamlText))
				return new();

			var file = _deserializer.Deserialize<ChaptersFile>(yamlText);
			return file?.ChapterAdvances ?? new();
		}

		/// <summary>Parse expressions.yaml content into typed expression entries.</summary>
		public List<ExpressionEntry> ParseExpressions(string yamlText)
		{
			if (string.IsNullOrWhiteSpace(yamlText))
				return new();

			var file = _deserializer.Deserialize<ExpressionsFile>(yamlText);
			return file?.ExpressionDefinitions ?? new();
		}

		/// <summary>Parse minigames.yaml content into typed minigame entries.</summary>
		public List<MinigameEntry> ParseMinigames(string yamlText)
		{
			if (string.IsNullOrWhiteSpace(yamlText))
				return new();

			var file = _deserializer.Deserialize<MinigameDefinitionsFile>(yamlText);
			return file?.MinigameDefinitions ?? new();
		}
	}
}