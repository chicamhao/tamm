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
		public Camera cam;
		public BallController ball;
		public ChopstickManager chopstickManager;

		[Header("Swipe thresholds (pixels)")]
		public float minSwipeDistance = 50f;
		public float maxSwipeDistance = 400f;

		[Tooltip("A pointer that moves less than this counts as a tap, not a swipe.")]
		public float tapSlop = 24f;

		private Vector2 pointerDownPosition;
		private bool pointerIsDown;

		private static readonly RaycastHit[] HitBuffer = new RaycastHit[16];

		private void Awake()
		{
			if (cam == null)
				cam = Camera.main;

			if (ball == null)
				ball = FindFirstObjectByType<BallController>();

			if (chopstickManager == null)
				chopstickManager = FindFirstObjectByType<ChopstickManager>();
		}

		private void Update()
		{
			Vector2 position;

			if (TryReadPointerDown(out position))
			{
				pointerDownPosition = position;
				pointerIsDown = true;
			}

			if (pointerIsDown && TryReadPointerUp(out position))
			{
				pointerIsDown = false;
				ResolveGesture(pointerDownPosition, position);
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
			if (delta.magnitude > minSwipeDistance)
			{
				float strength = Mathf.Clamp01(delta.magnitude / Mathf.Max(maxSwipeDistance, 1f));

				RatManager.Instance.OnThrowInput(strength);
				return;
			}

			if (delta.magnitude <= tapSlop)
				HandleTap(to);
		}

		private void HandleTap(Vector2 screenPosition)
		{
			if (RatManager.Instance == null)
				return;

			if (cam == null)
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

			if (cam == null)
				return;

			Ray ray = cam.ScreenPointToRay(screenPosition);

			int count = Physics.RaycastNonAlloc(
				ray,
				HitBuffer,
				1000f,
				~0,
				QueryTriggerInteraction.Ignore
			);

			float nearestChopstickDistance = float.MaxValue;
			float nearestBallDistance = float.MaxValue;

			for (int i = 0; i < count; i++)
			{
				RaycastHit hit = HitBuffer[i];

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