using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
	/// <summary>All facial expressions in one asset, keyed by expression id — imported from expressions.yaml.</summary>
	[CreateAssetMenu(fileName = "ExpressionSettings", menuName = "Game/Expression Settings")]
	public sealed class ExpressionSettings : ScriptableObject
	{
		// Unity 6 native dictionary serialization — key is the expression id ("expr_default", ...)
		[SerializeField] public Dictionary<string, Expression> Entries = new();
	}

	/// <summary>A facial expression defined by a list of morph target weights.</summary>
	[Serializable]
	public sealed class Expression
	{
		public List<MorphTargetValue> MorphTargets = new();
	}
}