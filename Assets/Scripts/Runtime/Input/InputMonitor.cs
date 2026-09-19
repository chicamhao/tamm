using Game.Core;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Game.Input
{
	// Static _pointerUnlockTime is intentional cross-instance state (minigame
	// unlock and level-reload re-lock land on different instances), so opt out of
	// Unity 6's auto statics cleanup that would wipe it exactly at that handoff.
	[Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
	public sealed class InputMonitor : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 Move;
		public Vector2 Look;
		public bool Jump;
		public bool Sprint;

		[Header("Movement Settings")]
		public bool AnalogMovement;

		[Header("Mouse Cursor Settings")]
		public bool CursorLocked = true;
		public bool CursorInputForLook = true;

		// Gameplay edge-reads (additive only — the Move/Look/Jump/Sprint path above is untouched):
		// actions are bound from the default actions asset (the same one PlayerInput feeds
		// On* events from), remappable in the InputSystem editor like every other action.
		private InputAction _interactAction;
		private InputAction _pauseAction;
		private Core.InputSettings _sensitivity;

		private void Start()
		{
			_sensitivity = Services.InputSettings;
			InputActionAsset asset = InputSystem.actions;
			Assert.IsNotNull(asset);
			_interactAction = asset.FindAction("Player/Interact");
			_interactAction?.Enable();
			_pauseAction = asset.FindAction("Player/Pause");
			_pauseAction?.Enable();

			// Apply the authored cursor state once at startup. The first focus event fires
			// when the window opens — before an additively-loaded level's InputMonitor
			// exists — so without this the cursor stays visible until the next focus change.
			SetCursorState(CursorLocked);
		}

		public bool GetInteractInputDown() => InputEnabled && _interactAction != null && _interactAction.WasPressedThisFrame();
		public bool GetPauseInputDown() => _pauseAction != null && _pauseAction.WasPressedThisFrame();


		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if (CursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void MoveInput(Vector2 newMoveDirection)
		{
			Move = InputEnabled ? newMoveDirection : Vector2.zero;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			// Sensitivity is applied here, not in the controller: every look source
			// (mouse, touch) converges on this setter, and Controller.cs stays untouched.
			// ponytail: scales gamepad look too — if gamepad feel matters later,
			// gate on the current device here. Also: if Controller.cs ever gets the
			// settings-branch sensitivity multiply, drop this one to avoid double-scale.
			float sensitivity = _sensitivity.MouseSensitivity.Value;
			Look = InputEnabled ? newLookDirection * sensitivity : Vector2.zero;
		}

		public void JumpInput(bool newJumpState)
		{
			Jump = InputEnabled && newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			Sprint = InputEnabled && newSprintState;
		}

		// Pause freeze: the menu disables input so Look/Move zero out and the camera
		// can't keep rotating (look is not deltaTime-scaled, so timeScale 0 alone
		// doesn't stop it). Re-enabling resumes from a clean slate.
		public bool InputEnabled = true;
		public void DisableInput() => InputEnabled = false;
		public void EnableInput() => InputEnabled = true;


		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(CursorLocked);
		}

		// WebGL: Chrome rejects requestPointerLock() within ~1s of exitPointerLock
		// (SecurityError in console, cursor stays free until the next request). Fast
		// minigames (rat) unlock at takeover and re-lock on return inside that window,
		// so defer the re-lock past it. The unlock timestamp is static: unlock and
		// re-lock land on different instances (parked core monitor vs freshly loaded
		// level monitor), and the first monitor may be destroyed before its deferred
		// lock fires — the fresh level's Start() re-requests through the same guard.
		private static float _pointerUnlockTime = float.NegativeInfinity;
		private bool _relockPending;
		private float _relockAt;

		public void SetCursorState(bool newState)
		{
			if (!newState)
			{
				_pointerUnlockTime = Time.time;
				_relockPending = false;
				Cursor.lockState = CursorLockMode.None;
				return;
			}

			if (Application.platform == RuntimePlatform.WebGLPlayer && Time.time - _pointerUnlockTime < 1f)
			{
				_relockPending = true;
				_relockAt = _pointerUnlockTime + 1f;
				return;
			}

			_relockPending = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Update()
		{
			if (_relockPending && Time.time >= _relockAt)
			{
				_relockPending = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}
}