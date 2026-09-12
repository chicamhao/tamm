using Game.Core;
using R3;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.UI
{
	// Leaf view: collect count + countdown + outcome. Bootstrapper.Awake wires services
	// before any Start runs, so subscribing here is safe.
	public sealed class GameHud : MonoBehaviour
	{
		[SerializeField] private TMP_Text _collectText;
		[SerializeField] private TMP_Text _timeText;
		[SerializeField] private TMP_Text _resultText;

		private IDisposable _onCollected;
		private IDisposable _onTime;
		private IDisposable _onResult;

		private void Start()
		{
			Assert.IsNotNull(_collectText, "GameHud requires all TMP_Text fields assigned");
			GameSession session = Game.Core.Services.Session;
			if (session == null) return; // running outside a Bootstrapper scene

			_collectText.text = "Collect 0/" + session.Total;
			_timeText.text = FormatTime(session.Remaining.Value);

			_onCollected = session.Collected.Subscribe(collected => _collectText.text = "Collect " + collected + "/" + session.Total);
			_onTime = session.Remaining.Subscribe(remaining => _timeText.text = FormatTime(remaining));
			_onResult = session.Result.Subscribe(result =>
			{
				if (result == GameResult.Won) _resultText.text = session.WinMessage;
				else if (result == GameResult.Lost) _resultText.text = session.LoseMessage;
				else _resultText.text = "";
			});
		}

		private void OnDestroy()
		{
			_onCollected?.Dispose();
			_onTime?.Dispose();
			_onResult?.Dispose();
		}

		private static String FormatTime(float seconds)
		{
			int total = (int)seconds;
			int minutes = total / 60;
			int secs = total % 60;
			return minutes + ":" + (secs < 10 ? "0" : "") + secs;
		}
	}
}