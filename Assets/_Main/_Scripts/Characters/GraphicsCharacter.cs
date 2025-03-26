using UnityEngine;

namespace Characters
{
	public abstract class GraphicsCharacter : Character
	{
		protected bool IsHighlighted { get; set; } = true;
		protected Color DisplayColor { get { return IsHighlighted ? LightColor : DarkColor; } }
		protected Color LightColor { get; private set; } = Color.white;
		protected Color DarkColor
		{
			get
			{
				return new Color(LightColor.r * Manager.GameOptions.DarkenBrightness,
					LightColor.g * Manager.GameOptions.DarkenBrightness,
					LightColor.b * Manager.GameOptions.DarkenBrightness,
					LightColor.a);
			}
		}

		public abstract Coroutine Lighten(float speed = 0);
		public abstract Coroutine Darken(float speed = 0);

		public virtual Coroutine SetColor(Color color, float speed = 0)
		{
			LightColor = color;
			return null;
		}
	}
}
