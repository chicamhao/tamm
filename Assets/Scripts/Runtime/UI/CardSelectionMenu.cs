using Game.Core;
using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Game.UI
{
	// Leaf view: opens when the player uses a card on an NPC (CardSelectionRequested),
	// lists owned cards as buttons. Picking one publishes CardSelected — the dialogue
	// system consumes that next.
	public sealed class CardSelectionMenu : MonoBehaviour
	{
		[SerializeField] private GameObject _panel;
		[SerializeField] private RectTransform _listRoot;
		[SerializeField] private TMP_Text _cardButtonTemplate; // inactive template child with a Button

		private readonly List<GameObject> _buttons = new();
		private IDisposable _onRequest;

		public Subject<string> CardSelected { get; } = new();

		private void Start()
		{
			Assert.IsNotNull(_panel, "CardSelectionMenu requires _panel");
			Assert.IsNotNull(_listRoot, "CardSelectionMenu requires _listRoot");
			Assert.IsNotNull(_cardButtonTemplate, "CardSelectionMenu requires a button template");
			Assert.IsNotNull(_cardButtonTemplate.GetComponent<Button>(), "card button template needs a Button component");

			_cardButtonTemplate.gameObject.SetActive(false);
			_panel.SetActive(false);

			_onRequest = Services.Cards.CardSelectionRequested.Subscribe(Open);
		}

		private void Open(string actorId)
		{
			IReadOnlyCollection<string> owned = Services.Cards.Owned;
			if (owned.Count == 0) return;

			foreach (GameObject go in _buttons) Destroy(go);
			_buttons.Clear();

			foreach (string cardId in owned)
			{
				TMP_Text label = Instantiate(_cardButtonTemplate, _listRoot);
				label.gameObject.SetActive(true);
				label.text = cardId;
				Button button = label.GetComponent<Button>();
				button.onClick.AddListener(() => Pick(cardId));
				_buttons.Add(label.gameObject);
			}

			_panel.SetActive(true);
		}

		private void Pick(string cardId)
		{
			_panel.SetActive(false);
			CardSelected.OnNext(cardId);
		}

		private void OnDestroy()
		{
			_onRequest?.Dispose();
			CardSelected.Dispose();
		}
	}
}