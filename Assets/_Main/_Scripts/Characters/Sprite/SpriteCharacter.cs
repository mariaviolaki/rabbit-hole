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
		SpriteAtlas spriteAtlas;
		Dictionary<SpriteLayerType, CharacterSpriteLayer> spriteLayers;

		protected override IEnumerator Init()
		{
			yield return base.Init();

			spriteLayers = new Dictionary<SpriteLayerType, CharacterSpriteLayer>();

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
					spriteLayers[layerType] = new CharacterSpriteLayer(manager, TransitionHandler, layerType, layerParent);
			}
		}

		public void SetSpriteInstant(SpriteLayerType layerType, string spriteName)
		{
			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return;

			spriteLayers[layerType].SetSpriteInstant(sprite);
		}
		public Coroutine SetSprite(SpriteLayerType layerType, string spriteName, float speed = 0)
		{
			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return null;

			CharacterSpriteLayer layer = spriteLayers[layerType];
			bool isSkipped = layer.IsChangingSprite;
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.FadeTransitionSpeed, isSkipped);

			return layer.SetSprite(sprite, speed);
		}

		public override void FlipInstant()
		{
			manager.StopProcess(ref directionCoroutine);

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.FlipInstant();
			}

			isFacingRight = !isFacingRight;
		}
		protected override IEnumerator FlipDirection(float speed, bool isSkipped)
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

		public override void ChangeBrightnessInstant(bool isHighlighted)
		{
			manager.StopProcess(ref brightnessCoroutine);

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.SetBrightnessInstant(isHighlighted ? LightColor : DarkColor);
			}

			this.isHighlighted = isHighlighted;
		}
		protected override IEnumerator ChangeBrightness(bool isHighlighted, float speed, bool isSkipped)
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

		public override void ChangeColorInstant(Color color)
		{
			manager.StopProcess(ref colorCoroutine);

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.SetColorInstant(color);
			}

			LightColor = color;
		}
		protected override IEnumerator ChangeColor(Color color, float speed, bool isSkipped)
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
			yield return layer.Flip(speed);
		}

		IEnumerator SetLayerImageBrightness(CharacterSpriteLayer layer, Color targetColor, float speed)
		{
			yield return layer.SetBrightness(targetColor, speed);
		}

		IEnumerator ColorLayerImage(CharacterSpriteLayer layer, Color targetColor, float speed)
		{
			yield return layer.SetColor(targetColor, speed);
		}

		Sprite GetSprite(SpriteLayerType layerType, string spriteName)
		{
			if (!spriteLayers.ContainsKey(layerType))
			{
				Debug.LogWarning($"'{layerType}' is not a valid sprite layer for {data.CastName}");
				return null;
			}

			Sprite sprite = spriteAtlas.GetSprite(spriteName);
			if (sprite == null)
			{
				Debug.LogWarning($"'{spriteName}' was not found in {data.CastName}'s sprites.");
				return null;
			}

			return sprite;
		}
	}
}
