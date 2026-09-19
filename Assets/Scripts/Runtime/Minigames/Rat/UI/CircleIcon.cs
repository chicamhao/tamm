using UnityEngine;
using UnityEngine.UI;

namespace Game.Minigames.Rat
{
	// Filled disc with an optional rim. Used as the backing for the step chips.
	// Ported 1:1 from rice/rat.
	public sealed class CircleIcon : PromptIcon
	{
		[Header("Rim")]
		public bool drawRim = true;
		public float rimWidth = 5f;

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			float radius = Size * 0.5f;

			if (drawRim && rimWidth > 0f)
				AddCircle(vh, Vector2.zero, radius, outlineColor, 32);

			AddCircle(vh, Vector2.zero, Mathf.Max(radius - rimWidth, 1f), color, 32);
		}
	}
}