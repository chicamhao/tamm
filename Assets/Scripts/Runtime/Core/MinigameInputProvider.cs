using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core
{
	// Default IMinigameInput: reads mouse + the first touch screen, whichever
	// device is active. Lives with the other input code in the core; a minigame
	// never constructs this itself — MinigameService passes it in OnStart.
	public sealed class MinigameInputProvider : IMinigameInput
	{
		public bool GetPointerDown(out Vector2 position)
		{
			if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
			{
				position = Mouse.current.position.ReadValue();
				return true;
			}

			if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
			{
				position = Touchscreen.current.primaryTouch.position.ReadValue();
				return true;
			}

			position = Vector2.zero;
			return false;
		}

		public bool GetPointerUp(out Vector2 position)
		{
			if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
			{
				position = Mouse.current.position.ReadValue();
				return true;
			}

			if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
			{
				position = Touchscreen.current.primaryTouch.position.ReadValue();
				return true;
			}

			position = Vector2.zero;
			return false;
		}

		public Vector2 GetPointerPosition()
		{
			if (Mouse.current != null)
				return Mouse.current.position.ReadValue();

			if (Touchscreen.current != null)
				return Touchscreen.current.primaryTouch.position.ReadValue();

			return Vector2.zero;
		}
	}
}