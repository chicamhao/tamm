using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
	/// <summary>ScriptableObject holding every minigame, keyed by id.</summary>
	[CreateAssetMenu(fileName = "MinigameSettings", menuName = "ScriptableObjects/MinigameSettings", order = 1)]
	public sealed class MinigameSettings : ScriptableObject
	{
		[SerializeField] public Dictionary<string, MinigameEntry> Entries = new();
	}

	/// <summary>One minigame: the scene it runs in plus the card granted on a win.</summary>
	[System.Serializable]
	public sealed class MinigameEntry
	{
		/// <summary>Additive-level scene asset, e.g. "Assets/Scenes/Rat.unity".</summary>
		public string SceneName = string.Empty;

		/// <summary>Card id granted to the player when the minigame is won (empty = no reward).</summary>
		public string RewardCardId = string.Empty;
	}
}