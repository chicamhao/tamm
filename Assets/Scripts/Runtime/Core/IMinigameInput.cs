using UnityEngine;

namespace Game.Core
{
	// Input channel handed to a minigame at OnStart. Minigames poll this instead
	// of reading UnityEngine.InputSystem directly, so the core owns which devices
	// map to "the pointer" and each imported minigame repo stays device-agnostic.
	// Deliberately minimal: just the pointer gestures every minigame in this
	// project uses. Add methods here only when a real minigame needs them.
	public interface IMinigameInput
	{
		/// <summary>True when the pointer went down this frame; position in screen pixels.</summary>
		bool GetPointerDown(out Vector2 position);

		/// <summary>True when the pointer went up this frame; position in screen pixels.</summary>
		bool GetPointerUp(out Vector2 position);

		/// <summary>Current pointer position in screen pixels.</summary>
		Vector2 GetPointerPosition();
	}
}