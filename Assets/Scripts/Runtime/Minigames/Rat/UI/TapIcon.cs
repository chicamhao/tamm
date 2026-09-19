using UnityEngine;
using UnityEngine.UI;

namespace Game.Minigames.Rat.UI
{
	// Hand-tap prompt: a simple hand silhouette under an expanding ripple ring.
	// This is the badge that sits on the thing the player must touch - a chopstick
	// during the picking phase, the ball during the catch phase. Ported 1:1 from rice/rat.
	public sealed class TapIcon : PromptIcon
	{
		[Header("Ripple")]
		public bool Ripple = true;
		public Color RippleColor = new Color(1f, 1f, 1f, 0.9f);
		public float RippleSpeed = 1.25f;

		private float _ripplePhase;

		private void Update()
		{
			if (!Ripple)
				return;

			float phase = (Time.unscaledTime * RippleSpeed) % 1f;

			if (Mathf.Abs(phase - _ripplePhase) > 0.004f)
			{
				_ripplePhase = phase;
				Redraw();
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			if (Ripple)
				BuildRipple(vh);

			BuildHand(vh, OutlineWidth, OutlineColor);
			BuildHand(vh, 0f, color);
		}

		private void BuildRipple(VertexHelper vh)
		{
			float s = Size;

			float radius = Mathf.Lerp(0.30f * s, 0.54f * s, _ripplePhase);
			float thickness = Mathf.Lerp(0.045f * s, 0.015f * s, _ripplePhase);

			Color c = RippleColor;
			c.a *= 1f - _ripplePhase;

			AddRing(
				vh,
				Vector2.zero,
				radius + thickness * 0.5f,
				radius - thickness * 0.5f,
				c
			);
		}

		/// <summary>
		/// Palm plus four fingers plus an angled thumb. Deliberately blocky - it reads
		/// as a hand at prompt size and costs nothing to draw.
		/// </summary>
		private void BuildHand(VertexHelper vh, float pad, Color32 col)
		{
			float s = Size;
			float p2 = pad * 2f;

			// Palm
			AddRoundedRect(
				vh,
				new Vector2(0f, -0.14f * s),
				new Vector2(0.44f * s + p2, 0.40f * s + p2),
				0.13f * s + pad,
				0f,
				col
			);

			// Four fingers, bottoms tucked into the palm.
			float[] fingerX = { -0.150f, -0.050f, 0.050f, 0.150f };
			float[] fingerH = { 0.28f, 0.35f, 0.32f, 0.25f };

			for (int i = 0; i < fingerX.Length; i++)
			{
				float h = fingerH[i] * s;

				AddRoundedRect(
					vh,
					new Vector2(fingerX[i] * s, -0.02f * s + h * 0.5f),
					new Vector2(0.095f * s + p2, h + p2),
					0.048f * s + pad,
					0f,
					col
				);
			}

			// Thumb, angled off the left side.
			AddRoundedRect(
				vh,
				new Vector2(-0.235f * s, -0.13f * s),
				new Vector2(0.095f * s + p2, 0.26f * s + p2),
				0.048f * s + pad,
				38f,
				col
			);
		}
	}
}