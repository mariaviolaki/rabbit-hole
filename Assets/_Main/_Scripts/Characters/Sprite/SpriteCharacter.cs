using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

namespace Characters
{
	public class SpriteCharacter : GraphicsCharacter
	{
		readonly Dictionary<SpriteLayerType, CharacterSpriteLayer> spriteLayers = new();
		readonly Dictionary<string, Sprite> sprites = new(StringComparer.OrdinalIgnoreCase);
		SpriteAtlas spriteAtlas;

		public Dictionary<SpriteLayerType, CharacterSpriteLayer> SpriteLayers => spriteLayers;

		protected override IEnumerator Init()
		{
			yield return base.Init();

			yield return manager.FileManager.LoadCharacterAtlas(data.CastName);
			spriteAtlas = manager.FileManager.GetCharacterAtlas(data.CastName);
			if (spriteAtlas == null) yield break;

			InitSpriteLayers();
		}

		void InitSpriteLayers()
		{
			Transform layerParentContainer = animator.transform.GetChild(0);

			foreach (Transform layerParent in layerParentContainer)
			{
				string layerName = layerParent.name;
				if (Enum.TryParse(layerName, true, out SpriteLayerType layerType))
					spriteLayers[layerType] = new CharacterSpriteLayer(manager, TransitionHandler, layerType, layerParent, isFacingRight);
			}

			// Cache the sprite in a dictionary for case-insensitive lookup
			Sprite[] spriteArray = new Sprite[spriteAtlas.spriteCount];
			spriteAtlas.GetSprites(spriteArray);
			foreach (Sprite sprite in spriteArray)
			{
				string spriteName = sprite.name;
				if (spriteName.EndsWith("(Clone)"))
					spriteName = spriteName.Substring(0, spriteName.Length - "(Clone)".Length);

				if (sprites.ContainsKey(spriteName))
					Debug.LogWarning($"Duplicate sprite name '{spriteName}' found in atlas for {data.CastName}.");

				sprites[spriteName] = sprite;
			}
		}

		public Coroutine SetSprite(SpriteLayerType layerType, string spriteName, bool isImmediate = false, float speed = 0)
		{
			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return null;

			CharacterSpriteLayer layer = spriteLayers[layerType];
			bool isSkipped = layer.IsChangingSprite;
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.FadeTransitionSpeed, isSkipped);

			return layer.SetSprite(sprite, isImmediate, speed);
		}

		public override Coroutine Flip(bool isImmediate = false, float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref directionCoroutine);

			if (isImmediate)
			{
				FlipDirectionImmediate();
				return null;	
			}
			else
			{
				directionCoroutine = manager.StartCoroutine(TransitionDirection(speed, isSkipped));
				return directionCoroutine;
			}
		}

		public override Coroutine Highlight(bool isImmediate = false, float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref brightnessCoroutine);

			if (isImmediate)
			{
				SetBrightnessImmediate(true);
				return null;
			}
			else
			{
				brightnessCoroutine = manager.StartCoroutine(TransitionBrightness(true, speed, isSkipped));
				return brightnessCoroutine;
			}
		}

		public override Coroutine Unhighlight(bool isImmediate = false, float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref brightnessCoroutine);

			if (isImmediate)
			{
				SetBrightnessImmediate(false);
				return null;
			}
			else
			{
				brightnessCoroutine = manager.StartCoroutine(TransitionBrightness(false, speed, isSkipped));
				return brightnessCoroutine;
			}
		}

		public override Coroutine SetColor(Color color, bool isImmediate = false, float speed = 0)
		{
			bool isSkipped = manager.StopProcess(ref colorCoroutine);

			if (isImmediate)
			{
				SetColorImmediate(color);
				return null;
			}
			else
			{
				colorCoroutine = manager.StartCoroutine(TransitionColor(color, speed, isSkipped));
				return colorCoroutine;
			}
		}

		void FlipDirectionImmediate()
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				if (isFacingRight)
					layer.FaceLeft(true);
				else
					layer.FaceRight(true);
			}

			isFacingRight = !isFacingRight;
		}

		void SetBrightnessImmediate(bool isHighlighted)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.SetBrightness(isHighlighted ? LightColor : DarkColor, true);
			}

			this.isHighlighted = isHighlighted;
		}

		void SetColorImmediate(Color color)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.SetColor(color, true);
			}

			LightColor = color;
		}

		Sprite GetSprite(SpriteLayerType layerType, string spriteName)
		{
			if (!spriteLayers.ContainsKey(layerType))
			{
				Debug.LogWarning($"'{layerType}' is not a valid sprite layer for {data.CastName}");
				return null;
			}

			if (!sprites.TryGetValue(spriteName, out Sprite sprite))
			{
				Debug.LogWarning($"'{spriteName}' was not found in {data.CastName}'s sprites.");
				return null;
			}

			return sprite;
		}

		IEnumerator TransitionDirection(float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.FadeTransitionSpeed, isSkipped);

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				manager.StartCoroutine(SetLayerImageDirection(layer, speed));
			}

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingDirection)) yield return null;

			isFacingRight = !isFacingRight;
			directionCoroutine = null;
		}

		IEnumerator TransitionBrightness(bool isHighlighted, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.BrightnessTransitionSpeed, isSkipped);
			Color targetColor = isHighlighted ? LightColor : DarkColor;

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				manager.StartCoroutine(SetLayerImageBrightness(layer, targetColor, speed));
			}

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingBrightness)) yield return null;

			this.isHighlighted = isHighlighted;
			brightnessCoroutine = null;
		}

		IEnumerator TransitionColor(Color color, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.ColorTransitionSpeed, isSkipped);
			Color targetColor = isHighlighted ? color : GetDarkColor(color);

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				manager.StartCoroutine(ColorLayerImage(layer, targetColor, speed));
			}

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingColor)) yield return null;

			LightColor = color;
			colorCoroutine = null;
		}

		IEnumerator SetLayerImageDirection(CharacterSpriteLayer layer, float speed)
		{
			if (isFacingRight)
				yield return layer.FaceLeft(false, speed);
			else
				yield return layer.FaceRight(false, speed);
		}

		IEnumerator SetLayerImageBrightness(CharacterSpriteLayer layer, Color targetColor, float speed)
		{
			yield return layer.SetBrightness(targetColor, false, speed);
		}

		IEnumerator ColorLayerImage(CharacterSpriteLayer layer, Color targetColor, float speed)
		{
			yield return layer.SetColor(targetColor, false, speed);
		}
	}
}
