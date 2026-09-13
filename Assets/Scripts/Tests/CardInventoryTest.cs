using R3;
using NUnit.Framework;

namespace Game.Core
{
	public sealed class CardInventoryTest
	{
		[Test]
		public void GrantId_AddsOnce_AndFires()
		{
			CardInventory inv = new CardInventory();
			bool fired = false;
			using var sub = inv.Granted.Subscribe(id => fired |= id == "card_cam");

			inv.GrantId("card_cam");
			inv.GrantId("card_cam"); // duplicate must be a no-op

			Assert.That(inv.Owns("card_cam"), Is.True);
			int count = 0;
			foreach (var _ in inv.Owned) count++;
			Assert.That(count, Is.EqualTo(1), "granting twice must not double-own");
			Assert.That(fired, Is.True);
			inv.Dispose();
		}

		[Test]
		public void GrantId_EmptyOrNull_IsIgnored()
		{
			CardInventory inv = new CardInventory();
			inv.GrantId(null);
			inv.GrantId("");
			Assert.That(inv.Owned.Count, Is.EqualTo(0));
			inv.Dispose();
		}

		[Test]
		public void GrantIds_RestoresWithoutFiring()
		{
			CardInventory inv = new CardInventory();
			bool fired = false;
			using var sub = inv.Granted.Subscribe(_ => fired = true);

			inv.GrantIds(new[] { "card_a", "card_b" });

			Assert.That(inv.Owns("card_a") && inv.Owns("card_b"), Is.True);
			Assert.That(fired, Is.False, "restore must not publish Granted");
			inv.Dispose();
		}
	}
}