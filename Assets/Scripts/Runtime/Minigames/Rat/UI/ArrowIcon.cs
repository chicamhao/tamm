using UnityEngine;
using UnityEngine.UI;

namespace Game.Minigames.Rat
{
	// Directional prompt arrow with a round anchor dot at its tail, drawn procedurally.
	// Covers the whole GDD section 18 prompt set - eight compass directions plus the two
	// rotate arcs - so the Stage 2 gesture prompts need no new icon work later.
	// Stage 1 only ever uses Up. Ported 1:1 from rice/rat.
	public sealed class ArrowIcon : PromptIcon
	{
		public enum Dir
		{
			Up,
			UpLeft,
			Left,
			DownLeft,
			Down,
			DownRight,
			Right,
			UpRight,
			RotateCW,
			RotateCCW
		}

		[Header("Arrow")]
		public Dir direction = Dir.Up;

		[Header("Idle animation")]
		public bool animate = true;
		[Tooltip("How far the arrow drifts along its own axis, in pixels.")]
		public float travel = 9f;
		public float speed = 1.7f;

		private float animShift;

		private void Update()
		{
			if (!animate)
				return;

			float shift = Mathf.Sin(Time.unscaledTime * speed * Mathf.PI) * travel * 0.5f;

			if (!Mathf.Approximately(shift, animShift))
			{
				animShift = shift;
				Redraw();
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			if (direction == Dir.RotateCW || direction == Dir.RotateCCW)
			{
				BuildRotate(vh, outlineWidth, outlineColor);
				BuildRotate(vh, 0f, color);
				return;
			}

			BuildArrow(vh, outlineWidth, outlineColor);
			BuildArrow(vh, 0f, color);
		}

		// =========================================
		// STRAIGHT ARROW
		// =========================================

		private void BuildArrow(VertexHelper vh, float pad, Color32 col)
		{
			float s = Size;
			float angle = AngleFor(direction);

			// Built pointing up, then rotated into place.
			float dotRadius = 0.100f * s + pad;
			Vector2 dotCentre = new Vector2(0f, -0.40f * s);

			Vector2 shaftCentre = new Vector2(0f, -0.13f * s);
			Vector2 shaftSize = new Vector2(0.17f * s + pad * 2f, 0.54f * s + pad * 2f);

			Vector2 headLeft = new Vector2(-0.245f * s, 0.10f * s);
			Vector2 headRight = new Vector2(0.245f * s, 0.10f * s);
			Vector2 headTip = new Vector2(0f, 0.46f * s);

			if (pad > 0f)
				ExpandTriangle(ref headLeft, ref headRight, ref headTip, pad);

			// The whole arrow drifts along its own pointing axis.
			Vector2 drift = new Vector2(0f, animShift);

			AddCircle(vh, Rotate(dotCentre + drift, angle), dotRadius, col);

			AddRoundedRect(
				vh,
				Rotate(shaftCentre + drift, angle),
				shaftSize,
				0.07f * s + pad,
				angle,
				col
			);

			AddTriangle(
				vh,
				Rotate(headLeft + drift, angle),
				Rotate(headRight + drift, angle),
				Rotate(headTip + drift, angle),
				col
			);
		}

		// =========================================
		// ROTATE ARC
		// =========================================

		private void BuildRotate(VertexHelper vh, float pad, Color32 col)
		{
			float s = Size;

			float radius = 0.30f * s;
			float thickness = 0.085f * s + pad * 2f;

			bool clockwise = direction == Dir.RotateCW;

			float start = clockwise ? 300f : -120f;
			float sweep = clockwise ? -250f : 250f;

			AddArc(
				vh,
				Vector2.zero,
				radius + thickness * 0.5f,
				radius - thickness * 0.5f,
				start,
				sweep,
				col
			);

			// Arrow head at the swept end, tangent to the arc.
			float endDeg = start + sweep;
			float endRad = endDeg * Mathf.Deg2Rad;

			Vector2 outward = new Vector2(Mathf.Cos(endRad), Mathf.Sin(endRad));
			Vector2 tangent = clockwise
				? new Vector2(outward.y, -outward.x)
				: new Vector2(-outward.y, outward.x);

			Vector2 baseCentre = outward * radius;
			float headHalf = 0.13f * s + pad;
			float headLength = 0.20f * s + pad;

			Vector2 a = baseCentre + outward * headHalf;
			Vector2 b = baseCentre - outward * headHalf;
			Vector2 tip = baseCentre + tangent * headLength;

			AddTriangle(vh, a, b, tip, col);
		}

		// =========================================
		// HELPERS
		// =========================================

		private static float AngleFor(Dir d)
		{
			switch (d)
			{
				case Dir.Up: return 0f;
				case Dir.UpLeft: return 45f;
				case Dir.Left: return 90f;
				case Dir.DownLeft: return 135f;
				case Dir.Down: return 180f;
				case Dir.DownRight: return 225f;
				case Dir.Right: return 270f;
				case Dir.UpRight: return 315f;
				default: return 0f;
			}
		}

		private static Vector2 Rotate(Vector2 p, float degrees)
		{
			if (Mathf.Approximately(degrees, 0f))
				return p;

			float rad = degrees * Mathf.Deg2Rad;
			float cos = Mathf.Cos(rad);
			float sin = Mathf.Sin(rad);

			return new Vector2(p.x * cos - p.y * sin, p.x * sin + p.y * cos);
		}

		/// <summary>Pushes each corner out from the centroid, so the outline pass sits behind the fill.</summary>
		private static void ExpandTriangle(ref Vector2 a, ref Vector2 b, ref Vector2 c, float pad)
		{
			Vector2 centroid = (a + b + c) / 3f;

			a += (a - centroid).normalized * pad * 1.6f;
			b += (b - centroid).normalized * pad * 1.6f;
			c += (c - centroid).normalized * pad * 1.6f;
		}
	}
}