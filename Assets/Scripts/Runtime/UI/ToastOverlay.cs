using Game.Content;
using Game.Core;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.UI
{
	// Card-acquired notifications: a stack of IMGUI toasts, zero scene wiring beyond
	// dropping this component on any object. Fades each toast out after a few seconds.
	// ponytail: dev-grade overlay; swap for a Canvas toast prefab when UI art lands.
	public sealed class ToastOverlay : MonoBehaviour
	{
		private sealed class Toast
		{
			public string Text;
			public float Remaining;
		}

		[SerializeField] private CardSettings _cards; // display names (optional)
		[SerializeField] private float _lifespan = 4.0f;

		private readonly List<Toast> _toasts = new();
		private IDisposable _onGranted;

		private void Start()
		{
			_onGranted = Services.Cards.Granted.Subscribe(OnGranted);
		}

		private void OnGranted(string cardId)
		{
			Toast toast = new Toast { Text = "Got: " + DisplayName(cardId), Remaining = _lifespan };
			_toasts.Add(toast);
		}

		private void Update()
		{
			if (_toasts.Count == 0) return;
			float dt = Time.deltaTime;
			for (int i = _toasts.Count - 1; i >= 0; i--)
			{
				_toasts[i].Remaining -= dt;
				if (_toasts[i].Remaining <= 0) _toasts.RemoveAt(i);
			}
		}

		private void OnGUI()
		{
			if (_toasts.Count == 0) return;
			float y = 60;
			foreach (Toast toast in _toasts)
			{
                GUI.Label(new Rect(12, y, 400, 22), toast.Text);
				y += 24;
			}
		}

		private string DisplayName(string cardId)
		{
			if (_cards != null && _cards.Entries.TryGetValue(cardId, out Card card) && card.DisplayName.Length > 0)
				return card.DisplayName;
			return cardId;
		}

		private void OnDestroy() => _onGranted?.Dispose();
	}
}