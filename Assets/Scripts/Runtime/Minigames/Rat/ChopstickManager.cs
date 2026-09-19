using System.Collections.Generic;
using UnityEngine;

namespace Game.Minigames.Rat
{
	// Owns the chopstick pool: creates them from the prefab, scatters them, hands
	// them out. Single source of truth - GameManager asks it rather than keeping
	// its own array. Ported from rice/rat, minus the balance-table pool sizing.
	public sealed class ChopstickManager : MonoBehaviour
	{
		[Header("Chopstick Setup")]
		public GameObject chopstickPrefab;

		[Tooltip("How many chopstick objects to create. Rounds may use fewer than this, " +
		         "never more, so set it to the largest total any round asks for.")]
		public int poolSize = RoundRules.TotalChopsticks;

		[Tooltip("Default count when no round says otherwise.")]
		public int chopstickCount = RoundRules.TotalChopsticks;

		[Header("Spawn Area")]
		public float spawnAreaX = 5f;
		public float spawnAreaZ = 5f;

		private Chopstick[] chopsticks;

		// Half the chopstick's vertical thickness when it lies flat. Measured from the
		// mesh rather than serialised: the old serialised value floated every stick
		// a fifth of a unit above the table surface.
		private float restHeight = 0.05f;

		// How many of the pool this round is using. The rest stay hidden.
		private int activeCount;

		/// <summary>How many chopsticks are still on the table.</summary>
		public int AvailableCount
		{
			get
			{
				if (chopsticks == null) return 0;

				int n = 0;

				for (int i = 0; i < chopsticks.Length; i++)
				{
					if (i >= activeCount) continue;
					if (chopsticks[i] != null && !chopsticks[i].IsCollected()) n++;
				}

				return n;
			}
		}

		private void Awake() => CreateChopsticks();

		private void CreateChopsticks()
		{
			if (chopsticks != null) return;

			if (chopstickPrefab == null)
			{
				Debug.LogError("ChopstickManager: chopstickPrefab is not assigned.", this);
				return;
			}

			int size = Mathf.Max(poolSize, chopstickCount);

			// Size the pool to the largest round, then show only what each round needs.
			// Chopsticks are never created mid-run: instantiating during a turn would
			// stall the frame the player is trying to act in.
			chopsticks = new Chopstick[size];

			for (int i = 0; i < size; i++)
			{
				GameObject obj = Instantiate(chopstickPrefab, transform);

				Chopstick chopstick = obj.GetComponent<Chopstick>();

				if (chopstick == null)
				{
					Debug.LogError("ChopstickManager: prefab has no Chopstick component.", this);
					continue;
				}

				chopstick.Initialize(i + 1);

				if (i == 0)
					restHeight = MeasureRestHeight(obj);

				obj.SetActive(false);

				chopsticks[i] = chopstick;
			}
		}

		/// <summary>
		/// How high a chopstick's centre must sit to rest on a surface, taken from the
		/// renderer bounds while it lies flat.
		/// </summary>
		private float MeasureRestHeight(GameObject obj)
		{
			Renderer r = obj.GetComponentInChildren<Renderer>();

			if (r == null) return restHeight;

			return Mathf.Max(r.bounds.extents.y, 0.01f);
		}

		/// <summary>Scatters all 10 chopsticks across the table.</summary>
		public void DropChopsticks() => DropChopsticks(chopstickCount);

		/// <summary>Scatters <paramref name="count"/> chopsticks and hides the rest of the pool.</summary>
		public void DropChopsticks(int count)
		{
			if (chopsticks == null) return;

			activeCount = Mathf.Clamp(count, 0, chopsticks.Length);

			if (count > chopsticks.Length)
			{
				Debug.LogWarning(
					"ChopstickManager: round asked for " + count + " chopsticks but the pool " +
					"holds " + chopsticks.Length + ". Raise poolSize.", this);
			}

			for (int i = 0; i < chopsticks.Length; i++)
			{
				if (chopsticks[i] == null) continue;

				if (i >= activeCount)
				{
					chopsticks[i].gameObject.SetActive(false);
					continue;
				}

				chopsticks[i].ResetChopstick();

				RandomizePosition(chopsticks[i]);
			}
		}

		/// <summary>Applies this round's spawn footprint and hit-target size.</summary>
		public void Configure(float areaX, float areaZ, float targetScale)
		{
			spawnAreaX = areaX;
			spawnAreaZ = areaZ;

			if (chopsticks == null) return;

			for (int i = 0; i < chopsticks.Length; i++)
			{
				if (chopsticks[i] != null)
					chopsticks[i].SetHitScale(targetScale);
			}
		}

		/// <summary>Puts a specific set of chopsticks back on the table, undoing a failed turn.</summary>
		public void ReturnToTable(List<Chopstick> toReturn)
		{
			if (toReturn == null) return;

			for (int i = 0; i < toReturn.Count; i++)
			{
				if (toReturn[i] == null) continue;

				toReturn[i].ResetChopstick();

				RandomizePosition(toReturn[i]);
			}
		}

		private void RandomizePosition(Chopstick chopstick)
		{
			chopstick.transform.position = FindSpawnPoint();

			// X 90 lays the cylinder flat; the Y term spins it on the table surface.
			chopstick.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
		}

		/// <summary>
		/// Picks a point that is genuinely on the table by probing straight down and
		/// keeping whatever surface it lands on. Probing beats hard-coding the table
		/// size: the serialised spawn area is wider than the table's short axis.
		/// </summary>
		private Vector3 FindSpawnPoint()
		{
			const int maxAttempts = 24;
			const float probeHeight = 10f;
			const float probeDistance = 30f;

			for (int attempt = 0; attempt < maxAttempts; attempt++)
			{
				float x = Random.Range(-spawnAreaX, spawnAreaX);
				float z = Random.Range(-spawnAreaZ, spawnAreaZ);

				RaycastHit hit;

				bool blocked = Physics.Raycast(
					new Vector3(x, probeHeight, z),
					Vector3.down,
					out hit,
					probeDistance,
					~0,
					QueryTriggerInteraction.Ignore
				);

				if (!blocked) continue;

				// Do not stack on another chopstick, and do not land on the ball.
				if (hit.collider.GetComponentInParent<Chopstick>() != null) continue;
				if (hit.collider.GetComponentInParent<BallController>() != null) continue;

				return new Vector3(x, hit.point.y + restHeight, z);
			}

			// Nothing solid under the spawn area at all. Fall back to the origin so the
			// chopsticks stay visible and the misconfiguration is obvious on screen.
			return new Vector3(0f, restHeight, 0f);
		}

		public Chopstick GetChopstick(int index)
		{
			if (chopsticks == null || index < 0 || index >= chopsticks.Length) return null;

			return chopsticks[index];
		}

		public Chopstick[] GetAllChopsticks() => chopsticks;
	}
}