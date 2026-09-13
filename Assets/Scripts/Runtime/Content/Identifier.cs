using System;
using UnityEngine;

namespace Game.Content
{
	/// <summary>Value object pairing the unique key and display name for an actor.</summary>
	[Serializable]
	public sealed class Identifier
	{
		[SerializeField] private string _id;
		[SerializeField] private string _displayName;

		/// <summary>Unique identifier matching DialogueSettings entries.</summary>
		public string ID => _id;
		/// <summary>Display name shown in UI prompts and toasts.</summary>
		public string DisplayName => _displayName;
	}
}