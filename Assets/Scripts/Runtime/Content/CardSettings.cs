using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
	/// <summary>All card data in one asset, keyed by card id — imported from cards.yaml.</summary>
	[CreateAssetMenu(fileName = "CardSettings", menuName = "Game/Card Settings")]
	public sealed class CardSettings : ScriptableObject
	{
		// Unity 6 native dictionary serialization — key is the card id ("card_cam", ...)
		[SerializeField] public Dictionary<string, Card> Entries = new();
	}

	/// <summary>A dialogue card the player can acquire and use on NPCs.</summary>
	[Serializable]
	public sealed class Card
	{
		public string DisplayName = string.Empty;
		[TextArea(3, 5)]
		public string Description = string.Empty;
		public Texture2D Icon;
		public List<Identifier> TargetActorIDs = new(); // empty = usable on all actors
	}
}