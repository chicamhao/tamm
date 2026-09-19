using Game.Content;
using UnityEngine;

namespace Game.Core
{
	// The game's data hub — minimum wiring: every content set + input default.
	// Create: Assets -> Create -> Game/Settings. Assign the asset on Bootstrapper.
	[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
	public sealed class GameSettings : ScriptableObject
	{
		[Header("Content")] public CardSettings Cards;
		[Header("Content")] public DialogueSettings Dialogues;
		[Header("Content")] public ChapterSettings Chapters;
		[Header("Content")] public MinigameSettings Minigames; // optional — built without minigames tolerate null
		[Header("Input")] public float MouseSensitivity = 1.0f;
	}
}