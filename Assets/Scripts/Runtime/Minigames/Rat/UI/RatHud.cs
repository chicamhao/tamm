using Game.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Game.Minigames.Rat.UI
{
	// Cooking-Mama-style action prompts (GDD sections 17, 18, 26-28) rendered with
	// UI Toolkit primitives: a step-chip row across the top showing THROW -> TAP ->
	// CATCH, and a large prompt icon that can either sit above the HUD or ride a
	// world object, so the hand badge lands on the thing the player must touch.
	//
	// Attach this next to the level's RuntimeUI. It reads the RatHud subtree of the
	// runtime panel and holds no game rules. RatManager drives the steps.
	public sealed class RatHud : MonoBehaviour
	{
		public enum Step
		{
			Throw = 0,
			Tap = 1,
			Catch = 2
		}

		public static readonly int StepCount = 3;

		[Header("World badge")]
		[Tooltip("Hide the label and counter while the badge rides the ball.")]
		public bool HideLabelWhileRiding = false;

		[Header("Chips")]
		public bool ShowChips = true;

		[Header("Geek tuning — riding offset")]
		[Tooltip("Nudge applied on top of the ball's screen position during CATCH.")]
		public Vector2 WorldBadgeOffset = new Vector2(0f, 0f);

		[SerializeField] private RuntimeUI _runtimeUI;

		private RuntimeUI _ui;
		private VisualElement _hud;
		private VisualElement _promptRoot;
		private VisualElement _chipRow;
		private VisualElement[] _chips = new VisualElement[StepCount];
		private Label[] _chipLabels = new Label[StepCount];
		private Label _arrow;
		private VisualElement _hand;
		private VisualElement _ripple;
		private Label _sweepHint;
		private Label _promptLabel;
		private Label _promptCount;
		private Label _roundText;
		private Label _heartsText;
		private Label _instructionText;
		private Label _progressText;
		private Label _resultText;

		private Transform _worldTarget;
		private Step _activeStep;
		private int _activeRequired;

		// Set per round from the round rules. The wording is data, not code.
		private string _promptVerb = "TAP";
		private bool _sweepEnabled;
		private bool _built;

		private void Start()
		{
			_ui = RuntimeUI.Resolve(_runtimeUI);
			Assert.IsNotNull(_ui, "RatHud requires a RuntimeUI in the scene");

			_hud = _ui.Q("RatHud");
			Assert.IsNotNull(_hud, "RatHud requires a RatHud element in the runtime UI");

			_promptRoot = _ui.Q("PromptRoot");
			_arrow = _ui.Q<Label>("ArrowIcon");
			_hand = _ui.Q("HandIcon");
			_ripple = _ui.Q("Ripple");
			_sweepHint = _ui.Q<Label>("SweepHint");
			_promptLabel = _ui.Q<Label>("PromptLabel");
			_promptCount = _ui.Q<Label>("PromptCount");
			_chipRow = _ui.Q("ChipRow");
			_roundText = _ui.Q<Label>("RoundText");
			_heartsText = _ui.Q<Label>("HeartsText");
			_instructionText = _ui.Q<Label>("InstructionText");
			_progressText = _ui.Q<Label>("ProgressText");
			_resultText = _ui.Q<Label>("ResultText");

			for (int i = 0; i < StepCount; i++)
			{
				_chips[i] = _ui.Q("Chip" + i);
				_chipLabels[i] = _ui.Q<Label>("ChipCaption" + i);
			}

			_built = true;

			_chipRow.style.display = ShowChips ? DisplayStyle.Flex : DisplayStyle.None;
			if (_resultText != null) _resultText.text = "";

			Hide();
			SetActiveStep(Step.Throw, 0);
		}

		// =========================================
		// PUBLIC API (RatManager drives these)
		// =========================================

		public void ShowThrow()
		{
			_worldTarget = null;

			_promptRoot.style.display = DisplayStyle.Flex;
			ShowIcon(true, false, false);

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

			_promptRoot.style.display = DisplayStyle.Flex;
			ShowIcon(false, true, _sweepEnabled);

			_promptLabel.text = _promptVerb + " " + required;
			_promptCount.text = collected + " / " + required;

			SetActiveStep(Step.Tap, required);
		}

		/// <summary>Pins the hand badge to a world object so it rides the ball as it falls.</summary>
		public void ShowCatch(Transform target)
		{
			_worldTarget = target;

			_promptRoot.style.display = DisplayStyle.Flex;
			ShowIcon(false, true, false);

			_promptLabel.text = "CATCH";
			_promptCount.text = "";

			SetActiveStep(Step.Catch, 0);
		}

		public void Hide()
		{
			_worldTarget = null;

			if (_promptRoot != null)
				_promptRoot.style.display = DisplayStyle.None;
		}

		// =========================================
		// HUD TEXTS (the old scene TMP texts were owned by the level; RatManager writes them)
		// =========================================

		public void SetRoundText(string text)
		{
			if (_roundText != null) _roundText.text = text;
		}

		public void SetHeartsText(string text)
		{
			if (_heartsText != null) _heartsText.text = text;
		}

		public void SetResultText(string text)
		{
			if (_resultText != null) _resultText.text = text;
		}

		// =========================================
		// PRESENTATION
		// =========================================

		private void ShowIcon(bool arrow, bool hand, bool sweep)
		{
			_arrow.style.display = arrow ? DisplayStyle.Flex : DisplayStyle.None;
			_hand.style.display = hand ? DisplayStyle.Flex : DisplayStyle.None;
			_ripple.style.display = hand ? DisplayStyle.Flex : DisplayStyle.None;
			_sweepHint.style.display = sweep ? DisplayStyle.Flex : DisplayStyle.None;

			bool textVisible = !(_worldTarget != null && HideLabelWhileRiding);
			_promptLabel.style.display = textVisible ? DisplayStyle.Flex : DisplayStyle.None;
			_promptCount.style.display = textVisible ? DisplayStyle.Flex : DisplayStyle.None;
		}

		private void LateUpdate()
		{
			if (!_built)
				return;

			PlacePrompt();
			UpdateRipple();
		}

		/// <summary>
		/// Resting: prompt block sits centre-upper of the panel. Riding (CATCH): the
		/// block follows the ball's screen projection, nudged by WorldBadgeOffset.
		/// All in the panel's logical (reference) space, so scaling is a no-op.
		/// </summary>
		private void PlacePrompt()
		{
			if (_promptRoot == null || _promptRoot.style.display == DisplayStyle.None)
				return;

			Rect content = _ui.Root.contentRect;
			float logicalH = content.height > 0f ? content.height : 900f;
			float logicalW = content.width > 0f ? content.width : 1600f;

			if (_worldTarget == null)
			{
				_promptRoot.style.translate = new Translate(logicalW * 0.5f - 130f, 180f);
				return;
			}

			Camera cam = Camera.main;

			if (cam == null)
				return;

			Vector3 screen = cam.WorldToScreenPoint(_worldTarget.position);

			if (screen.z < 0f)
				return;

			_promptRoot.style.translate = new Translate(
				screen.x + WorldBadgeOffset.x,
				logicalH - screen.y + WorldBadgeOffset.y);
		}

		/// <summary>Expanding, fading ring around the tap disc. Uses unscaled time so it keeps running while paused.</summary>
		private void UpdateRipple()
		{
			if (_ripple == null || _ripple.style.display == DisplayStyle.None)
				return;

			float period = 1.2f;
			float k = (Time.unscaledTime % period) / period;

			float size = Mathf.Lerp(150f, 250f, k);

			_ripple.style.width = size;
			_ripple.style.height = size;
			_ripple.style.opacity = 1f - k;
		}

		// =========================================
		// CHIPS
		// =========================================

		private void SetActiveStep(Step active, int required)
		{
			_activeStep = active;
			_activeRequired = required;

			for (int i = 0; i < StepCount; i++)
			{
				if (_chipLabels[i] != null)
					_chipLabels[i].text = CaptionFor((Step)i, _activeRequired);
			}

			ApplyStepAppearance();
		}

		private void ApplyStepAppearance()
		{
			for (int i = 0; i < StepCount; i++)
			{
				VisualElement chip = _chips[i];

				bool isActive = i == (int)_activeStep;
				bool isDone = i < (int)_activeStep;

				chip.RemoveFromClassList("active");
				chip.RemoveFromClassList("done");
				chip.RemoveFromClassList("inactive");

				chip.AddToClassList(isActive ? "active" : (isDone ? "done" : "inactive"));
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
	}
}