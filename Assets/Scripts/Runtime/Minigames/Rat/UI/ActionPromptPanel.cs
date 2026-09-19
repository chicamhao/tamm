using Game.Minigames.Rat.UI;
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
	// RatManager tells this which step is live. It holds no game rules.
	// Ported 1:1 from rice/rat.
	public sealed class ActionPromptPanel : MonoBehaviour
	{
		public enum Step
		{
			Throw = 0,
			Tap = 1,
			Catch = 2
		}

		private const int _stepCount = 3;

		// =========================================
		// LAYOUT - all tunable, all applied live
		// =========================================
		//
		// Units are canvas units from the anchor named in each header. The canvas is
		// 800x600 reference with Expand match, so at 16:9 it works out about 1066x600
		// with (0,0) at the centre of the screen. Negative Y is down.

		[Header("Main prompt (anchored to screen centre)")]
		[Tooltip("Where the prompt block sits when it is NOT riding a world object.")]
		public Vector2 PromptRestingPosition = new Vector2(0f, -185f);

		[Tooltip("Size of the big arrow / hand icon.")]
		public Vector2 PromptIconSize = new Vector2(118f, 118f);

		[Tooltip("Icon offset inside the prompt block.")]
		public Vector2 PromptIconOffset = new Vector2(0f, 30f);

		[Tooltip("Offset of the SWIPE UP / TAP n label.")]
		public Vector2 PromptLabelOffset = new Vector2(0f, -52f);

		[Tooltip("Offset of the 0 / 3 counter.")]
		public Vector2 PromptCountOffset = new Vector2(0f, -84f);

		public float PromptLabelFontSize = 30f;
		public float PromptCountFontSize = 24f;

		[Header("Sweep hint")]
		[Tooltip("Shown beside the hand on rounds where dragging is unlocked.")]
		public Vector2 SweepHintOffset = new Vector2(84f, 30f);
		public Vector2 SweepHintSize = new Vector2(62f, 62f);

		[Header("Hand badge while riding the ball")]
		[Tooltip("Nudge applied on top of the ball's screen position during CATCH.")]
		public Vector2 WorldBadgeOffset = new Vector2(0f, 0f);

		[Tooltip("Icon size while the badge is pinned to a world object.")]
		public Vector2 WorldBadgeIconSize = new Vector2(96f, 96f);

		[Tooltip("Hide the label and counter while the badge rides the ball.")]
		public bool HideLabelWhileRiding = false;

		[Header("Step chips (anchored to top centre)")]
		public Vector2 ChipRowPosition = new Vector2(0f, -52f);
		public float ChipSpacing = 108f;
		public float ChipDiameter = 62f;
		public float ChipGlyphSize = 36f;
		public float ChipCaptionY = -34f;
		public float ChipYOffset = 6f;
		public float ChipActiveScale = 1.18f;
		public float ChipIdleScale = 0.92f;
		public float ChipCaptionFontSize = 15f;
		public bool ShowChips = true;

		[Header("HUD texts (GDD 27)")]
		[Tooltip("Offsets from the TOP-LEFT corner.")]
		public Vector2 RoundTextPosition = new Vector2(18f, -14f);
		public Vector2 HeartsTextPosition = new Vector2(18f, -60f);

		[Tooltip("Offsets from the BOTTOM-CENTRE.")]
		public Vector2 InstructionTextPosition = new Vector2(0f, 96f);
		public Vector2 ProgressTextPosition = new Vector2(0f, 56f);

		[Tooltip("Offset from the SCREEN CENTRE, for the round intro / MISS / SUCCESS banner.")]
		public Vector2 ResultTextPosition = new Vector2(0f, 60f);

		[Header("Colours")]
		public Color Ink = new Color(0.06f, 0.24f, 0.62f, 1f);
		public Color Fill = new Color(0.62f, 0.85f, 1f, 1f);
		public Color ChipIdleColor = new Color(0.78f, 0.84f, 0.90f, 1f);
		public Color ChipDoneColor = new Color(0.55f, 0.80f, 0.58f, 1f);

		// =========================================

		private Canvas _canvas;
		private RectTransform _canvasRect;

		private RectTransform _promptRoot;
		private RectTransform _chipRow;
		private ArrowIcon _arrow;
		private ArrowIcon _sweepHint;
		private TapIcon _hand;
		private TMP_Text _promptLabel;
		private TMP_Text _promptCount;

		private readonly CircleIcon[] _chipBacks = new CircleIcon[_stepCount];
		private readonly PromptIcon[] _chipGlyphs = new PromptIcon[_stepCount];
		private readonly TMP_Text[] _chipLabels = new TMP_Text[_stepCount];

		private TMP_Text _roundText;
		private TMP_Text _instructionText;
		private TMP_Text _progressText;
		private TMP_Text _heartsText;
		private TMP_Text _resultText;

		private Transform _worldTarget;
		private Step _activeStep;
		private int _activeRequired;

		// Set per round from the round rules. The wording is data, not code.
		private string _promptVerb = "TAP";
		private bool _sweepEnabled;
		private bool _built;

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
				existing._canvas = canvas;
				existing._canvasRect = (RectTransform)canvas.transform;
				existing.Build();

				return existing;
			}

			GameObject go = new GameObject("ActionPrompts", typeof(RectTransform));

			go.layer = LayerMask.NameToLayer("UI");
			go.transform.SetParent(canvas.transform, false);

			ActionPromptPanel panel = go.AddComponent<ActionPromptPanel>();

			panel._canvas = canvas;
			panel._canvasRect = (RectTransform)canvas.transform;
			panel.Build();

			return panel;
		}

		private void Build()
		{
			if (_built)
				return;

			_built = true;

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
			_promptRoot = MakeRect("Prompt", transform, new Vector2(0.5f, 0.5f), new Vector2(240f, 210f));

			_arrow = MakeIcon<ArrowIcon>("Arrow", _promptRoot);
			_arrow.Direction = ArrowIcon.Dir.Up;

			_hand = MakeIcon<TapIcon>("Hand", _promptRoot);

			_sweepHint = MakeIcon<ArrowIcon>("SweepHint", _promptRoot);
			_sweepHint.Direction = ArrowIcon.Dir.Right;

			_promptLabel = MakeText("Label", _promptRoot, PromptLabelFontSize, TextAlignmentOptions.Center);
			_promptCount = MakeText("Count", _promptRoot, PromptCountFontSize, TextAlignmentOptions.Center);
		}

		private void BuildChips()
		{
			_chipRow = MakeRect("StepChips", transform, new Vector2(0.5f, 1f), new Vector2(360f, 96f));

			for (int i = 0; i < _stepCount; i++)
			{
				CircleIcon back = MakeIcon<CircleIcon>("Chip" + i, _chipRow);
				back.RimWidth = 4f;

				_chipBacks[i] = back;

				RectTransform backRect = (RectTransform)back.transform;

				if (i == (int)Step.Throw)
				{
					ArrowIcon glyph = MakeIcon<ArrowIcon>("Glyph", backRect);
					glyph.Direction = ArrowIcon.Dir.Up;
					glyph.Animate = false;
					glyph.OutlineWidth = 2.5f;

					_chipGlyphs[i] = glyph;
				}
				else
				{
					TapIcon glyph = MakeIcon<TapIcon>("Glyph", backRect);
					glyph.Ripple = false;
					glyph.OutlineWidth = 2.5f;

					_chipGlyphs[i] = glyph;
				}

				_chipLabels[i] = MakeText("Caption" + i, _chipRow, ChipCaptionFontSize, TextAlignmentOptions.Center);
				_chipLabels[i].text = CaptionFor((Step)i, 0);
			}

			SetActiveStep(Step.Throw, 0);
		}

		// =========================================
		// LAYOUT - reapplied every frame so Inspector edits show live
		// =========================================

		private void LateUpdate()
		{
			if (!_built)
				return;

			ApplyLayout();
			UpdatePromptPosition();
		}

		private void ApplyLayout()
		{
			bool riding = _worldTarget != null;

			Vector2 iconSize = riding ? WorldBadgeIconSize : PromptIconSize;

			SetRect((RectTransform)_arrow.transform, iconSize, PromptIconOffset);
			SetRect((RectTransform)_hand.transform, iconSize, PromptIconOffset);
			SetRect((RectTransform)_sweepHint.transform, SweepHintSize, SweepHintOffset);

			SetRect(_promptLabel.rectTransform, new Vector2(240f, 40f), PromptLabelOffset);
			SetRect(_promptCount.rectTransform, new Vector2(240f, 34f), PromptCountOffset);

			_promptLabel.fontSize = PromptLabelFontSize;
			_promptCount.fontSize = PromptCountFontSize;

			bool textVisible = !(riding && HideLabelWhileRiding);

			_promptLabel.enabled = textVisible;
			_promptCount.enabled = textVisible;

			_chipRow.gameObject.SetActive(ShowChips);
			_chipRow.anchoredPosition = ChipRowPosition;

			for (int i = 0; i < _stepCount; i++)
			{
				float x = (i - 1) * ChipSpacing;

				SetRect(
					(RectTransform)_chipBacks[i].transform,
					new Vector2(ChipDiameter, ChipDiameter),
					new Vector2(x, ChipYOffset)
				);

				SetRect(
					(RectTransform)_chipGlyphs[i].transform,
					new Vector2(ChipGlyphSize, ChipGlyphSize),
					Vector2.zero
				);

				SetRect(
					_chipLabels[i].rectTransform,
					new Vector2(ChipSpacing - 4f, 22f),
					new Vector2(x, ChipCaptionY)
				);

				_chipLabels[i].fontSize = ChipCaptionFontSize;
			}

			ApplyStepAppearance();
			ApplyHudTextPositions();
		}

		private void UpdatePromptPosition()
		{
			if (_promptRoot == null || !_promptRoot.gameObject.activeSelf)
				return;

			if (_worldTarget == null)
			{
				_promptRoot.anchoredPosition = PromptRestingPosition;
				return;
			}

			Camera cam = Camera.main;

			if (cam == null || _canvasRect == null)
				return;

			Vector3 screen = cam.WorldToScreenPoint(_worldTarget.position);

			if (screen.z < 0f)
				return;

			Camera uiCamera = _canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay
				? null
				: (_canvas != null ? _canvas.worldCamera : null);

			Vector2 local;

			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
					_canvasRect, screen, uiCamera, out local))
			{
				_promptRoot.anchoredPosition = local + WorldBadgeOffset;
			}
		}

		// =========================================
		// PUBLIC API
		// =========================================

		public void ShowThrow()
		{
			_worldTarget = null;

			_promptRoot.gameObject.SetActive(true);

			_arrow.gameObject.SetActive(true);
			_hand.gameObject.SetActive(false);
			_sweepHint.gameObject.SetActive(false);

			_promptLabel.text = "SWIPE UP";
			_promptCount.text = "";

			SetActiveStep(Step.Throw, 0);
		}

		/// <summary>
		/// Sets the wording and whether the sweep affordance is shown. Called once per
		/// round from RatManager, straight out of the round rules.
		/// </summary>
		public void SetRoundStyle(string verb, bool allowSweep)
		{
			_promptVerb = string.IsNullOrEmpty(verb) ? "TAP" : verb;
			_sweepEnabled = allowSweep;
		}

		/// <summary>Short form for the chip caption, which is too narrow for "TAP OR SWEEP".</summary>
		private string ShortVerb()
		{
			int space = _promptVerb.IndexOf(' ');

			return space > 0 ? _promptVerb.Substring(0, space) : _promptVerb;
		}

		public void ShowTap(int collected, int required)
		{
			_worldTarget = null;

			_promptRoot.gameObject.SetActive(true);

			_arrow.gameObject.SetActive(false);
			_hand.gameObject.SetActive(true);
			_sweepHint.gameObject.SetActive(_sweepEnabled);

			_promptLabel.text = _promptVerb + " " + required;
			_promptCount.text = collected + " / " + required;

			SetActiveStep(Step.Tap, required);
		}

		/// <summary>Pins the hand badge to a world object so it rides the ball as it falls.</summary>
		public void ShowCatch(Transform target)
		{
			_worldTarget = target;

			_promptRoot.gameObject.SetActive(true);

			_arrow.gameObject.SetActive(false);
			_hand.gameObject.SetActive(true);
			_sweepHint.gameObject.SetActive(false);

			_promptLabel.text = "CATCH";
			_promptCount.text = "";

			SetActiveStep(Step.Catch, 0);
		}

		public void Hide()
		{
			_worldTarget = null;

			if (_promptRoot != null)
				_promptRoot.gameObject.SetActive(false);
		}

		// =========================================
		// CHIPS
		// =========================================

		private void SetActiveStep(Step active, int required)
		{
			_activeStep = active;
			_activeRequired = required;

			for (int i = 0; i < _stepCount; i++)
			{
				if (_chipLabels[i] != null)
					_chipLabels[i].text = CaptionFor((Step)i, _activeRequired);
			}

			ApplyStepAppearance();
		}

		private void ApplyStepAppearance()
		{
			for (int i = 0; i < _stepCount; i++)
			{
				bool isActive = i == (int)_activeStep;
				bool isDone = i < (int)_activeStep;

				float alpha = isActive ? 1f : (isDone ? 0.55f : 0.35f);

				if (_chipBacks[i] != null)
				{
					_chipBacks[i].color = isActive ? Fill : (isDone ? ChipDoneColor : ChipIdleColor);
					_chipBacks[i].OutlineColor = Ink;
					_chipBacks[i].transform.localScale =
						Vector3.one * (isActive ? ChipActiveScale : ChipIdleScale);
				}

				if (_chipGlyphs[i] != null)
				{
					Color c = Ink;
					c.a = alpha;

					// Dim the outline too, otherwise a faded glyph keeps a hard dark edge.
					_chipGlyphs[i].color = c;
					_chipGlyphs[i].OutlineColor = c;
				}

				if (_chipLabels[i] != null)
					_chipLabels[i].alpha = alpha;
			}

			if (_arrow != null)
			{
				_arrow.color = Fill;
				_arrow.OutlineColor = Ink;
			}

			if (_hand != null)
			{
				_hand.color = Fill;
				_hand.OutlineColor = Ink;
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
			_roundText = round;
			_instructionText = instruction;
			_progressText = progress;
			_heartsText = hearts;
			_resultText = result;

			Configure(_roundText, 32f, TextAlignmentOptions.TopLeft);
			Configure(_heartsText, 22f, TextAlignmentOptions.TopLeft);
			Configure(_instructionText, 30f, TextAlignmentOptions.Center);
			Configure(_progressText, 24f, TextAlignmentOptions.Center);
			Configure(_resultText, 40f, TextAlignmentOptions.Center);

			if (_resultText != null)
				_resultText.text = "";

			ApplyHudTextPositions();
		}

		private void ApplyHudTextPositions()
		{
			Place(_roundText, new Vector2(0f, 1f), new Vector2(0f, 1f), RoundTextPosition, new Vector2(460f, 46f));
			Place(_heartsText, new Vector2(0f, 1f), new Vector2(0f, 1f), HeartsTextPosition, new Vector2(300f, 34f));
			Place(_instructionText, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), InstructionTextPosition, new Vector2(520f, 46f));
			Place(_progressText, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), ProgressTextPosition, new Vector2(300f, 36f));
			Place(_resultText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), ResultTextPosition, new Vector2(620f, 180f));
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

			icon.color = Fill;
			icon.OutlineColor = Ink;
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