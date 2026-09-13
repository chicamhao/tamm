using Game.Input;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Interaction
{
	// Crosshair-ray interaction: every frame finds the hit Interactable (aim highlight via
	// QuickOutline) and on Interact input triggers it. Needs the hub component (the player's
	// InputMonitor) assigned.
	public sealed class Interactor : MonoBehaviour
	{
		[SerializeField] private InputMonitor _input;
		[SerializeField] private float _maxReach = 100.0f;
		[SerializeField] private Color _aimColor = new Color(1.0f, 0.72f, 0.15f, 1.0f);

		private Camera _camera;
		private Interactable _aimed;

		private void Awake()
		{
			Assert.IsNotNull(_input, "Interactor requires the player's InputMonitor assigned");

			GameObject main = GameObject.FindGameObjectWithTag("MainCamera");
			_camera = main == null ? null : main.GetComponent<Camera>();
			Assert.IsNotNull(_camera, "Interactor requires a Camera tagged MainCamera");
		}

		private void Update()
		{
			Vector3 center = new(Screen.width / 2.0f, Screen.height / 2.0f, 0.0f);
			bool pressed = _input.GetInteractInputDown();

			RaycastHit hit;
			if (_camera != null && Physics.Raycast(_camera.ScreenPointToRay(center), out hit, _maxReach))
			{
				Interactable target = hit.collider.GetComponentInParent<Interactable>();
				if (target != null)
				{
					Aim(target);
					if (pressed) target.Interact();
					return;
				}
			}
			ClearAim();
		}

		private void Aim(Interactable target)
		{
			if (target == _aimed) return;
			ClearAim();
			_aimed = target;
			Outline outline = target.GetComponent<Outline>();
			if (outline != null)
			{
				outline.enabled = true;
				outline.OutlineColor = _aimColor;
			}
		}

		private void ClearAim()
		{
			if (_aimed == null) return;
			Outline outline = _aimed.GetComponent<Outline>();
			if (outline != null) outline.enabled = false;
			_aimed = null;
		}
	}
}