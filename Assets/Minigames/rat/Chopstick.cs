using System.Collections;
using UnityEngine;

namespace Game.Minigames.Rat
{
	// One chopstick. Owns its own id and state; it does not decide whether a tap is
	// legal - RatManager does that and then calls Collect().
	// Ported 1:1 from rice/rat (new-physics Renderer/MaterialPropertyBlock tinting).
	public sealed class Chopstick : MonoBehaviour
	{
		public enum ChopstickState
		{
			Available,
			Collected
		}

		public int Id;

		[Header("Collect feedback")]
		public Color CollectColor = new Color(0.25f, 1f, 0.35f);
		public float FlashSeconds = 0.12f;

		public ChopstickState State { get; private set; }

		private Renderer _rend;
		private MaterialPropertyBlock _mpb;

		// Base collider sizes, so the per-round hit scale is always applied to the
		// original radius rather than compounding on the previous round's value.
		private CapsuleCollider[] _capsules;
		private float[] _baseCapsuleRadii;
		private SphereCollider[] _spheres;
		private float[] _baseSphereRadii;
		private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
		private static readonly int _colorId = Shader.PropertyToID("_Color");

		private void Awake()
		{
			_rend = GetComponent<Renderer>();

			// A property block tints the shared material without instantiating a
			// copy per chopstick, which is what tinting rend.material would do.
			_mpb = new MaterialPropertyBlock();

			CacheColliders();
		}

		private void CacheColliders()
		{
			_capsules = GetComponentsInChildren<CapsuleCollider>(true);
			_baseCapsuleRadii = new float[_capsules.Length];

			for (int i = 0; i < _capsules.Length; i++)
				_baseCapsuleRadii[i] = _capsules[i].radius;

			_spheres = GetComponentsInChildren<SphereCollider>(true);
			_baseSphereRadii = new float[_spheres.Length];

			for (int i = 0; i < _spheres.Length; i++)
				_baseSphereRadii[i] = _spheres[i].radius;
		}

		/// <summary>
		/// Scales the hit area for this round. Bigger targets raise the collection rate
		/// the player can actually achieve without changing the ball physics.
		/// </summary>
		public void SetHitScale(float scale)
		{
			if (_capsules == null)
				CacheColliders();

			float s = Mathf.Max(scale, 0.01f);

			for (int i = 0; i < _capsules.Length; i++)
			{
				if (_capsules[i] != null)
					_capsules[i].radius = _baseCapsuleRadii[i] * s;
			}

			for (int i = 0; i < _spheres.Length; i++)
			{
				if (_spheres[i] != null)
					_spheres[i].radius = _baseSphereRadii[i] * s;
			}
		}

		public void Initialize(int chopstickID)
		{
			Id = chopstickID;
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
			SetTint(CollectColor);

			yield return new WaitForSeconds(FlashSeconds);

			ClearTint();

			gameObject.SetActive(false);
		}

		private void SetTint(Color c)
		{
			if (_rend == null) return;

			_rend.GetPropertyBlock(_mpb);

			// URP Lit uses _BaseColor; set _Color too so this still reads correctly
			// if the placeholder material is ever swapped for a built-in shader.
			_mpb.SetColor(_baseColorId, c);
			_mpb.SetColor(_colorId, c);

			_rend.SetPropertyBlock(_mpb);
		}

		private void ClearTint()
		{
			if (_rend == null) return;

			_rend.SetPropertyBlock(null);
		}
	}
}