using Game.Core;
using Game.Minigames.Rat.UI;
using System.Collections.Generic;
using UnityEngine;

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

		public GameState State;

		[Header("Gameplay")]
		public int Round = 1;
		public int Hearts = 3;

		public BallController Ball;

		[Header("Catch line")]
		[Tooltip("Catch line height ABOVE the throw point. The ball rests on the table at its start height, so a line below that could never be crossed.")]
		public float CatchLineOffset = 0.05f;

		[Header("Pacing (seconds)")]
		public float TurnDelay = 0.7f;
		public float FailDelay = 1.1f;
		public float RoundDelay = 1.4f;

		[Header("UI")]
		public RatHud Hud;

		private ChopstickManager _chopstickManager;

		private int _remainingInRound;
		private int _requiredThisTurn;
		private int _collectedThisTurn;

		private readonly List<Chopstick> _collectedThisTurnList = new List<Chopstick>();

		/// <summary>Minigame id registered in MinigameSettings; used to report the outcome.</summary>
		private static readonly string _minigameId = "rat";

		public int RequiredThisTurn => _requiredThisTurn;
		public int CollectedThisTurn => _collectedThisTurn;

		/// <summary>Y at which a descending ball counts as missed. Sits just above the throw point.</summary>
		public float CatchLineY => Ball != null ? Ball.LaunchY + CatchLineOffset : 0f;

		private void Awake()
		{
			Instance = this;

			if (Ball == null)
				Ball = FindAnyObjectByType<BallController>();

			_chopstickManager = FindAnyObjectByType<ChopstickManager>();

			if (_chopstickManager == null)
				Debug.LogError("RatManager: no ChopstickManager in the scene.", this);

			if (Hud == null)
				Hud = FindFirstObjectByType<RatHud>();

			if (Hud == null)
				Debug.LogError("RatManager: no RatHud in the scene.", this);
		}

		private void Start() => StartGame();

		public void StartGame()
		{
			CancelInvoke();

			Round = 1;
			Hearts = 3;

			StartRound();
		}

		private void StartRound()
		{
			CancelInvoke();

			_remainingInRound = RoundRules.TotalFor(Round);

			ApplyRoundConfig();

			if (_chopstickManager != null)
				_chopstickManager.DropChopsticks(_remainingInRound);

			ShowRoundIntro();

			BeginTurn();
		}

		/// <summary>Pushes this round's authored numbers into the ball and the chopsticks.</summary>
		private void ApplyRoundConfig()
		{
			// The slice uses the traditional rules with no per-round tuning: every
			// round keeps the built-in ball and spawn settings.
			if (Hud != null)
				Hud.SetRoundStyle(RoundRules.PromptVerb(Round), RoundRules.AllowSweep(Round));
		}

		private void BeginTurn()
		{
			CancelInvoke();

			_requiredThisTurn = RoundRules.RequiredForTurn(Round, _remainingInRound);
			_collectedThisTurn = 0;
			_collectedThisTurnList.Clear();

			State = GameState.WaitingForThrow;

			if (Ball != null)
				Ball.ResetBall();

			if (Hud != null)
				Hud.ShowThrow();

			UpdateUI();
		}

		// =========================================
		// INPUT ENTRY POINTS (called by GestureInput)
		// =========================================

		public void OnThrowInput(float normalizedStrength)
		{
			if (State != GameState.WaitingForThrow || Ball == null)
				return;

			Ball.Throw(normalizedStrength);

			OnBallThrown();
		}

		public void OnBallThrown()
		{
			if (State != GameState.WaitingForThrow)
				return;

			State = GameState.Picking;

			ClearResult();

			if (Hud != null)
				Hud.ShowTap(_collectedThisTurn, _requiredThisTurn);

			UpdateUI();
		}

		public void OnChopstickTapped(Chopstick chopstick)
		{
			if (State != GameState.Picking || chopstick == null)
				return;

			// Any available chopstick counts - there is no wrong one. Failure in this
			// slice is timing only.
			if (!chopstick.TryCollect())
				return;

			_collectedThisTurn++;
			_collectedThisTurnList.Add(chopstick);

			if (_collectedThisTurn >= _requiredThisTurn)
				EnterCatchPhase();

			UpdateUI();
		}

		private void EnterCatchPhase()
		{
			State = GameState.WaitingForCatch;

			if (Hud != null && Ball != null)
				Hud.ShowCatch(Ball.transform);
		}

		public void TryCatchBall()
		{
			if (State != GameState.WaitingForCatch || Ball == null)
				return;

			Ball.Catch();

			OnBallCaught();
		}

		// =========================================
		// OUTCOMES
		// =========================================

		private void OnBallCaught()
		{
			State = GameState.RoundComplete;

			_remainingInRound -= _requiredThisTurn;

			SetResult("SUCCESS");

			if (Hud != null)
				Hud.Hide();

			UpdateUI();

			if (_remainingInRound <= 0)
				Invoke(nameof(CompleteRound), RoundDelay);
			else
				Invoke(nameof(BeginTurn), TurnDelay);
		}

		private void CompleteRound()
		{
			if (Round >= RoundRules.FinalRound)
			{
				EnterSliceComplete();
				return;
			}

			Round++;

			StartRound();
		}

		public void FailTurn()
		{
			if (State == GameState.Failed ||
				State == GameState.GameOver ||
				State == GameState.SliceComplete)
				return;

			CancelInvoke();

			State = GameState.Failed;

			Hearts--;

			SetResult("MISS");

			if (Hud != null)
				Hud.Hide();

			if (Ball != null)
				Ball.ResetBall();

			// The turn failed, so chopsticks taken during it go back on the table.
			if (_chopstickManager != null)
				_chopstickManager.ReturnToTable(_collectedThisTurnList);

			_collectedThisTurnList.Clear();
			_collectedThisTurn = 0;

			UpdateUI();

			if (Hearts <= 0)
				Invoke(nameof(EnterGameOver), FailDelay);
			else
				Invoke(nameof(BeginTurn), FailDelay);
		}

		private void EnterGameOver()
		{
			State = GameState.GameOver;

			SetResult("You ran out of hearts");

			ReportOutcome(false);
		}

		private void EnterSliceComplete()
		{
			State = GameState.SliceComplete;

			SetResult("ALL ROUNDS CLEAR");

			ReportOutcome(true);
		}

		/// <summary>Reports win/lose to the core service, which grants the reward and returns to the narrative.</summary>
		private void ReportOutcome(bool won)
		{
			// Unloading the minigame scene destroys this GameObject next frame; the
			// static Instance is cleared by the core scene teardown, not here.
			Services.Minigame?.Complete(_minigameId, won);
		}

		// =========================================
		// MISS DETECTION
		// =========================================

		private void Update()
		{
			if (State != GameState.Picking && State != GameState.WaitingForCatch)
				return;

			if (Ball == null)
				return;

			// Two independent miss conditions: the ball crossed the catch line on the way
			// down, or it has come to rest after being thrown. The second covers geometry
			// the first cannot - a ball scaled large enough never reaches the line.
			if (Ball.HasFallenBelow(CatchLineY) || Ball.HasSettledAfterThrow())
				FailTurn();
		}

		// =========================================
		// UI
		// =========================================

		private void ShowRoundIntro()
		{
			SetResult(RoundRules.RoundName(Round) + "\n" + RoundRules.TurnBreakdown(Round));
		}

		private void SetResult(string s)
		{
			if (Hud != null)
				Hud.SetResultText(s);
		}

		private void ClearResult() => SetResult("");

		private void UpdateUI()
		{
			if (Hud != null)
				Hud.SetRoundText("ROUND " + Round + " - " + RoundRules.RoundName(Round));

			if (Hud != null)
				Hud.SetHeartsText("HEARTS: " + Mathf.Max(Hearts, 0));
		}
	}
}