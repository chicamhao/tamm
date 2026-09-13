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
			// includeInactive: the template is inactive by design (Unity 6 defaults GetComponentInParent to skip inactive)
			Assert.IsNotNull(_cardButtonTemplate.GetComponentInParent<Button>(true), "card button template needs a Button component");
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
				// clone the template ROOT so Button + Image + Label come as one piece
				GameObject templateRoot = _cardButtonTemplate.transform.parent.gameObject;
				GameObject clone = Instantiate(templateRoot, _listRoot);
				SetActiveRecursively(clone, true); // Instantiate preserves per-GO inactive flags; wake the whole subtree
				TMP_Text label = clone.GetComponentInChildren<TMP_Text>(true);
				label.text = DisplayName(cardId);

				// stack top-down under the list root (anchored to its top edge)
				clone.transform.localPosition = new Vector3(0, -(float)_buttons.Count * 52.0f, 0);
				Button button = clone.GetComponent<Button>();
				if (button != null)
					button.onClick.AddListener(() => Pick(cardId));
				_buttons.Add(clone);
			}

			_panel.SetActive(true);
			_input.DisableInput(); // freeze Look/Move so the camera can't rotate under the menu
			_input.SetCursorState(false); // show + unlock the mouse so buttons are clickable
		}

		private void Pick(string cardId)
		{
			_panel.SetActive(false);
			Services.Dialogue?.Play(cardId, _actorId, Services.Chapter != null ? Services.Chapter.CurrentChapter.Value : 0);
			_input.EnableInput();
			_input.SetCursorState(true);
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

		private static void SetActiveRecursively(GameObject go, bool active)
		{
			go.SetActive(active);
			for (int i = 0; i < go.transform.childCount; i++)
				SetActiveRecursively(go.transform.GetChild(i).gameObject, active);
		}
	}
}