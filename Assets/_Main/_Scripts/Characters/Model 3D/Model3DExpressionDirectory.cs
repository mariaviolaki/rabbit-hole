using System;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
	[RequireComponent(typeof(SkinnedMeshRenderer))]
	public class Model3DExpressionDirectory : MonoBehaviour
	{
		[SerializeField] Model3DExpression[] expressionData;
		readonly Dictionary<string, SubExpression[]> expressions = new(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, SubExpression[]> Expressions { get { return expressions; } }

		void Awake()
		{
			SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

			// Convert the array to a dictionary for fast lookups
			foreach (Model3DExpression modelExpression in expressionData)
			{
				foreach (SubExpression subExpression in modelExpression.SubExpressions)
				{
					// Cache the index for each expression to avoid looking for it multiple times
					subExpression.Index = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(subExpression.Name);
				}

				expressions.Add(modelExpression.Name, modelExpression.SubExpressions);
			}
		}
	}
}
