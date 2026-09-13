using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
	/// <summary>ScriptableObject containing a chapter lookup keyed by "ActorID_Chapter".</summary>
	[CreateAssetMenu(fileName = "ChapterSettings", menuName = "ScriptableObjects/ChapterSettings", order = 1)]
	public sealed class ChapterSettings : ScriptableObject
	{
		// Unity 6 native dictionary serialization — key is $"{ActorID}_{Chapter}"
		[SerializeField] public Dictionary<string, ChapterEntry> Entries = new();
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