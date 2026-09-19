using System.Collections;
using UnityEngine;

namespace Game.Minigames.Rat
{
	// One chopstick. Owns its own id and state; it does not decide whether a tap is
	// legal - GameManager does that and then calls Collect().
	// Ported 1:1 from rice/rat (new-physics Renderer/MaterialPropertyBlock tinting).
	public sealed class Chopstick : MonoBehaviour
	{
		public enum ChopstickState
		{
			Available,
			Collected
		}

		public int id;

		[Header("Collect feedback")]
		public Color collectColor = new Color(0.25f, 1f, 0.35f);
		public float flashSeconds = 0.12f;

		public ChopstickState State { get; private set; }

		private Renderer rend;
		private MaterialPropertyBlock mpb;

		// Base collider sizes, so the per-round hit scale is always applied to the
		// original radius rather than compounding on the previous round's value.
		private CapsuleCollider[] capsules;
		private float[] baseCapsuleRadii;
		private SphereCollider[] spheres;
		private float[] baseSphereRadii;
		private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
		private static readonly int ColorId = Shader.PropertyToID("_Color");

		private void Awake()
		{
			rend = GetComponent<Renderer>();

			// A property block tints the shared material without instantiating a
			// copy per chopstick, which is what tinting rend.material would do.
			mpb = new MaterialPropertyBlock();

			CacheColliders();
		}

		private void CacheColliders()
		{
			capsules = GetComponentsInChildren<CapsuleCollider>(true);
			baseCapsuleRadii = new float[capsules.Length];

			for (int i = 0; i < capsules.Length; i++)
				baseCapsuleRadii[i] = capsules[i].radius;

			spheres = GetComponentsInChildren<SphereCollider>(true);
			baseSphereRadii = new float[spheres.Length];

			for (int i = 0; i < spheres.Length; i++)
				baseSphereRadii[i] = spheres[i].radius;
		}

		/// <summary>
		/// Scales the hit area for this round. Bigger targets raise the collection rate
		/// the player can actually achieve without changing the ball physics.
		/// </summary>
		public void SetHitScale(float scale)
		{
			if (capsules == null)
				CacheColliders();

			float s = Mathf.Max(scale, 0.01f);

			for (int i = 0; i < capsules.Length; i++)
			{
				if (capsules[i] != null)
					capsules[i].radius = baseCapsuleRadii[i] * s;
			}

			for (int i = 0; i < spheres.Length; i++)
			{
				if (spheres[i] != null)
					spheres[i].radius = baseSphereRadii[i] * s;
			}
		}

		public void Initialize(int chopstickID)
		{
			id = chopstickID;
			ResetChopstick();
		}

		public void ResetChopstick()
		{
			StopAllCoroutines();

			State = ChopstickState.Available;

			ClearTint();

			gameObject.SetActive(true);
		}

		/// <summary>
		/// Marks this chopstick collected. Returns false if it was already taken,
		/// so a double tap in the same frame cannot count twice.
		/// </summary>
		public bool TryCollect()
		{
			if (State == ChopstickState.Collected) return false;

			State = ChopstickState.Collected;

			gameObject.SetActive(true);

			StopAllCoroutines();
			StartCoroutine(FlashThenHide());

			return true;
		}

		public bool IsCollected() => State == ChopstickState.Collected;

		private IEnumerator FlashThenHide()
		{
			SetTint(collectColor);

			yield return new WaitForSeconds(flashSeconds);

			ClearTint();

			gameObject.SetActive(false);
		}

		private void SetTint(Color c)
		{
			if (rend == null) return;

			rend.GetPropertyBlock(mpb);

			// URP Lit uses _BaseColor; set _Color too so this still reads correctly
			// if the placeholder material is ever swapped for a built-in shader.
			mpb.SetColor(BaseColorId, c);
			mpb.SetColor(ColorId, c);

			rend.SetPropertyBlock(mpb);
		}

		private void ClearTint()
		{
			if (rend == null) return;

			rend.SetPropertyBlock(null);
		}
	}
}