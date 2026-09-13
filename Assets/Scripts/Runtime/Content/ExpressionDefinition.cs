using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
	/// <summary>ScriptableObject defining a facial expression via a list of morph target values.</summary>
	[CreateAssetMenu(fileName = "ExpressionDefinition", menuName = "ScriptableObjects/ExpressionDefinition", order = 1)]
	public sealed class ExpressionDefinition : ScriptableObject
	{
		public List<MorphTargetValue> MorphTargets = new();
	}
}