using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
	/// <summary>ScriptableObject holding all dialogue entries keyed by "CardID_ActorID".</summary>
	[CreateAssetMenu(fileName = "DialogSettings", menuName = "ScriptableObjects/DialogSettings", order = 1)]
	public sealed class DialogueSettings : ScriptableObject
	{
		// Unity 6 native dictionary serialization — key is $"{CardId}_{ActorId}" or
		// $"{CardId}_{ActorId}_{ChapterId}" (chapter-specific override, falls back to the plain key)
		[SerializeField] public Dictionary<string, DialogueEntry> Entries = new();
	}

	/// <summary>Represents a dialogue triggered by a card for a specific NPC.
	/// CardID and ActorID are encoded in the dictionary key.</summary>
	[System.Serializable]
	public sealed class DialogueEntry
	{
		[Header("Content")]
		public List<DialogueLine> Lines;

		[Header("Reward")]
		[Tooltip("Card granted to the player when this dialogue ends (null = no reward)")]
		public CardDefinition RewardCard;
	}

	/// <summary>A single line of dialogue with display duration and optional facial expression.</summary>
	[System.Serializable]
	public struct DialogueLine
	{
		public string Line;
		public float DisplayDuration;
		public ExpressionDefinition Expression;
	}

	/// <summary>Defines a morph target weight and blend time for facial expressions.</summary>
	[System.Serializable]
	public struct MorphTargetValue
	{
		public string name;
		public float value;
		public float blendInTime;
	}
}