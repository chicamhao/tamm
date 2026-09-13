using NUnit.Framework;
using R3;
using System.Collections.Generic;

namespace Game.Core
{
	public sealed class InteractionServiceTest
	{
		[Test]
		public void Interact_CardId_GrantsCard()
		{
			CardInventory inv = new CardInventory();
			InteractionService svc = new InteractionService(inv, new[] { "card_cam" }, new string[0]);

			bool granted = false;
			using var sub = inv.Granted.Subscribe(id => granted |= id == "card_cam");

			svc.Interact("card_cam");

			Assert.That(granted, Is.True);
			Assert.That(inv.Owns("card_cam"), Is.True);
			inv.Dispose();
		}

		[Test]
		public void Interact_ActorId_FiresSelectionRequest()
		{
			CardInventory inv = new CardInventory();
			InteractionService svc = new InteractionService(inv, new string[0], new[] { "cam" });

			string requested = null;
			using var sub = inv.CardSelectionRequested.Subscribe(actorId => requested = actorId);

			svc.Interact("cam");

			Assert.That(requested, Is.EqualTo("cam"));
			Assert.That(inv.Owned.Count, Is.EqualTo(0), "actor interaction must not grant a card");
			inv.Dispose();
		}

		[Test]
		public void Interact_UnknownId_DoesNothing()
		{
			CardInventory inv = new CardInventory();
			InteractionService svc = new InteractionService(inv, new[] { "card_cam" }, new[] { "cam" });

			bool anything = false;
			using var granted = inv.Granted.Subscribe(_ => anything = true);
			using var requested = inv.CardSelectionRequested.Subscribe(_ => anything = true);

			svc.Interact("nosuchid");

			Assert.That(anything, Is.False);
			Assert.That(inv.Owned.Count, Is.EqualTo(0));
			inv.Dispose();
		}

		// --- DeriveActorIds ---

		[Test]
		public void DeriveActors_FromDialogueKeys_StripsCardPrefix()
		{
			var result = InteractionService.DeriveActorIds(
				new[] { "card_betelPalmRoot_cam", "card_betelPalmRoot_meKe", "card_cam_cam" },
				null,
				new[] { "card_betelPalmRoot", "card_cam" });
			Assert.That(result, Is.EqualTo(new List<string> { "cam", "meKe" }));
		}

		[Test]
		public void DeriveActors_TrailingChapterSuffix_IsStripped()
		{
			var result = InteractionService.DeriveActorIds(
				new[] { "card_betelPalmRoot_cam", "card_betelPalmRoot_cam_2" },
				new[] { "cam_1", "meKe_2" },
				new[] { "card_betelPalmRoot" });
			Assert.That(result, Is.EqualTo(new List<string> { "cam", "meKe" }));
		}

		[Test]
		public void DeriveActors_EmptyInput_IsEmpty()
		{
			var result = InteractionService.DeriveActorIds(null, null, null);
			Assert.That(result.Count, Is.EqualTo(0));
		}

		[Test]
		public void DeriveActors_UnknownCardPrefix_KeyIsSkipped()
		{
			var result = InteractionService.DeriveActorIds(new[] { "card_missing_cam" }, null, new[] { "card_cam" });
			Assert.That(result.Count, Is.EqualTo(0));
		}
	}
}