using UnityEngine;

namespace Characters
{
	[System.Serializable]
	public class Model3DExpression
	{
		[SerializeField] string name;
		[SerializeField] SubExpression[] subExpressions;

		public string Name { get { return name; } }
		public SubExpression[] SubExpressions { get { return subExpressions; } }
	}

	[System.Serializable]
	public class SubExpression
	{
		[SerializeField] string name;
		[SerializeField] float weight;
		int index;

		public string Name { get { return name; } }
		public float Weight { get { return weight; } }
		public int Index { get { return index; } set { index = value; } }
	}
}
