using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
	/// <summary>ScriptableObject defining a dialogue card the player can acquire and use on NPCs.</summary>
	[CreateAssetMenu(fileName = "NewCard", menuName = "Game/Card Data")]
	public sealed class CardDefinition : ScriptableObject
	{
		[Header("Card Info")]
		public string CardID;
		public string DisplayName;
		[TextArea(3, 5)]
		public string Description;
		public Texture2D Icon;
		public List<Identifier> TargetActorIDs; // empty = usable on all actors
	}
}