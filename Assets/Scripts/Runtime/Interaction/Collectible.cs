using Game.Core;
using UnityEngine;

namespace Game.Interaction
{
	// Click target: collected by ClickCollector when the crosshair ray hits this object.
	public sealed class Collectible : MonoBehaviour
	{
		private bool _collected;

		public void Collect()
		{
			if (_collected) return;
			GameSession session = Game.Core.Services.Session;
			if (session == null || session.Result.Value != GameResult.InProgress) return;
			
			_collected = true;
			session.Collect();
			gameObject.SetActive(false);
		}
	}
}