using UnityEngine;

namespace Game.Minigames.Rat
{
	// Ball physics and state only. Knows nothing about rounds or hearts -
	// GameManager reads these values and decides what they mean.
	//
	// Everything here is measured from the ball's ACTUAL size and launch height rather
	// than assumed from the start point, so scaling the ball does not break miss detection.
	// Ported 1:1 from rice/rat (new-physics API: Rigidbody + ForceMode + Physics.gravity).
	public sealed class BallController : MonoBehaviour
	{
		public Rigidbody rb;
		public Transform startPoint;

		[Header("Throw force (tune these live in Play mode)")]
		public float minThrowForce = 5f;
		public float maxThrowForce = 9f;

		[Header("Throw direction")]
		[Tooltip("0 = always straight up, ignoring swipe direction (the traditional toss). " +
		         "1 = lean the full tilt angle toward wherever the player swiped.")]
		[Range(0f, 1f)]
		public float directionInfluence = 1f;

		[Tooltip("How far the throw may lean away from vertical, in degrees.")]
		[Range(0f, 75f)]
		public float maxTiltDegrees = 30f;

		[Header("Gravity")]
		[Tooltip("1 = normal gravity, lower = floatier. Lengthens the action window.")]
		[Range(0.1f, 2f)]
		public float gravityScale = 1f;

		[Header("Rest position")]
		[Tooltip("Keep the ball clear of whatever is under the start point.")]
		public bool restOnSurface = true;

		[Tooltip("Gap left between the ball and the surface it rests on.")]
		public float surfaceSkin = 0.02f;

		[Header("Miss detection")]
		[Tooltip("The ball must climb this far above the launch height before a miss can be registered.")]
		public float riseThreshold = 0.3f;

		[Tooltip("Speed below which a thrown ball counts as settled.")]
		public float settleSpeed = 0.35f;

		[Tooltip("How long it must stay settled before the turn is called a miss.")]
		public float settleSeconds = 0.35f;

		private bool thrown;
		private bool hasRisen;
		private float lastVerticalSpeed;
		private float launchY;
		private float settledFor;

		private SphereCollider sphere;

		private static readonly RaycastHit[] ProbeBuffer = new RaycastHit[8];

		/// <summary>Y of the start point. Kept for reference; the catch line uses LaunchY.</summary>
		public float StartY => startPoint != null ? startPoint.position.y : 0f;

		/// <summary>
		/// The height the ball was actually thrown from. The catch line is measured
		/// from this, not the start point, because a scaled ball rests higher.
		/// </summary>
		public float LaunchY => thrown ? launchY : transform.position.y;

		/// <summary>World-space radius, including whatever scale the ball has been given.</summary>
		public float Radius
		{
			get
			{
				Vector3 s = transform.lossyScale;
				float maxScale = Mathf.Max(Mathf.Abs(s.x), Mathf.Max(Mathf.Abs(s.y), Mathf.Abs(s.z)));
				float local = sphere != null ? sphere.radius : 0.5f;
				return local * maxScale;
			}
		}

		/// <summary>Normalised swipe strength of the last throw, 0..1.</summary>
		public float LastThrowStrength { get; private set; }

		/// <summary>The world direction the last throw was actually launched along.</summary>
		public Vector3 LastThrowDirection { get; private set; }

		/// <summary>
		/// Total hang time of the last throw, in seconds. This is the player's action
		/// window - derived from the VERTICAL component of the launch velocity.
		/// </summary>
		public float PredictedAirtime
		{
			get
			{
				float g = EffectiveGravity;
				if (g <= 0.01f) return 0f;
				return 2f * lastVerticalSpeed / g;
			}
		}

		/// <summary>Gravity actually acting on the ball this round, in m/s2.</summary>
		public float EffectiveGravity => Mathf.Max(Physics.gravity.magnitude * gravityScale, 0.0001f);

		private void Awake()
		{
			if (rb == null) rb = GetComponent<Rigidbody>();
			sphere = GetComponent<SphereCollider>();

			// Gravity is applied by hand in FixedUpdate so it can be scaled per round.
			// Unity 3D has no per-rigidbody gravity scale; that is a 2D-only feature.
			if (rb != null) rb.useGravity = false;

			LastThrowDirection = Vector3.up;
		}

		private void Start() => ResetBall();

		private void Update()
		{
			if (!thrown) return;

			// Arm the miss check once the ball is genuinely in the air.
			if (!hasRisen && transform.position.y > launchY + riseThreshold) hasRisen = true;

			// Track how long a risen ball has been essentially stationary.
			if (hasRisen && rb.linearVelocity.sqrMagnitude < settleSpeed * settleSpeed)
				settledFor += Time.deltaTime;
			else
				settledFor = 0f;
		}

		private void FixedUpdate()
		{
			// Hand-applied gravity, because useGravity was switched off so it can be
			// scaled per round. Acceleration mode ignores mass, matching real gravity.
			if (rb != null && !rb.isKinematic)
				rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
		}

		/// <summary>Straight-up throw, the traditional toss.</summary>
		public void Throw(float normalizedForce) => Throw(normalizedForce, Vector3.zero);

		/// <summary>
		/// Throws along <paramref name="swipeDirection"/>, a world-space horizontal vector.
		/// Pass zero for straight up. The lean is clamped to maxTiltDegrees and scaled by
		/// directionInfluence, so the ball always keeps enough vertical velocity to be catchable.
		/// </summary>
		public void Throw(float normalizedForce, Vector3 swipeDirection)
		{
			if (thrown) return;

			thrown = true;
			hasRisen = false;
			settledFor = 0f;

			// Remember where it actually left from, whatever its size.
			launchY = transform.position.y;

			LastThrowStrength = Mathf.Clamp01(normalizedForce);

			Vector3 dir = ResolveThrowDirection(swipeDirection);
			LastThrowDirection = dir;

			// Enable physics
			rb.isKinematic = false;

			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;

			float force = Mathf.Lerp(minThrowForce, maxThrowForce, LastThrowStrength);
			rb.AddForce(dir * force, ForceMode.Impulse);

			// An impulse imparts force/mass of velocity. Hang time depends on the
			// vertical part of it only.
			float speed = force / Mathf.Max(rb.mass, 0.0001f);
			lastVerticalSpeed = speed * Mathf.Max(dir.y, 0f);
		}

		/// <summary>
		/// Tilts straight-up toward the swipe direction by at most maxTiltDegrees,
		/// scaled by directionInfluence.
		/// </summary>
		private Vector3 ResolveThrowDirection(Vector3 swipeDirection)
		{
			// Only the horizontal part of the swipe steers the throw.
			Vector3 horizontal = new Vector3(swipeDirection.x, 0f, swipeDirection.z);

			if (horizontal.sqrMagnitude < 0.000001f) return Vector3.up;

			float tilt = maxTiltDegrees * Mathf.Clamp01(directionInfluence);
			if (tilt <= 0.01f) return Vector3.up;

			horizontal.Normalize();

			// Rotate the up vector toward the swipe direction by the tilt angle.
			return Vector3.RotateTowards(Vector3.up, horizontal, tilt * Mathf.Deg2Rad, 0f).normalized;
		}

		public void Catch()
		{
			if (!thrown) return;

			thrown = false;
			hasRisen = false;
			settledFor = 0f;

			ReturnToStart();
		}

		public void ResetBall()
		{
			thrown = false;
			hasRisen = false;
			settledFor = 0f;
			lastVerticalSpeed = 0f;
			LastThrowStrength = 0f;
			LastThrowDirection = Vector3.up;

			ReturnToStart();
		}

		private void ReturnToStart()
		{
			// Stop physics before moving the transform, otherwise the old velocity carries.
			if (!rb.isKinematic)
			{
				rb.linearVelocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
			}

			rb.isKinematic = true;

			if (startPoint == null) return;

			transform.position = RestPosition();
		}

		/// <summary>
		/// The start point, lifted if the ball's own radius would push it through whatever
		/// is underneath. Probing beats hard-coding a height: the designer can rescale the
		/// ball freely and it still sits on the surface instead of inside it.
		/// </summary>
		private Vector3 RestPosition()
		{
			Vector3 p = startPoint.position;

			if (!restOnSurface) return p;

			float radius = Radius;

			int count = Physics.RaycastNonAlloc(
				new Vector3(p.x, p.y + 20f, p.z),
				Vector3.down,
				ProbeBuffer,
				60f,
				~0,
				QueryTriggerInteraction.Ignore
			);

			float bestY = float.NegativeInfinity;

			for (int i = 0; i < count; i++)
			{
				// Skip the ball itself, and anything it should not perch on.
				if (ProbeBuffer[i].collider.GetComponentInParent<BallController>() != null) continue;
				if (ProbeBuffer[i].collider.GetComponentInParent<Chopstick>() != null) continue;

				if (ProbeBuffer[i].point.y > bestY) bestY = ProbeBuffer[i].point.y;
			}

			if (bestY > float.NegativeInfinity)
				p.y = Mathf.Max(p.y, bestY + radius + surfaceSkin);

			return p;
		}

		public bool IsThrown() => thrown;

		public bool IsFalling()
		{
			if (rb.isKinematic) return false;
			return rb.linearVelocity.y < 0f;
		}

		/// <summary>
		/// True once a thrown ball has climbed clear of the launch height and then dropped
		/// back to <paramref name="y"/> on its way down.
		/// </summary>
		public bool HasFallenBelow(float y)
		{
			if (!thrown || !hasRisen) return false;
			return transform.position.y <= y && rb.linearVelocity.y <= 0f;
		}

		/// <summary>
		/// Safety net: a thrown ball that has risen and then come to rest can never be
		/// caught, whatever the geometry.
		/// </summary>
		public bool HasSettledAfterThrow() => thrown && hasRisen && settledFor >= settleSeconds;

		/// <summary>Height above the catch line, used for the time-remaining readout.</summary>
		public float HeightAboveCatchLine(float catchLineY) => transform.position.y - catchLineY;
	}
}