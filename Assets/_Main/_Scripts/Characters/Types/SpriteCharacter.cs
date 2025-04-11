using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Characters
{
	public class SpriteCharacter : GraphicsCharacter
	{
		const string LayerContainerName = "Layers";

		SpriteAtlas spriteAtlas;
		Dictionary<SpriteLayerType, CharacterSpriteLayer> spriteLayers;

		Coroutine colorCoroutine;
		Coroutine brightnessCoroutine;
		Coroutine directionCoroutine;

		protected override async Task Init()
		{
			await base.Init();
			
			spriteLayers = new Dictionary<SpriteLayerType, CharacterSpriteLayer>();
			spriteAtlas = await manager.FileManager.LoadCharacterAtlas(data.CastName);

			InitSpriteLayers();

			Debug.Log($"Created Sprite Character: {data.Name}");
		}

		void InitSpriteLayers()
		{
			Transform layerParentContainer = animator.transform.Find(LayerContainerName);

			foreach (Transform layerParent in layerParentContainer)
			{
				Image image = layerParent.GetComponentInChildren<Image>();
				if (image == null) continue;

				string layerName = layerParent.name;
				if (Enum.TryParse(layerName, true, out SpriteLayerType layer))
					spriteLayers[layer] = new CharacterSpriteLayer(layer, image, manager);
			}
		}

		public Coroutine SetSprite(SpriteLayerType layerType, string spriteName, float speed = 0)
		{
			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return null;

			return spriteLayers[layerType].SetSprite(sprite, speed);
		}

		public override Coroutine FaceLeft(float speed = 0)
		{
			if (!isFacingRight) return null;

			return Flip(speed);
		}

		public override Coroutine FaceRight(float speed = 0)
		{
			if (isFacingRight) return null;

			return Flip(speed);
		}

		public override Coroutine Flip(float speed = 0)
		{
			manager.StopProcess(ref directionCoroutine);

			directionCoroutine = manager.StartCoroutine(FlipDirection(speed));
			return directionCoroutine;
		}

		public override Coroutine Lighten(float speed = 0)
		{
			manager.StopProcess(ref brightnessCoroutine);
			
			brightnessCoroutine = manager.StartCoroutine(ChangeBrightness(true, speed));
			return brightnessCoroutine;
		}

		public override Coroutine Darken(float speed = 0)
		{
			manager.StopProcess(ref brightnessCoroutine);

			brightnessCoroutine = manager.StartCoroutine(ChangeBrightness(false, speed));
			return brightnessCoroutine;
		}

		public override Coroutine SetColor(Color color, float speed = 0)
		{
			base.SetColor(color);

			manager.StopProcess(ref colorCoroutine);

			colorCoroutine = manager.StartCoroutine(ChangeColor(DisplayColor, speed));
			return colorCoroutine;
		}

		IEnumerator FlipDirection(float speed)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				if (isFacingRight)
					layer.FaceLeft(speed);
				else
					layer.FaceRight(speed);
			}

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingDirection)) yield return null;

			isFacingRight = !isFacingRight;
			directionCoroutine = null;
		}

		IEnumerator ChangeBrightness(bool isLightColor, float speed)
		{
			Color targetColor = isLightColor ? LightColor : DarkColor;

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.SetColor(targetColor, speed);
			}

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingColor)) yield return null;

			isHighlighted = isLightColor;
			brightnessCoroutine = null;
		}

		IEnumerator ChangeColor(Color color, float transitionSpeed)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.SetColor(color, transitionSpeed);
			}

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingColor)) yield return null;

			colorCoroutine = null;
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
