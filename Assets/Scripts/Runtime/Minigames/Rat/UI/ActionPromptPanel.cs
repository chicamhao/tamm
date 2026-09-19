using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Minigames.Rat
{
	// Cooking-Mama-style action prompts (GDD sections 17, 18, 26-28).
	//
	// Builds everything at runtime under the existing Canvas: a step-chip row across
	// the top showing THROW -> TAP -> CATCH, and a large prompt icon that can either
	// sit above the HUD or ride a world object, so the hand badge lands on the thing
	// the player must actually touch.
	//
	// EVERY position, size and colour below is a serialised field, reapplied every
	// LateUpdate. Select this object in the Hierarchy DURING Play and drag the numbers
	// to move the layout live. To make the values stick after you leave Play mode, add
	// this component to a GameObject under the Canvas - CreateIn will reuse it instead
	// of building a throwaway one.
	//
	// GameManager tells this which step is live. It holds no game rules.
	// Ported 1:1 from rice/rat.
	public sealed class ActionPromptPanel : MonoBehaviour
	{
		public enum Step
		{
			Throw = 0,
			Tap = 1,
			Catch = 2
		}

		private const int StepCount = 3;

		// =========================================
		// LAYOUT - all tunable, all applied live
		// =========================================
		//
		// Units are canvas units from the anchor named in each header. The canvas is
		// 800x600 reference with Expand match, so at 16:9 it works out about 1066x600
		// with (0,0) at the centre of the screen. Negative Y is down.

		[Header("Main prompt (anchored to screen centre)")]
		[Tooltip("Where the prompt block sits when it is NOT riding a world object.")]
		public Vector2 promptRestingPosition = new Vector2(0f, -185f);

		[Tooltip("Size of the big arrow / hand icon.")]
		public Vector2 promptIconSize = new Vector2(118f, 118f);

		[Tooltip("Icon offset inside the prompt block.")]
		public Vector2 promptIconOffset = new Vector2(0f, 30f);

		[Tooltip("Offset of the SWIPE UP / TAP n label.")]
		public Vector2 promptLabelOffset = new Vector2(0f, -52f);

		[Tooltip("Offset of the 0 / 3 counter.")]
		public Vector2 promptCountOffset = new Vector2(0f, -84f);

		public float promptLabelFontSize = 30f;
		public float promptCountFontSize = 24f;

		[Header("Sweep hint")]
		[Tooltip("Shown beside the hand on rounds where dragging is unlocked.")]
		public Vector2 sweepHintOffset = new Vector2(84f, 30f);
		public Vector2 sweepHintSize = new Vector2(62f, 62f);

		[Header("Hand badge while riding the ball")]
		[Tooltip("Nudge applied on top of the ball's screen position during CATCH.")]
		public Vector2 worldBadgeOffset = new Vector2(0f, 0f);

		[Tooltip("Icon size while the badge is pinned to a world object.")]
		public Vector2 worldBadgeIconSize = new Vector2(96f, 96f);

		[Tooltip("Hide the label and counter while the badge rides the ball.")]
		public bool hideLabelWhileRiding = false;

		[Header("Step chips (anchored to top centre)")]
		public Vector2 chipRowPosition = new Vector2(0f, -52f);
		public float chipSpacing = 108f;
		public float chipDiameter = 62f;
		public float chipGlyphSize = 36f;
		public float chipCaptionY = -34f;
		public float chipYOffset = 6f;
		public float chipActiveScale = 1.18f;
		public float chipIdleScale = 0.92f;
		public float chipCaptionFontSize = 15f;
		public bool showChips = true;

		[Header("HUD texts (GDD 27)")]
		[Tooltip("Offsets from the TOP-LEFT corner.")]
		public Vector2 roundTextPosition = new Vector2(18f, -14f);
		public Vector2 heartsTextPosition = new Vector2(18f, -60f);

		[Tooltip("Offsets from the BOTTOM-CENTRE.")]
		public Vector2 instructionTextPosition = new Vector2(0f, 96f);
		public Vector2 progressTextPosition = new Vector2(0f, 56f);

		[Tooltip("Offset from the SCREEN CENTRE, for the round intro / MISS / SUCCESS banner.")]
		public Vector2 resultTextPosition = new Vector2(0f, 60f);

		[Header("Colours")]
		public Color ink = new Color(0.06f, 0.24f, 0.62f, 1f);
		public Color fill = new Color(0.62f, 0.85f, 1f, 1f);
		public Color chipIdleColor = new Color(0.78f, 0.84f, 0.90f, 1f);
		public Color chipDoneColor = new Color(0.55f, 0.80f, 0.58f, 1f);

		// =========================================

		private Canvas canvas;
		private RectTransform canvasRect;

		private RectTransform promptRoot;
		private RectTransform chipRow;
		private ArrowIcon arrow;
		private ArrowIcon sweepHint;
		private TapIcon hand;
		private TMP_Text promptLabel;
		private TMP_Text promptCount;

		private readonly CircleIcon[] chipBacks = new CircleIcon[StepCount];
		private readonly PromptIcon[] chipGlyphs = new PromptIcon[StepCount];
		private readonly TMP_Text[] chipLabels = new TMP_Text[StepCount];

		private TMP_Text roundText;
		private TMP_Text instructionText;
		private TMP_Text progressText;
		private TMP_Text heartsText;
		private TMP_Text resultText;

		private Transform worldTarget;
		private Step activeStep;
		private int activeRequired;

		// Set per round from the round rules. The wording is data, not code.
		private string promptVerb = "TAP";
		private bool sweepEnabled;
		private bool built;

		// =========================================
		// CONSTRUCTION
		// =========================================

		public static ActionPromptPanel CreateIn(Canvas canvas)
		{
			if (canvas == null)
			{
				Debug.LogError("ActionPromptPanel: no Canvas in the scene.");
				return null;
			}

			// The serialised Canvas scale in this scene is {0,0,0}, which is degenerate.
			canvas.transform.localScale = Vector3.one;

			// Reuse a panel placed in the scene by hand, so its Inspector values survive
			// leaving Play mode. Otherwise build a throwaway one at runtime.
			ActionPromptPanel existing = canvas.GetComponentInChildren<ActionPromptPanel>(true);

			if (existing != null)
			{
				existing.canvas = canvas;
				existing.canvasRect = (RectTransform)canvas.transform;
				existing.Build();

				return existing;
			}

			GameObject go = new GameObject("ActionPrompts", typeof(RectTransform));

			go.layer = LayerMask.NameToLayer("UI");
			go.transform.SetParent(canvas.transform, false);

			ActionPromptPanel panel = go.AddComponent<ActionPromptPanel>();

			panel.canvas = canvas;
			panel.canvasRect = (RectTransform)canvas.transform;
			panel.Build();

			return panel;
		}

		private void Build()
		{
			if (built)
				return;

			built = true;

			// Fill the canvas, whether this object was created here or found in the scene.
			RectTransform self = (RectTransform)transform;

			self.anchorMin = Vector2.zero;
			self.anchorMax = Vector2.one;
			self.offsetMin = Vector2.zero;
			self.offsetMax = Vector2.zero;
			self.localScale = Vector3.one;

			BuildPrompt();
			BuildChips();

			ApplyLayout();
			Hide();
		}

		private void BuildPrompt()
		{
			promptRoot = MakeRect("Prompt", transform, new Vector2(0.5f, 0.5f), new Vector2(240f, 210f));

			arrow = MakeIcon<ArrowIcon>("Arrow", promptRoot);
			arrow.direction = ArrowIcon.Dir.Up;

			hand = MakeIcon<TapIcon>("Hand", promptRoot);

			sweepHint = MakeIcon<ArrowIcon>("SweepHint", promptRoot);
			sweepHint.direction = ArrowIcon.Dir.Right;

			promptLabel = MakeText("Label", promptRoot, promptLabelFontSize, TextAlignmentOptions.Center);
			promptCount = MakeText("Count", promptRoot, promptCountFontSize, TextAlignmentOptions.Center);
		}

		private void BuildChips()
		{
			chipRow = MakeRect("StepChips", transform, new Vector2(0.5f, 1f), new Vector2(360f, 96f));

			for (int i = 0; i < StepCount; i++)
			{
				CircleIcon back = MakeIcon<CircleIcon>("Chip" + i, chipRow);
				back.rimWidth = 4f;

				chipBacks[i] = back;

				RectTransform backRect = (RectTransform)back.transform;

				if (i == (int)Step.Throw)
				{
					ArrowIcon glyph = MakeIcon<ArrowIcon>("Glyph", backRect);
					glyph.direction = ArrowIcon.Dir.Up;
					glyph.animate = false;
					glyph.outlineWidth = 2.5f;

					chipGlyphs[i] = glyph;
				}
				else
				{
					TapIcon glyph = MakeIcon<TapIcon>("Glyph", backRect);
					glyph.ripple = false;
					glyph.outlineWidth = 2.5f;

					chipGlyphs[i] = glyph;
				}

				chipLabels[i] = MakeText("Caption" + i, chipRow, chipCaptionFontSize, TextAlignmentOptions.Center);
				chipLabels[i].text = CaptionFor((Step)i, 0);
			}

			SetActiveStep(Step.Throw, 0);
		}

		// =========================================
		// LAYOUT - reapplied every frame so Inspector edits show live
		// =========================================

		private void LateUpdate()
		{
			if (!built)
				return;

			ApplyLayout();
			UpdatePromptPosition();
		}

		private void ApplyLayout()
		{
			bool riding = worldTarget != null;

			Vector2 iconSize = riding ? worldBadgeIconSize : promptIconSize;

			SetRect((RectTransform)arrow.transform, iconSize, promptIconOffset);
			SetRect((RectTransform)hand.transform, iconSize, promptIconOffset);
			SetRect((RectTransform)sweepHint.transform, sweepHintSize, sweepHintOffset);

			SetRect(promptLabel.rectTransform, new Vector2(240f, 40f), promptLabelOffset);
			SetRect(promptCount.rectTransform, new Vector2(240f, 34f), promptCountOffset);

			promptLabel.fontSize = promptLabelFontSize;
			promptCount.fontSize = promptCountFontSize;

			bool textVisible = !(riding && hideLabelWhileRiding);

			promptLabel.enabled = textVisible;
			promptCount.enabled = textVisible;

			chipRow.gameObject.SetActive(showChips);
			chipRow.anchoredPosition = chipRowPosition;

			for (int i = 0; i < StepCount; i++)
			{
				float x = (i - 1) * chipSpacing;

				SetRect(
					(RectTransform)chipBacks[i].transform,
					new Vector2(chipDiameter, chipDiameter),
					new Vector2(x, chipYOffset)
				);

				SetRect(
					(RectTransform)chipGlyphs[i].transform,
					new Vector2(chipGlyphSize, chipGlyphSize),
					Vector2.zero
				);

				SetRect(
					chipLabels[i].rectTransform,
					new Vector2(chipSpacing - 4f, 22f),
					new Vector2(x, chipCaptionY)
				);

				chipLabels[i].fontSize = chipCaptionFontSize;
			}

			ApplyStepAppearance();
			ApplyHudTextPositions();
		}

		private void UpdatePromptPosition()
		{
			if (promptRoot == null || !promptRoot.gameObject.activeSelf)
				return;

			if (worldTarget == null)
			{
				promptRoot.anchoredPosition = promptRestingPosition;
				return;
			}

			Camera cam = Camera.main;

			if (cam == null || canvasRect == null)
				return;

			Vector3 screen = cam.WorldToScreenPoint(worldTarget.position);

			if (screen.z < 0f)
				return;

			Camera uiCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
				? null
				: (canvas != null ? canvas.worldCamera : null);

			Vector2 local;

			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
					canvasRect, screen, uiCamera, out local))
			{
				promptRoot.anchoredPosition = local + worldBadgeOffset;
			}
		}

		// =========================================
		// PUBLIC API
		// =========================================

		public void ShowThrow()
		{
			worldTarget = null;

			promptRoot.gameObject.SetActive(true);

			arrow.gameObject.SetActive(true);
			hand.gameObject.SetActive(false);
			sweepHint.gameObject.SetActive(false);

			promptLabel.text = "SWIPE UP";
			promptCount.text = "";

			SetActiveStep(Step.Throw, 0);
		}

		/// <summary>
		/// Sets the wording and whether the sweep affordance is shown. Called once per
		/// round from GameManager, straight out of the round rules.
		/// </summary>
		public void SetRoundStyle(string verb, bool allowSweep)
		{
			promptVerb = string.IsNullOrEmpty(verb) ? "TAP" : verb;
			sweepEnabled = allowSweep;
		}

		/// <summary>Short form for the chip caption, which is too narrow for "TAP OR SWEEP".</summary>
		private string ShortVerb()
		{
			int space = promptVerb.IndexOf(' ');

			return space > 0 ? promptVerb.Substring(0, space) : promptVerb;
		}

		public void ShowTap(int collected, int required)
		{
			worldTarget = null;

			promptRoot.gameObject.SetActive(true);

			arrow.gameObject.SetActive(false);
			hand.gameObject.SetActive(true);
			sweepHint.gameObject.SetActive(sweepEnabled);

			promptLabel.text = promptVerb + " " + required;
			promptCount.text = collected + " / " + required;

			SetActiveStep(Step.Tap, required);
		}

		/// <summary>Pins the hand badge to a world object so it rides the ball as it falls.</summary>
		public void ShowCatch(Transform target)
		{
			worldTarget = target;

			promptRoot.gameObject.SetActive(true);

			arrow.gameObject.SetActive(false);
			hand.gameObject.SetActive(true);
			sweepHint.gameObject.SetActive(false);

			promptLabel.text = "CATCH";
			promptCount.text = "";

			SetActiveStep(Step.Catch, 0);
		}

		public void Hide()
		{
			worldTarget = null;

			if (promptRoot != null)
				promptRoot.gameObject.SetActive(false);
		}

		// =========================================
		// CHIPS
		// =========================================

		private void SetActiveStep(Step active, int required)
		{
			activeStep = active;
			activeRequired = required;

			for (int i = 0; i < StepCount; i++)
			{
				if (chipLabels[i] != null)
					chipLabels[i].text = CaptionFor((Step)i, activeRequired);
			}

			ApplyStepAppearance();
		}

		private void ApplyStepAppearance()
		{
			for (int i = 0; i < StepCount; i++)
			{
				bool isActive = i == (int)activeStep;
				bool isDone = i < (int)activeStep;

				float alpha = isActive ? 1f : (isDone ? 0.55f : 0.35f);

				if (chipBacks[i] != null)
				{
					chipBacks[i].color = isActive ? fill : (isDone ? chipDoneColor : chipIdleColor);
					chipBacks[i].outlineColor = ink;
					chipBacks[i].transform.localScale =
						Vector3.one * (isActive ? chipActiveScale : chipIdleScale);
				}

				if (chipGlyphs[i] != null)
				{
					Color c = ink;
					c.a = alpha;

					// Dim the outline too, otherwise a faded glyph keeps a hard dark edge.
					chipGlyphs[i].color = c;
					chipGlyphs[i].outlineColor = c;
				}

				if (chipLabels[i] != null)
					chipLabels[i].alpha = alpha;
			}

			if (arrow != null)
			{
				arrow.color = fill;
				arrow.outlineColor = ink;
			}

			if (hand != null)
			{
				hand.color = fill;
				hand.outlineColor = ink;
			}
		}

		private string CaptionFor(Step step, int required)
		{
			switch (step)
			{
				case Step.Throw: return "THROW";
				case Step.Tap: return required > 0 ? ShortVerb() + " x" + required : ShortVerb();
				default: return "CATCH";
			}
		}

		// =========================================
		// EXISTING HUD TEXTS
		// =========================================

		/// <summary>
		/// Adopts the texts already in the scene so their placement is driven by the
		/// fields above. Done from code because the scene file is Editor-owned.
		/// Any of them may be null - the prompt label/counter cover the same
		/// information as instructionText and progressText.
		/// </summary>
		public void AdoptExistingTexts(
			TMP_Text round,
			TMP_Text instruction,
			TMP_Text progress,
			TMP_Text hearts,
			TMP_Text result)
		{
			roundText = round;
			instructionText = instruction;
			progressText = progress;
			heartsText = hearts;
			resultText = result;

			Configure(roundText, 32f, TextAlignmentOptions.TopLeft);
			Configure(heartsText, 22f, TextAlignmentOptions.TopLeft);
			Configure(instructionText, 30f, TextAlignmentOptions.Center);
			Configure(progressText, 24f, TextAlignmentOptions.Center);
			Configure(resultText, 40f, TextAlignmentOptions.Center);

			if (resultText != null)
				resultText.text = "";

			ApplyHudTextPositions();
		}

		private void ApplyHudTextPositions()
		{
			Place(roundText, new Vector2(0f, 1f), new Vector2(0f, 1f), roundTextPosition, new Vector2(460f, 46f));
			Place(heartsText, new Vector2(0f, 1f), new Vector2(0f, 1f), heartsTextPosition, new Vector2(300f, 34f));
			Place(instructionText, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), instructionTextPosition, new Vector2(520f, 46f));
			Place(progressText, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), progressTextPosition, new Vector2(300f, 36f));
			Place(resultText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), resultTextPosition, new Vector2(620f, 180f));
		}

		private static void Configure(TMP_Text text, float fontSize, TextAlignmentOptions alignment)
		{
			if (text == null)
				return;

			text.gameObject.layer = LayerMask.NameToLayer("UI");
			text.fontSize = fontSize;
			text.alignment = alignment;
			text.textWrappingMode = TextWrappingModes.Normal;
		}

		private static void Place(
			TMP_Text text,
			Vector2 anchor,
			Vector2 pivot,
			Vector2 position,
			Vector2 size)
		{
			if (text == null)
				return;

			RectTransform rt = text.rectTransform;

			rt.anchorMin = anchor;
			rt.anchorMax = anchor;
			rt.pivot = pivot;
			rt.sizeDelta = size;
			rt.anchoredPosition = position;
			rt.localScale = Vector3.one;
			rt.localRotation = Quaternion.identity;
		}

		// =========================================
		// BUILD HELPERS
		// =========================================

		private static RectTransform MakeRect(string name, Transform parent, Vector2 anchor, Vector2 size)
		{
			GameObject go = new GameObject(name, typeof(RectTransform));

			go.layer = LayerMask.NameToLayer("UI");
			go.transform.SetParent(parent, false);

			RectTransform rt = (RectTransform)go.transform;

			rt.anchorMin = anchor;
			rt.anchorMax = anchor;
			rt.pivot = new Vector2(0.5f, 0.5f);
			rt.sizeDelta = size;
			rt.anchoredPosition = Vector2.zero;

			return rt;
		}

		private T MakeIcon<T>(string name, Transform parent) where T : PromptIcon
		{
			// CanvasRenderer is created up front, before the Graphic, because Graphic
			// caches its renderer on first access. Without it the icon draws nothing.
			GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));

			go.layer = LayerMask.NameToLayer("UI");
			go.transform.SetParent(parent, false);

			RectTransform rt = (RectTransform)go.transform;

			rt.anchorMin = new Vector2(0.5f, 0.5f);
			rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.pivot = new Vector2(0.5f, 0.5f);

			T icon = go.AddComponent<T>();

			icon.color = fill;
			icon.outlineColor = ink;
			icon.raycastTarget = false;

			return icon;
		}

		private static TMP_Text MakeText(string name, Transform parent, float fontSize, TextAlignmentOptions alignment)
		{
			GameObject go = new GameObject(name, typeof(RectTransform));

			go.layer = LayerMask.NameToLayer("UI");
			go.transform.SetParent(parent, false);

			RectTransform rt = (RectTransform)go.transform;

			rt.anchorMin = new Vector2(0.5f, 0.5f);
			rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.pivot = new Vector2(0.5f, 0.5f);

			TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();

			text.fontSize = fontSize;
			text.alignment = alignment;
			text.color = Color.white;
			text.raycastTarget = false;
			text.textWrappingMode = TextWrappingModes.NoWrap;

			return text;
		}

		private static void SetRect(RectTransform rt, Vector2 size, Vector2 position)
		{
			if (rt == null)
				return;

			rt.sizeDelta = size;
			rt.anchoredPosition = position;
		}
	}
}