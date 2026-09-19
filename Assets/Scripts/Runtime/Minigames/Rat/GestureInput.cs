using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Minigames.Rat
{
	// Input only: turns mouse drags and touches into intents and hands them to
	// RatManager. Holds no game rules and no state beyond the current gesture.
	//
	// Ported from rice/rat minus the sweep (drag-to-collect) affordance and the
	// swipe-direction steering — v1 is tap-only, every throw goes straight up and
	// only the swipe length sets the throw height.
	public sealed class GestureInput : MonoBehaviour
	{
		[Header("Optional - found automatically if left empty")]
		public Camera Cam;
		public BallController Ball;
		public ChopstickManager ChopstickManager;

		[Header("Swipe thresholds (pixels)")]
		public float MinSwipeDistance = 50f;
		public float MaxSwipeDistance = 400f;

		[Tooltip("A pointer that moves less than this counts as a tap, not a swipe.")]
		public float TapSlop = 24f;

		private Vector2 _pointerDownPosition;
		private bool _pointerIsDown;

		private static readonly RaycastHit[] _hitBuffer = new RaycastHit[16];

		private void Awake()
		{
			if (Cam == null)
				Cam = Camera.main;

			if (Ball == null)
				Ball = FindFirstObjectByType<BallController>();

			if (ChopstickManager == null)
				ChopstickManager = FindFirstObjectByType<ChopstickManager>();
		}

		private void Update()
		{
			Vector2 position;

			if (TryReadPointerDown(out position))
			{
				_pointerDownPosition = position;
				_pointerIsDown = true;
			}

			if (_pointerIsDown && TryReadPointerUp(out position))
			{
				_pointerIsDown = false;
				ResolveGesture(_pointerDownPosition, position);
			}
		}

		// =========================================
		// POINTER (mouse or touch)
		// =========================================

		private bool TryReadPointerDown(out Vector2 position)
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

		private bool TryReadPointerUp(out Vector2 position)
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

		// =========================================
		// GESTURE
		// =========================================

		private void ResolveGesture(Vector2 from, Vector2 to)
		{
			if (RatManager.Instance == null)
				return;

			Vector2 delta = to - from;

			// Any swipe past the threshold throws. Checked before tap so a fast flick is
			// never mistaken for a tap on whatever happened to be under the finger.
			if (delta.magnitude > MinSwipeDistance)
			{
				float strength = Mathf.Clamp01(delta.magnitude / Mathf.Max(MaxSwipeDistance, 1f));

				RatManager.Instance.OnThrowInput(strength);
				return;
			}

			if (delta.magnitude <= TapSlop)
				HandleTap(to);
		}

		private void HandleTap(Vector2 screenPosition)
		{
			if (RatManager.Instance == null)
				return;

			if (Cam == null)
				return;

			Chopstick nearestChopstick;
			BallController hitBall;

			Probe(screenPosition, out nearestChopstick, out hitBall);

			// Both are state-guarded inside RatManager, so offering both candidates is
			// safe and avoids a tie-break here: only the one that matches the current
			// phase can act. The ball and a chopstick often overlap on screen.
			if (nearestChopstick != null)
				RatManager.Instance.OnChopstickTapped(nearestChopstick);

			if (hitBall != null)
				RatManager.Instance.TryCatchBall();
		}

		/// <summary>
		/// Raycasts the whole line, not just the nearest hit: the table sits between the
		/// camera and the chopsticks lying on it, so the first thing hit is often not the
		/// target. Returns the nearest available chopstick and the nearest ball.
		/// </summary>
		private void Probe(Vector2 screenPosition, out Chopstick chopstick, out BallController ballHit)
		{
			chopstick = null;
			ballHit = null;

			if (Cam == null)
				return;

			Ray ray = Cam.ScreenPointToRay(screenPosition);

			int count = Physics.RaycastNonAlloc(
				ray,
				_hitBuffer,
				1000f,
				~0,
				QueryTriggerInteraction.Ignore
			);

			float nearestChopstickDistance = float.MaxValue;
			float nearestBallDistance = float.MaxValue;

			for (int i = 0; i < count; i++)
			{
				RaycastHit hit = _hitBuffer[i];

				Chopstick c = hit.collider.GetComponentInParent<Chopstick>();

				if (c != null && !c.IsCollected() && hit.distance < nearestChopstickDistance)
				{
					chopstick = c;
					nearestChopstickDistance = hit.distance;
					continue;
				}

				BallController b = hit.collider.GetComponentInParent<BallController>();

				if (b != null && hit.distance < nearestBallDistance)
				{
					ballHit = b;
					nearestBallDistance = hit.distance;
				}
			}
		}
	}
}