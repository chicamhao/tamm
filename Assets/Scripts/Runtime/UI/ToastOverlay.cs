using Game.Content;
using Game.Core;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
	// Card-acquired notifications: a stack of toasts in the runtime UI panel, zero
	// scene wiring beyond dropping this component on any object. Fades each toast
	// out after a few seconds.
	public sealed class ToastOverlay : MonoBehaviour
	{
		private sealed class Toast
		{
			public Label View;
			public float Remaining;
		}

		[SerializeField] private CardSettings _cards; // display names (optional)
		[SerializeField] private float _lifespan = 4.0f;
		[SerializeField] private RuntimeUI _runtimeUI;

		private readonly List<Toast> _toasts = new();
		private VisualElement _host;
		private IDisposable _onGranted;

		private void Start()
		{
			RuntimeUI ui = RuntimeUI.Resolve(_runtimeUI);
			if (ui == null)
			{
				enabled = false;
				return;
			}

			_host = ui.Q("ToastsHost");
			if (Bootstrapper.Instance == null)
			{
				Debug.LogWarning("ToastOverlay: core scene not running — open Assets/Scenes/Bootstrapper.unity and press Play (it loads the Playground level)");
				return;
			}

			_onGranted = Services.Cards.Granted.Subscribe(OnGranted);
		}

		private void OnGranted(string cardId)
		{
			Label label = new Label("Got: " + DisplayName(cardId));
			label.AddToClassList("toast");
			_host.Add(label);
			_toasts.Add(new Toast { View = label, Remaining = _lifespan });
		}

		private void Update()
		{
			if (_toasts.Count == 0) return;
			float dt = Time.deltaTime;
			for (int i = _toasts.Count - 1; i >= 0; i--)
			{
				Toast toast = _toasts[i];
				toast.Remaining -= dt;
				if (toast.Remaining <= 0f)
				{
					toast.View.RemoveFromHierarchy();
					_toasts.RemoveAt(i);
				}
				else if (toast.Remaining < 0.5f)
				{
					toast.View.style.opacity = Mathf.Clamp01(toast.Remaining / 0.5f); // final half-second fade
				}
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