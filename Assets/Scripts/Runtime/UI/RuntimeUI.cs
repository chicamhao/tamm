using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Game.UI
{
	// The level's runtime UI panel. Every level scene that draws UI hosts one
	// "RuntimeUI" GameObject: a UIDocument whose source tree lives in
	// Runtime/UI (PlaygroundUI.uxml, RatUI.uxml). UI leaves query this component
	// for their screen subtrees instead of holding scene references to the panel.
	//
	// One instance per level scene. Leaves capture their instance once in Start,
	// never through the static property, so a document loaded later (a level
	// loaded over the core Playground) can't shadow the one they bound to.
	public sealed class RuntimeUI : MonoBehaviour
	{
		[SerializeField] private UIDocument _document;

		public UIDocument Document => _document;
		public VisualElement Root => _document.rootVisualElement;

		/// <summary>
		/// Resolves the panel to bind against. Leaves hold an optional serialized ref
		/// (set in the Inspector when several level documents can be alive at once, e.g.
		/// the Rat level additive over the core Playground); otherwise fall back to the
		/// scene's only document.
		/// </summary>
		public static RuntimeUI Resolve(RuntimeUI assigned)
		{
			return assigned != null ? assigned : FindFirstObjectByType<RuntimeUI>();
		}

		private void Awake()
		{
			if (_document == null)
				_document = GetComponent<UIDocument>();

			Assert.IsNotNull(_document, "RuntimeUI requires a UIDocument on the same GameObject");
		}

		/// <summary>Shortcut for the commonly-accessed elements.</summary>
		public VisualElement Q(string name)
		{
			return _document.rootVisualElement.Q<VisualElement>(name);
		}

		/// <summary>Badly-named back-compat lookup keeping UI source identical across levels.</summary>
		public T Q<T>(string name) where T : VisualElement
		{
			return _document.rootVisualElement.Q<T>(name);
		}
	}
}