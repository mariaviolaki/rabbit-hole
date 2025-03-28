using System.Threading.Tasks;
using UnityEngine;

namespace Characters
{
	public abstract class GraphicsCharacter : Character
	{
		protected Animator animator;
		protected bool isFacingRight;
		protected bool isHighlighted = true;

		protected Color DisplayColor { get { return isHighlighted ? LightColor : DarkColor; } }
		protected Color LightColor { get; private set; } = Color.white;
		protected Color DarkColor
		{
			get
			{
				return new Color(LightColor.r * manager.GameOptions.DarkenBrightness,
					LightColor.g * manager.GameOptions.DarkenBrightness,
					LightColor.b * manager.GameOptions.DarkenBrightness,
					LightColor.a);
			}
		}

		protected async override Task Init()
		{
			// Load this character's prefab into the scene
			GameObject prefab = await manager.FileManager.LoadCharacterPrefab(data.CastName);
			GameObject rootGameObject = Object.Instantiate(prefab, manager.Container);

			root = rootGameObject.GetComponent<RectTransform>();
			animator = root.GetComponentInChildren<Animator>();
			isFacingRight = manager.GameOptions.AreSpritesFacingRight;
		}

		public abstract Coroutine Flip(float speed = 0);
		public abstract Coroutine FaceLeft(float speed = 0);
		public abstract Coroutine FaceRight(float speed = 0);
		public abstract Coroutine Lighten(float speed = 0);
		public abstract Coroutine Darken(float speed = 0);

		public virtual Coroutine SetColor(Color color, float speed = 0)
		{
			LightColor = color;
			return null;
		}

		public void SetPriority(int index)
		{
			if (!isVisible) return;

			manager.SetPriority(data.Name, index);
		}

		public void Animate(string animationName)
		{
			animator.SetTrigger(animationName);
		}

		public void Animate(string animationName, bool isPlaying)
		{
			animator.SetBool(animationName, isPlaying);
		}
	}
}
