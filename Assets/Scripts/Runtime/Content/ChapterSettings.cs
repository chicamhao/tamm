using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
	/// <summary>ScriptableObject containing a chapter lookup keyed by "ActorID_Chapter" plus
	/// chapter-advance gates (progress conditions that unlock the next chapter).</summary>
	[CreateAssetMenu(fileName = "ChapterSettings", menuName = "ScriptableObjects/ChapterSettings", order = 1)]
	public sealed class ChapterSettings : ScriptableObject
	{
		// Unity 6 native dictionary serialization — key is $"{ActorID}_{Chapter}"
		[SerializeField] public Dictionary<string, ChapterEntry> Entries = new();

		// Gates: "when all conditions hold, advance to this chapter" — imported from chapters.yaml
		public List<ChapterGate> Gates = new();
	}

	/// <summary>Advances to Chapter once all its conditions are met.</summary>
	[System.Serializable]
	public sealed class ChapterGate
	{
		public int Chapter;
		public List<ProgressCondition> Conditions = new();
	}

	/// <summary>One player-progress condition: owns a card, or has had a card-actor conversation.</summary>
	[System.Serializable]
	public sealed class ProgressCondition
	{
		public ProgressConditionType Type;
		public string CardId = string.Empty;
		public string ActorId = string.Empty;
	}

	public enum ProgressConditionType
	{
		OwnsCard,
		HadConversation,
	}

	/// <summary>Defines an actor's state at a given chapter — spawn point, animation, and visibility.
	/// ActorID and Chapter are encoded in the dictionary key.</summary>
	[System.Serializable]
	public struct ChapterEntry
	{
		public string SpawnPointID;
		public AnimationClip Anim;
		public bool IsVisible;
	}
}