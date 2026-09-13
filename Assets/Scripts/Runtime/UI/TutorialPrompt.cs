using UnityEngine;

namespace Game.UI
{
	// First-run hint overlay (IMGUI, top-left). Shows only in exported builds — in-editor
	// play is the dev loop and skips it (devs have F2 DebugMenu instead). Auto-fades.
	public sealed class TutorialPrompt : MonoBehaviour
	{
		[SerializeField] private float _lifespan = 7.0f;

		private float _remaining;

		private void Start()
		{
			if (Application.isEditor)
			{
				enabled = false; // build-only prompt
				return;
			}
			_remaining = _lifespan;
		}

		private void Update()
		{
			if (_remaining > 0) _remaining -= Time.deltaTime;
		}

		private void OnGUI()
		{
			if (_remaining <= 0) return;

			GUI.Box(new Rect(16, 16, 380, 130), "");
			GUILayout.BeginArea(new Rect(24, 24, 360, 120));
			GUILayout.Label("Move: WASD       Look: Mouse");
			GUILayout.Label("Interact: E      Pause: P");
			GUILayout.Label("Interact with objects to collect cards,");
			GUILayout.Label("then use them when talking to people.");
			GUILayout.EndArea();
		}
	}
}