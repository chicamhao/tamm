using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Minigames.Rat
{
	// Base for the Cooking-Mama-style action prompt icons. Every icon builds its own
	// geometry in OnPopulateMesh, so the prototype ships with no sprites, no atlas and
	// nothing to wire in the Inspector. Ported 1:1 from rice/rat.
	//
	// The CanvasRenderer requirement is declared HERE, not just inherited from Graphic:
	// inherited RequireComponent did not get applied through this abstract base, so every
	// icon ended up with no CanvasRenderer, generated a correct mesh, and drew nothing.
	[RequireComponent(typeof(CanvasRenderer))]
	public abstract class PromptIcon : MaskableGraphic
	{
		[Header("Outline")]
		public Color outlineColor = new Color(0.06f, 0.24f, 0.62f, 1f);
		public float outlineWidth = 6f;

		[Tooltip("Width of the transparent skirt used to soften edges, in pixels.")]
		public float edgeSoftness = 1.25f;

		private static readonly List<Vector2> Scratch = new List<Vector2>(64);

		/// <summary>Shortest side of the rect. All icon geometry is expressed as a fraction of it.</summary>
		protected float Size
		{
			get
			{
				Rect r = rectTransform.rect;
				return Mathf.Min(r.width, r.height);
			}
		}

		// =========================================
		// CONVEX SHAPES (anti-aliased)
		// =========================================

		/// <summary>
		/// Fills a convex polygon and adds a transparent skirt around it so the edge
		/// does not stair-step. Only valid for convex point sets.
		/// </summary>
		protected void AddConvexPolygon(VertexHelper vh, List<Vector2> points, Color32 color)
		{
			if (points == null || points.Count < 3)
				return;

			Vector2 centre = Vector2.zero;

			for (int i = 0; i < points.Count; i++)
				centre += points[i];

			centre /= points.Count;

			int centreIndex = vh.currentVertCount;

			vh.AddVert(centre, color, Vector2.zero);

			int first = vh.currentVertCount;

			for (int i = 0; i < points.Count; i++)
				vh.AddVert(points[i], color, Vector2.zero);

			for (int i = 0; i < points.Count; i++)
			{
				int a = first + i;
				int b = first + ((i + 1) % points.Count);

				vh.AddTriangle(centreIndex, a, b);
			}

			if (edgeSoftness <= 0f)
				return;

			Color32 fade = new Color32(color.r, color.g, color.b, 0);

			int skirt = vh.currentVertCount;

			for (int i = 0; i < points.Count; i++)
			{
				Vector2 outward = (points[i] - centre).normalized;

				vh.AddVert(points[i] + outward * edgeSoftness, fade, Vector2.zero);
			}

			for (int i = 0; i < points.Count; i++)
			{
				int next = (i + 1) % points.Count;

				vh.AddTriangle(first + i, skirt + i, skirt + next);
				vh.AddTriangle(first + i, skirt + next, first + next);
			}
		}

		protected void AddCircle(VertexHelper vh, Vector2 centre, float radius, Color32 color, int segments = 28)
		{
			Scratch.Clear();

			for (int i = 0; i < segments; i++)
			{
				float a = (i / (float)segments) * Mathf.PI * 2f;

				Scratch.Add(centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius);
			}

			AddConvexPolygon(vh, Scratch, color);
		}

		/// <summary>Axis-aligned or rotated rounded rectangle.</summary>
		protected void AddRoundedRect(
			VertexHelper vh,
			Vector2 centre,
			Vector2 size,
			float radius,
			float angleDegrees,
			Color32 color,
			int cornerSegments = 5)
		{
			Vector2 half = size * 0.5f;

			radius = Mathf.Clamp(radius, 0f, Mathf.Min(half.x, half.y));

			Scratch.Clear();

			// Corner centres, counter-clockwise from bottom-right.
			Vector2[] corners =
			{
				new Vector2(half.x - radius, -half.y + radius),
				new Vector2(half.x - radius,  half.y - radius),
				new Vector2(-half.x + radius,  half.y - radius),
				new Vector2(-half.x + radius, -half.y + radius)
			};

			for (int c = 0; c < 4; c++)
			{
				float start = -Mathf.PI * 0.5f + c * Mathf.PI * 0.5f;

				for (int s = 0; s <= cornerSegments; s++)
				{
					float a = start + (s / (float)cornerSegments) * Mathf.PI * 0.5f;

					Scratch.Add(corners[c] + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius);
				}
			}

			if (!Mathf.Approximately(angleDegrees, 0f))
			{
				float rad = angleDegrees * Mathf.Deg2Rad;
				float cos = Mathf.Cos(rad);
				float sin = Mathf.Sin(rad);

				for (int i = 0; i < Scratch.Count; i++)
				{
					Vector2 p = Scratch[i];

					Scratch[i] = new Vector2(p.x * cos - p.y * sin, p.x * sin + p.y * cos);
				}
			}

			for (int i = 0; i < Scratch.Count; i++)
				Scratch[i] += centre;

			AddConvexPolygon(vh, Scratch, color);
		}

		protected void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color32 color)
		{
			Scratch.Clear();
			Scratch.Add(a);
			Scratch.Add(b);
			Scratch.Add(c);

			AddConvexPolygon(vh, Scratch, color);
		}

		// =========================================
		// ARCS (not convex - no skirt)
		// =========================================

		/// <summary>Ring segment from startDegrees sweeping counter-clockwise.</summary>
		protected void AddArc(
			VertexHelper vh,
			Vector2 centre,
			float outerRadius,
			float innerRadius,
			float startDegrees,
			float sweepDegrees,
			Color32 color,
			int segments = 24)
		{
			if (segments < 1)
				return;

			int first = vh.currentVertCount;

			for (int i = 0; i <= segments; i++)
			{
				float a = (startDegrees + sweepDegrees * (i / (float)segments)) * Mathf.Deg2Rad;

				Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

				vh.AddVert(centre + dir * outerRadius, color, Vector2.zero);
				vh.AddVert(centre + dir * innerRadius, color, Vector2.zero);
			}

			for (int i = 0; i < segments; i++)
			{
				int o0 = first + i * 2;
				int i0 = o0 + 1;
				int o1 = o0 + 2;
				int i1 = o0 + 3;

				vh.AddTriangle(o0, o1, i1);
				vh.AddTriangle(o0, i1, i0);
			}
		}

		protected void AddRing(
			VertexHelper vh,
			Vector2 centre,
			float outerRadius,
			float innerRadius,
			Color32 color,
			int segments = 36)
		{
			AddArc(vh, centre, outerRadius, innerRadius, 0f, 360f, color, segments);
		}

		/// <summary>Re-run OnPopulateMesh on the next frame. Used by the animated icons.</summary>
		protected void Redraw() => SetVerticesDirty();
	}
}