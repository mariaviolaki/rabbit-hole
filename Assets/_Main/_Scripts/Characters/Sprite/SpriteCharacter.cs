using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
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
					spriteLayers[layerType] = new CharacterSpriteLayer(this, TransitionHandler, layerType, layerParent, isFacingRight);
			}

			// Cache the sprite in a dictionary for case-insensitive lookup
			Sprite[] spriteArray = new Sprite[spriteAtlas.spriteCount];
			spriteAtlas.GetSprites(spriteArray);
			foreach (Sprite sprite in spriteArray)
			{
				string spriteName = GetRawSpriteName(sprite.name);

				if (sprites.ContainsKey(spriteName))
					Debug.LogWarning($"Duplicate sprite name '{spriteName}' found in atlas for {data.CastName}.");

				sprites[spriteName] = sprite;
			}
		}

		public Coroutine SetSprite(string spriteName, SpriteLayerType layerType, bool isImmediate = false, float speed = 0)
		{
			layerType = GetLayerTypeFromInput(layerType);

			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return null;

			CharacterSpriteLayer layer = spriteLayers[layerType];
			return layer.SetSprite(sprite, isImmediate, speed);
		}

		public override Coroutine Flip(bool isImmediate = false, float speed = 0)
		{
			if (skippedDirectionCoroutine != null) return null;
			bool isSkipped = manager.StopProcess(ref directionCoroutine);

			if (isImmediate)
			{
				FlipDirectionImmediate();
				return null;	
			}
			else if (isSkipped)
			{
				skippedDirectionCoroutine = manager.StartCoroutine(TransitionDirection(speed, isSkipped));
				return skippedDirectionCoroutine;
			}
			else
			{
				directionCoroutine = manager.StartCoroutine(TransitionDirection(speed, isSkipped));
				return directionCoroutine;
			}
		}

		public override Coroutine SetHighlighted(bool isHighlighted, bool isImmediate = false, float speed = 0)
		{
			if (isHighlighted == this.isHighlighted || skippedBrightnessCoroutine != null) return null;
			bool isSkipped = manager.StopProcess(ref brightnessCoroutine);

			if (isImmediate)
			{
				SetBrightnessImmediate(isHighlighted);
				return null;
			}
			else if (isSkipped)
			{
				skippedBrightnessCoroutine = manager.StartCoroutine(TransitionBrightness(isHighlighted, speed, isSkipped));
				return skippedBrightnessCoroutine;
			}
			else
			{
				brightnessCoroutine = manager.StartCoroutine(TransitionBrightness(isHighlighted, speed, isSkipped));
				return brightnessCoroutine;
			}
		}

		public override Coroutine SetColor(Color color, bool isImmediate = false, float speed = 0)
		{
			if (color == LightColor || skippedColorCoroutine != null) return null;
			bool isSkipped = manager.StopProcess(ref colorCoroutine);

			if (isImmediate)
			{
				SetColorImmediate(color);
				return null;
			}
			else if (isSkipped)
			{
				skippedColorCoroutine = manager.StartCoroutine(TransitionColor(color, speed, isSkipped));
				return skippedColorCoroutine;
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
			ForEachLayer(layer => layer.SetBrightness(isHighlighted ? LightColor : DarkColor, true));
			this.isHighlighted = isHighlighted;
		}

		void SetColorImmediate(Color color)
		{
			ForEachLayer(layer => layer.SetColor(color, true));
			LightColor = color;
		}

		IEnumerator TransitionDirection(float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.TransitionSpeed, isSkipped);

			ForEachLayer(layer => manager.StartCoroutine(SetLayerImageDirection(layer, speed)));

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingDirection)) yield return null;

			isFacingRight = !isFacingRight;
			if (isSkipped) skippedDirectionCoroutine = null;
			else directionCoroutine = null;
		}

		IEnumerator TransitionBrightness(bool isHighlighted, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.TransitionSpeed, isSkipped);
			Color targetColor = isHighlighted ? LightColor : DarkColor;

			ForEachLayer(layer => manager.StartCoroutine(SetLayerImageBrightness(layer, targetColor, speed)));

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingBrightness)) yield return null;

			this.isHighlighted = isHighlighted;
			if (isSkipped) skippedBrightnessCoroutine = null;
			else brightnessCoroutine = null;
		}

		IEnumerator TransitionColor(Color color, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.TransitionSpeed, isSkipped);
			Color targetColor = isHighlighted ? color : GetDarkColor(color);

			ForEachLayer(layer => manager.StartCoroutine(ColorLayerImage(layer, targetColor, speed)));

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingColor)) yield return null;

			LightColor = color;
			if (isSkipped) skippedColorCoroutine = null;
			else colorCoroutine = null;
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

		void ForEachLayer(Action<CharacterSpriteLayer> action)
		{
			foreach (CharacterSpriteLayer spriteLayer in spriteLayers.Values)
			{
				action(spriteLayer);
			}
		}

		SpriteLayerType GetLayerTypeFromInput(SpriteLayerType layerInput)
		{
			// Get the default layer if invalid input was provided
			if (spriteLayers.Count == 1) return SpriteLayerType.None;
			else if (layerInput == SpriteLayerType.None) return SpriteLayerType.Face;
			else return layerInput;
		}

		public string GetRawSpriteName(string spriteName)
		{
			if (string.IsNullOrWhiteSpace(spriteName)) return string.Empty;

			if (!spriteName.EndsWith("(Clone)")) return spriteName;
			return spriteName.Substring(0, spriteName.Length - "(Clone)".Length);
		}
	}
}
