using Game.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Minigames.Rat
{
	// The single authority for the minigame: rounds, turns, grouping, hearts and
	// the catch-line miss check. Input and UI both talk to this; neither holds rules.
	//
	// Ported from rice/rat prototype: score counter and TuningHud are gone; the
	// ActionPromptPanel HUD is ported with it. Win/lose report to the core
	// MinigameService, which grants the reward card and returns to the narrative
	// level (Bootstrapper.LoadLevel handles the scene swap). The scene is
	// unloaded when the minigame ends, so the static Instance needs lifecycle cleanup.
	[Unity.Scripting.LifecycleManagement.AutoStaticsCleanup]
	public sealed partial class RatManager : MonoBehaviour
	{
		public static RatManager Instance;

		public enum GameState
		{
			WaitingForThrow,
			Picking,
			WaitingForCatch,
			RoundComplete,
			Failed,
			GameOver,
			SliceComplete
		}

		public GameState state;

		[Header("Gameplay")]
		public int round = 1;
		public int hearts = 3;

		public BallController ball;

		[Header("Catch line")]
		[Tooltip("Catch line height ABOVE the throw point. The ball rests on the table at its start height, so a line below that could never be crossed.")]
		public float catchLineOffset = 0.05f;

		[Header("Pacing (seconds)")]
		public float turnDelay = 0.7f;
		public float failDelay = 1.1f;
		public float roundDelay = 1.4f;

		[Header("UI")]
		public TMP_Text roundText;
		public TMP_Text instructionText;
		public TMP_Text progressText;
		public TMP_Text heartsText;
		public TMP_Text resultText;

		private ChopstickManager chopstickManager;
		private ActionPromptPanel prompts;

		private int remainingInRound;
		private int requiredThisTurn;
		private int collectedThisTurn;

		private readonly List<Chopstick> collectedThisTurnList = new List<Chopstick>();

		/// <summary>Minigame id registered in MinigameSettings; used to report the outcome.</summary>
		private static readonly string MinigameId = "rat";

		public int RequiredThisTurn => requiredThisTurn;
		public int CollectedThisTurn => collectedThisTurn;

		/// <summary>Y at which a descending ball counts as missed. Sits just above the throw point.</summary>
		public float CatchLineY => ball != null ? ball.LaunchY + catchLineOffset : 0f;

		private void Awake()
		{
			Instance = this;

			if (ball == null)
				ball = FindFirstObjectByType<BallController>();

			chopstickManager = FindFirstObjectByType<ChopstickManager>();

			if (chopstickManager == null)
				Debug.LogError("RatManager: no ChopstickManager in the scene.", this);

			Canvas canvas = FindFirstObjectByType<Canvas>();

			prompts = ActionPromptPanel.CreateIn(canvas);

			if (prompts != null)
				prompts.AdoptExistingTexts(roundText, instructionText, progressText, heartsText, resultText);
		}

		private void Start() => StartGame();

		public void StartGame()
		{
			CancelInvoke();

			round = 1;
			hearts = 3;

			StartRound();
		}

		private void StartRound()
		{
			CancelInvoke();

			remainingInRound = RoundRules.TotalFor(round);

			ApplyRoundConfig();

			if (chopstickManager != null)
				chopstickManager.DropChopsticks(remainingInRound);

			ShowRoundIntro();

			BeginTurn();
		}

		/// <summary>Pushes this round's authored numbers into the ball and the chopsticks.</summary>
		private void ApplyRoundConfig()
		{
			// The slice uses the traditional rules with no per-round tuning: every
			// round keeps the built-in ball and spawn settings.
			if (prompts != null)
				prompts.SetRoundStyle(RoundRules.PromptVerb(round), RoundRules.AllowSweep(round));
		}

		private void BeginTurn()
		{
			CancelInvoke();

			requiredThisTurn = RoundRules.RequiredForTurn(round, remainingInRound);
			collectedThisTurn = 0;
			collectedThisTurnList.Clear();

			state = GameState.WaitingForThrow;

			if (ball != null)
				ball.ResetBall();

			if (prompts != null)
				prompts.ShowThrow();

			UpdateUI();
		}

		// =========================================
		// INPUT ENTRY POINTS (called by GestureInput)
		// =========================================

		public void OnThrowInput(float normalizedStrength)
		{
			if (state != GameState.WaitingForThrow || ball == null)
				return;

			ball.Throw(normalizedStrength);

			OnBallThrown();
		}

		public void OnBallThrown()
		{
			if (state != GameState.WaitingForThrow)
				return;

			state = GameState.Picking;

			ClearResult();

			if (prompts != null)
				prompts.ShowTap(collectedThisTurn, requiredThisTurn);

			UpdateUI();
		}

		public void OnChopstickTapped(Chopstick chopstick)
		{
			if (state != GameState.Picking || chopstick == null)
				return;

			// Any available chopstick counts - there is no wrong one. Failure in this
			// slice is timing only.
			if (!chopstick.TryCollect())
				return;

			collectedThisTurn++;
			collectedThisTurnList.Add(chopstick);

			if (collectedThisTurn >= requiredThisTurn)
				EnterCatchPhase();

			UpdateUI();
		}

		private void EnterCatchPhase()
		{
			state = GameState.WaitingForCatch;

			if (prompts != null && ball != null)
				prompts.ShowCatch(ball.transform);
		}

		public void TryCatchBall()
		{
			if (state != GameState.WaitingForCatch || ball == null)
				return;

			ball.Catch();

			OnBallCaught();
		}

		// =========================================
		// OUTCOMES
		// =========================================

		private void OnBallCaught()
		{
			state = GameState.RoundComplete;

			remainingInRound -= requiredThisTurn;

			SetResult("SUCCESS");

			if (prompts != null)
				prompts.Hide();

			UpdateUI();

			if (remainingInRound <= 0)
				Invoke(nameof(CompleteRound), roundDelay);
			else
				Invoke(nameof(BeginTurn), turnDelay);
		}

		private void CompleteRound()
		{
			if (round >= RoundRules.FinalRound)
			{
				EnterSliceComplete();
				return;
			}

			round++;

			StartRound();
		}

		public void FailTurn()
		{
			if (state == GameState.Failed ||
				state == GameState.GameOver ||
				state == GameState.SliceComplete)
				return;

			CancelInvoke();

			state = GameState.Failed;

			hearts--;

			SetResult("MISS");

			if (prompts != null)
				prompts.Hide();

			if (ball != null)
				ball.ResetBall();

			// The turn failed, so chopsticks taken during it go back on the table.
			if (chopstickManager != null)
				chopstickManager.ReturnToTable(collectedThisTurnList);

			collectedThisTurnList.Clear();
			collectedThisTurn = 0;

			UpdateUI();

			if (hearts <= 0)
				Invoke(nameof(EnterGameOver), failDelay);
			else
				Invoke(nameof(BeginTurn), failDelay);
		}

		private void EnterGameOver()
		{
			state = GameState.GameOver;

			SetResult("You ran out of hearts");

			ReportOutcome(false);
		}

		private void EnterSliceComplete()
		{
			state = GameState.SliceComplete;

			SetResult("ALL ROUNDS CLEAR");

			ReportOutcome(true);
		}

		/// <summary>Reports win/lose to the core service, which grants the reward and returns to the narrative.</summary>
		private void ReportOutcome(bool won)
		{
			// Unloading the minigame scene destroys this GameObject next frame; the
			// static Instance is cleared by the core scene teardown, not here.
			Services.Minigame?.Complete(MinigameId, won);
		}

		// =========================================
		// MISS DETECTION
		// =========================================

		private void Update()
		{
			if (state != GameState.Picking && state != GameState.WaitingForCatch)
				return;

			if (ball == null)
				return;

			// Two independent miss conditions: the ball crossed the catch line on the way
			// down, or it has come to rest after being thrown. The second covers geometry
			// the first cannot - a ball scaled large enough never reaches the line.
			if (ball.HasFallenBelow(CatchLineY) || ball.HasSettledAfterThrow())
				FailTurn();
		}

		// =========================================
		// UI
		// =========================================

		private void ShowRoundIntro()
		{
			SetResult(RoundRules.RoundName(round) + "\n" + RoundRules.TurnBreakdown(round));
		}

		private void SetResult(string s)
		{
			if (resultText != null)
				resultText.text = s;
		}

		private void ClearResult() => SetResult("");

		private void UpdateUI()
		{
			if (roundText != null)
				roundText.text = "ROUND " + round + " - " + RoundRules.RoundName(round);

			if (heartsText != null)
				heartsText.text = "HEARTS: " + Mathf.Max(hearts, 0);
		}
	}
}