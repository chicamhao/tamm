using Game.Content;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
	public sealed class ChapterStateTest
	{
		private CardInventory _cards;
		private DialogueService _dialogue;

		[Test]
		public void Grant_MeetsGate_AdvancesChapter()
		{
			ChapterState state = BuildState(new ProgressCondition
			{
				Type = ProgressConditionType.OwnsCard,
				CardId = "card_cam"
			});

			_cards.GrantId("card_other");
			Assert.That(state.CurrentChapter.Value, Is.EqualTo(1), "unrelated progress must not advance");

			_cards.GrantId("card_cam");
			Assert.That(state.CurrentChapter.Value, Is.EqualTo(2));
		}

		[Test]
		public void AllConditionsRequired_NoPartialAdvance()
		{
			ChapterState state = BuildState(
				new ProgressCondition { Type = ProgressConditionType.OwnsCard, CardId = "card_cam" },
				new ProgressCondition { Type = ProgressConditionType.HadConversation, CardId = "card_cam", ActorId = "cam" });

			_cards.GrantId("card_cam"); // only one of two conditions
			Assert.That(state.CurrentChapter.Value, Is.EqualTo(1));
		}

		[Test]
		public void Conversation_MeetsGate_AdvancesChapter()
		{
			ChapterState state = BuildState(new ProgressCondition
			{
				Type = ProgressConditionType.HadConversation,
				CardId = "card_cam",
				ActorId = "cam"
			});

			// Play + advance twice = finish -> ConversationConducted fires
			_dialogue.Play("card_cam", "cam");
			_dialogue.Advance();
			_dialogue.Advance();

			Assert.That(state.CurrentChapter.Value, Is.EqualTo(2));
		}

		[Test]
		public void NoGate_StaysOnOne()
		{
			ChapterState state = BuildState();
			_cards.GrantId("card_anything");
			Assert.That(state.CurrentChapter.Value, Is.EqualTo(1));
		}

		// --- helpers ---

		private ChapterState BuildState(params ProgressCondition[] gateConditions)
		{
			_cards = new CardInventory();
			DialogueSettings dialogueSettings = ScriptableObject.CreateInstance<DialogueSettings>();
			dialogueSettings.Entries["card_cam_cam"] = new DialogueEntry
			{
				Lines = new List<DialogueLine>
				{
					new DialogueLine { Line = "a" },
					new DialogueLine { Line = "b" },
				}
			};
			_dialogue = new DialogueService(_cards, dialogueSettings);

			ChapterSettings settings = ScriptableObject.CreateInstance<ChapterSettings>();
			if (gateConditions != null && gateConditions.Length > 0)
			{
				settings.Gates.Add(new ChapterGate { Chapter = 2, Conditions = new List<ProgressCondition>(gateConditions) });
			}

			return new ChapterState(_cards, _dialogue, settings);
		}
	}
}