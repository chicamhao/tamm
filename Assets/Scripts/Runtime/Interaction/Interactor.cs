using Game.Input;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Interaction
{
	// Crosshair-ray interaction: on Interact input, finds the hit Interactable and
	// calls Interact(). Needs the hub component (the player's InputMonitor) assigned.
	public sealed class Interactor : MonoBehaviour
	{
		[SerializeField] private InputMonitor _input;
		[SerializeField] private float _maxReach = 100.0f;

		private Camera _camera;

		private void Awake()
		{
			Assert.IsNotNull(_input, "Interactor requires the player's InputMonitor assigned");

			GameObject main = GameObject.FindGameObjectWithTag("MainCamera");
			_camera = main == null ? null : main.GetComponent<Camera>();
			Assert.IsNotNull(_camera, "Interactor requires a Camera tagged MainCamera");
		}

		private void Update()
		{
			if (!_input.GetInteractInputDown()) return;

			Vector3 center = new(Screen.width / 2.0f, Screen.height / 2.0f, 0.0f);
			RaycastHit hit;
			if (!Physics.Raycast(_camera.ScreenPointToRay(center), out hit, _maxReach)) return;

			hit.collider.GetComponentInParent<Interactable>()?.Interact();
		}
	}
}