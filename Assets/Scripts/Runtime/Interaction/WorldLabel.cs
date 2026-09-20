using Game.Content;
using Game.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Game.Interaction
{
	// Screen-space label for an Interactable (replaces WorldTag's TMP billboard).
	// Sits on the same GameObject as the Interactable; in Start it binds to the
	// level's shared RuntimeUI panel, appends a Label to the WorldLabels layer, and
	// from LateUpdate projects the owner's world position onto that layer, hiding
	// while the point is behind the camera. Same projection math as the rat badge.
	public sealed class WorldLabel : MonoBehaviour
	{
		[SerializeField] private CardSettings _cards; // display names; unknown ids show raw
		[SerializeField] private float _heightOffset = 0.8f;
		[SerializeField] private RuntimeUI _runtimeUI; // set when additive scenes host several panels

		private RuntimeUI _ui;
		private Label _element;
		private Camera _camera;

		private void Start()
		{
			Interactable owner = GetComponent<Interactable>();
			Assert.IsNotNull(owner, "WorldLabel requires an Interactable on the same object");

			_ui = RuntimeUI.Resolve(_runtimeUI);
			Assert.IsNotNull(_ui, "WorldLabel requires a RuntimeUI in the scene");

			VisualElement labels = _ui.Q("WorldLabels");
			Assert.IsNotNull(labels, "WorldLabel requires a WorldLabels layer in the runtime UI");

			_camera = Camera.main;

			Label element = new Label(DisplayName(owner.Id));
			element.AddToClassList("world-label");
			labels.Add(element);
			_element = element;
		}

		private void LateUpdate()
		{
			if (_element == null || _camera == null)
				return;

			Vector3 screen = _camera.WorldToScreenPoint(transform.position + Vector3.up * _heightOffset);

			// Point behind the camera projects mirrored; drop the label entirely.
			if (screen.z < 0f)
			{
				_element.style.display = DisplayStyle.None;
				return;
			}

			// Panel-logical space, matching RatHud.PlacePrompt: WorldToScreenPoint
			// reports bottom-left origin, UITK lays out top-left.
			float logicalH = ScreenHeight();
			_element.style.display = DisplayStyle.Flex;
			_element.style.translate = new Translate(screen.x, logicalH - screen.y);
		}

		private float ScreenHeight()
		{
			Rect content = _ui.Root.contentRect;
			return content.height > 0f ? content.height : 900f;
		}

		private string DisplayName(string id)
		{
			if (_cards != null && _cards.Entries.TryGetValue(id, out Card card) && card.DisplayName.Length > 0)
				return card.DisplayName;
			return id;
		}
	}
}