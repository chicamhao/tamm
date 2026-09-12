using System;
using UnityEngine;

namespace Game.Core
{
	// Data-driven game settings, edited in the Inspector on a .asset.
	// Create: Assets -> Create -> Game/Settings. Assign the asset on Bootstrapper.
	// Scale path: per-level = separate .asset per level; new settings = new field here.
	[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
	public sealed class GameSettings : ScriptableObject
	{
		[Header("Collectibles")] public int ObjectCount = 3;
		[Header("Timer")] public float TimeLimitSeconds = 60.0f;
		[Header("Messages")] public String WinMessage = "You win!";
		[Header("Messages")] public String LoseMessage = "Time's up!";
		[Header("Input")] public float MouseSensitivity = 1.0f;
	}
}