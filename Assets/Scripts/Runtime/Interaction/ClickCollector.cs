using Game.Input;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Interaction
{
	// Click target collector: when the hub fires Interact, raycasts from the screen
	// center (crosshair) and collects the Collectible that was hit. Needs the hub
	// component (the player's InputMonitor) assigned in the Inspector.
	public sealed class ClickCollector : MonoBehaviour
	{
		[SerializeField] private InputMonitor _input;
		[SerializeField] private float _maxReach = 100.0f;

		private Camera _camera;

		private void Awake()
		{
			Assert.IsNotNull(_input, "ClickCollector requires the player's InputMonitor assigned");

			GameObject main = GameObject.FindGameObjectWithTag("MainCamera");
			_camera = main == null ? null : main.GetComponent<Camera>();
			Assert.IsNotNull(_camera, "ClickCollector requires a Camera tagged MainCamera");
		}

		private void Update()
		{
			if (!_input.GetInteractInputDown()) return;
			CollectAtCenter();
		}

		private void CollectAtCenter()
		{
			Vector3 center = new(Screen.width / 2.0f, Screen.height / 2.0f, 0.0f);
			RaycastHit hit;
			if (!Physics.Raycast(_camera.ScreenPointToRay(center), out hit, _maxReach)) return;

			Collectible collectible = hit.collider.GetComponentInParent<Collectible>();
			if (collectible != null) collectible.Collect();
		}
	}
}