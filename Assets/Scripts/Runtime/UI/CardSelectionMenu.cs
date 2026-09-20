using Game.Content;
using Game.Core;
using Game.Input;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Game.UI
{
	// Leaf view: opens when the player uses a card on an NPC (CardSelectionRequested),
	// lists owned cards as buttons. Picking one hands off to DialogueService.Play —
	// the NPC responds (or has nothing to say) from there.
	public sealed class CardSelectionMenu : MonoBehaviour
	{
		[SerializeField] private CardSettings _cards; // for display names; label falls back to the raw id
		[SerializeField] private InputMonitor _input; // frozen while the menu is open (like SettingsMenu)
		[SerializeField] private RuntimeUI _runtimeUI;

		private readonly List<Button> _buttons = new();
		private VisualElement _screen;
		private VisualElement _listRoot;
		private IDisposable _onRequest;
		private string _actorId;

		private void Start()
		{
			RuntimeUI ui = RuntimeUI.Resolve(_runtimeUI);
			Assert.IsNotNull(ui, "CardSelectionMenu requires a RuntimeUI in the scene");

			_screen = ui.Q("CardScreen");
			_listRoot = ui.Q("CardList");
			Assert.IsNotNull(_listRoot, "CardSelectionMenu requires a CardList element");
			Assert.IsNotNull(_input, "CardSelectionMenu requires the player's InputMonitor assigned");

			_screen.style.display = DisplayStyle.None;

			_onRequest = Services.Cards.CardSelectionRequested.Subscribe(Open);
		}

		private void Open(string actorId)
		{
			IReadOnlyCollection<string> owned = Services.Cards.Owned;
			if (owned.Count == 0) return;
			_actorId = actorId;

			Clear();

			foreach (string cardId in owned)
			{
				Button button = new Button { text = DisplayName(cardId) };
				button.AddToClassList("card-button");
				button.clicked += () => Pick(cardId); // captured cardId is stable per iteration
				_listRoot.Add(button);
				_buttons.Add(button);
			}

			_screen.style.display = DisplayStyle.Flex;
			_input.DisableInput(); // freeze Look/Move so the camera can't rotate under the menu
			_input.SetCursorState(false); // show + unlock the mouse so buttons are clickable
		}

		private void Clear()
		{
			foreach (Button button in _buttons)
				button.RemoveFromHierarchy();
			_buttons.Clear();
		}

		private void Pick(string cardId)
		{
			_screen.style.display = DisplayStyle.None;
			Clear();
			Services.Dialogue.Play(cardId, _actorId, Services.Chapter.CurrentChapter.Value);
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
	}
}