using Game.Content;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using R3;

namespace Game.Core
{
	public sealed class DialogueServiceTest
	{
		[Test]
		public void Play_NoEntry_IsNoOp()
		{
			DialogueService svc = NewService();
			svc.Play("card_x", "cam");
			Assert.That(svc.IsPlaying.Value, Is.False, "no entry means the NPC has nothing to say");
			svc.Dispose();
		}

		[Test]
		public void Play_StartsFirstLine_WithSpeaker()
		{
			DialogueService svc = NewService();
			svc.Play("card_cam", "cam");
			Assert.That(svc.IsPlaying.Value, Is.True);
			Assert.That(svc.SpeakerName.Value, Is.EqualTo("cam"));
			Assert.That(svc.CurrentLine.Value.Line, Is.EqualTo("first"));
			svc.Dispose();
		}

		[Test]
		public void Advance_ThroughLines_RecordsConversation_AndGrantsReward()
		{
			CardInventory cards = new CardInventory();
			DialogueSettings settings = NewSettings();
			DialogueService svc = new DialogueService(cards, settings);

			string conducted = null;
			using var sub = svc.ConversationConducted.Subscribe(key => conducted = key);

			svc.Play("card_cam", "cam");
			svc.Advance(); // second line
			Assert.That(svc.IsPlaying.Value, Is.True);
			svc.Advance(); // finish

			Assert.That(svc.IsPlaying.Value, Is.False);
			Assert.That(conducted, Is.EqualTo("card_cam_cam"));
			Assert.That(svc.HadConversation("card_cam", "cam"), Is.True);
			Assert.That(cards.Owns("card_reward"), Is.True, "reward card must be granted on finish");
			cards.Dispose();
			svc.Dispose();
		}

		[Test]
		public void Play_ChapterKey_PreferredOverDefault()
		{
			DialogueSettings settings = NewSettings();
			settings.Entries.Add("card_cam_cam_2", new DialogueEntry
			{
				Lines = new List<DialogueLine> { new DialogueLine { Line = "chapter2 line" } }
			});
			DialogueService svc = new DialogueService(new CardInventory(), settings);

			svc.Play("card_cam", "cam", chapter: 2);
			Assert.That(svc.CurrentLine.Value.Line, Is.EqualTo("chapter2 line"));

			svc.Dispose();
		}

		private static DialogueService NewService()
		{
			return new DialogueService(new CardInventory(), NewSettings());
		}

		private static DialogueSettings NewSettings()
		{
			var settings = ScriptableObject.CreateInstance<DialogueSettings>();
			settings.Entries["card_cam_cam"] = new DialogueEntry
			{
				RewardCardId = "card_reward",
				Lines = new List<DialogueLine>
				{
					new DialogueLine { Line = "first", DisplayDuration = 2f },
					new DialogueLine { Line = "second", DisplayDuration = 2f },
				}
			};
			return settings;
		}
	}
}