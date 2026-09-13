using Game.Content;
using Game.Core;
using Game.Input;
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
	// lists owned cards as buttons. Picking one hands off to DialogueService.Play —
	// the NPC responds (or has nothing to say) from there.
	public sealed class CardSelectionMenu : MonoBehaviour
	{
		[SerializeField] private GameObject _panel;
		[SerializeField] private RectTransform _listRoot;
		[SerializeField] private TMP_Text _cardButtonTemplate; // inactive template child with a Button
		[SerializeField] private CardSettings _cards; // for display names; label falls back to the raw id
		[SerializeField] private InputMonitor _input; // frozen while the menu is open (like SettingsMenu)

		private readonly List<GameObject> _buttons = new();
		private IDisposable _onRequest;
		private string _actorId;

		private void Start()
		{
			Assert.IsNotNull(_panel, "CardSelectionMenu requires _panel");
			Assert.IsNotNull(_listRoot, "CardSelectionMenu requires _listRoot");
			Assert.IsNotNull(_cardButtonTemplate, "CardSelectionMenu requires a button template");
			Assert.IsNotNull(_cardButtonTemplate.GetComponent<Button>(), "card button template needs a Button component");
			Assert.IsNotNull(_input, "CardSelectionMenu requires the player's InputMonitor assigned");

			_cardButtonTemplate.gameObject.SetActive(false);
			_panel.SetActive(false);

			if (Services.Cards == null) return; // outside a Bootstrapper scene
			_onRequest = Services.Cards.CardSelectionRequested.Subscribe(Open);
		}

		private void Open(string actorId)
		{
			IReadOnlyCollection<string> owned = Services.Cards.Owned;
			if (owned.Count == 0) return;
			_actorId = actorId;

			foreach (GameObject go in _buttons) Destroy(go);
			_buttons.Clear();

			foreach (string cardId in owned)
			{
				TMP_Text label = Instantiate(_cardButtonTemplate, _listRoot);
				label.gameObject.SetActive(true);
				label.text = DisplayName(cardId);
				Button button = label.GetComponent<Button>();
				button.onClick.AddListener(() => Pick(cardId));
				_buttons.Add(label.gameObject);
			}

			_panel.SetActive(true);
			_input.DisableInput();
		}

		private void Pick(string cardId)
		{
			_panel.SetActive(false);
			Services.Dialogue?.Play(_actorId, cardId, Services.Chapter != null ? Services.Chapter.CurrentChapter.Value : 0);
			_input.EnableInput();
		}

		private string DisplayName(string cardId)
		{
			if (_cards != null && _cards.Entries.TryGetValue(cardId, out Card card))
				return card.DisplayName;
			return cardId;
		}

		private void OnDestroy()
		{
			_onRequest?.Dispose();
		}
	}
}