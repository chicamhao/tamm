using Game.Content;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Interaction
{
	// 3D label floating above an Interactable, billboarded to the MainCamera.
	// Sits on the same GameObject as the Interactable; the label is a TMPro
	// world-space text child (font assigned in the editor — one-time scene work).
	public sealed class WorldTag : MonoBehaviour
	{
		[SerializeField] private TMP_Text _label;
		[SerializeField] private CardSettings _cards; // display names; unknown ids show raw
		[SerializeField] private float _heightOffset = 2.5f;

		private Camera _camera;

		private void Start()
		{
			Interactable owner = GetComponent<Interactable>();
			Assert.IsNotNull(owner, "WorldTag requires an Interactable on the same object");
			Assert.IsNotNull(_label, "WorldTag requires a TMP label child assigned");

			GameObject main = GameObject.FindGameObjectWithTag("MainCamera");
			_camera = main == null ? null : main.GetComponent<Camera>();

			_label.transform.localPosition = new Vector3(0, _heightOffset, 0);
			_label.text = DisplayName(owner.Id);
		}

		private void LateUpdate()
		{
			if (_label == null || _camera == null) return;
			Transform t = _camera.transform;
			// ponytail: camera-forward billboard; fine for tags, revisit if tags need to read in mirrors.
			_label.transform.rotation = Quaternion.LookRotation(t.forward, t.up);
		}

		private string DisplayName(string id)
		{
			if (_cards != null && _cards.Entries.TryGetValue(id, out Card card) && card.DisplayName.Length > 0)
				return card.DisplayName;
			return id;
		}
	}
}