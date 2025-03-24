using UnityEngine;

namespace Characters
{
	public abstract class GraphicsCharacter : Character
	{
		public Color OverlayColor { get; protected set; }

		public virtual Coroutine SetColor(Color color, float transitionSpeed = 0)
		{
			OverlayColor = color;
			return null;
		}
	}
}
