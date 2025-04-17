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

		Coroutine spriteCoroutine;

		protected override async Task Init()
		{
			await base.Init();
			
			spriteLayers = new Dictionary<SpriteLayerType, CharacterSpriteLayer>();
			spriteAtlas = await manager.FileManager.LoadCharacterAtlas(data.CastName);

			InitSpriteLayers();
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
					spriteLayers[layer] = new CharacterSpriteLayer(layer, image);
			}
		}

		public void SetSpriteInstant(SpriteLayerType layerType, string spriteName)
		{
			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return;

			spriteLayers[layerType].LayerImage.sprite = sprite;
		}
		public Coroutine SetSprite(SpriteLayerType layerType, string spriteName, float speed = 0)
		{
			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return null;

			spriteCoroutine = manager.StartCoroutine(ChangeSprite(spriteLayers[layerType], sprite, speed));
			return spriteCoroutine;
		}

		public override void FlipInstant()
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				Transform layerTransform = layer.LayerImage.transform;
				layerTransform.localScale = new Vector3(-layerTransform.localScale.x, 1, 1);
			}

			isFacingRight = !isFacingRight;
		}
		protected override IEnumerator FlipDirection(float speed)
		{
			yield return FadeImage(canvasGroup, false, speed);

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				Transform layerTransform = layer.LayerImage.transform;
				layerTransform.localScale = new Vector3(-layerTransform.localScale.x, 1, 1);
			}

			yield return FadeImage(canvasGroup, true, speed);

			isFacingRight = !isFacingRight;
			directionCoroutine = null;
		}

		public override void ChangeBrightnessInstant(bool isHighlighted)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.LayerImage.color = isHighlighted ? LightColor : DarkColor;
			}

			this.isHighlighted = isHighlighted;
		}
		protected override IEnumerator ChangeBrightness(bool isHighlighted, float speed)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.BrightnessCoroutine = manager.StartCoroutine(SetLayerImageBrightness(layer, isHighlighted, speed));
			}

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.BrightnessCoroutine != null)) yield return null;

			this.isHighlighted = isHighlighted;
			brightnessCoroutine = null;
		}

		public override void ChangeColorInstant(Color color)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.LayerImage.color = color;
			}

			LightColor = color;
		}
		protected override IEnumerator ChangeColor(Color color, float speed)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.ColorCoroutine = manager.StartCoroutine(ColorLayerImage(layer, color, speed));
			}

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.ColorCoroutine != null)) yield return null;

			LightColor = color;
			colorCoroutine = null;
		}

		IEnumerator ChangeSprite(CharacterSpriteLayer spriteLayer, Sprite sprite, float speed)
		{
			yield return FadeImage(spriteLayer.LayerCanvasGroup, false, speed);

			spriteLayer.LayerImage.sprite = sprite;

			yield return FadeImage(spriteLayer.LayerCanvasGroup, true, speed);

			spriteCoroutine = null;
		}

		IEnumerator SetLayerImageBrightness(CharacterSpriteLayer layer, bool isHighlighted, float speed)
		{
			yield return SetImageBrightness(layer.LayerImage, isHighlighted, speed);
			layer.BrightnessCoroutine = null;
		}

		IEnumerator ColorLayerImage(CharacterSpriteLayer layer, Color color, float speed)
		{
			yield return ColorImage(layer.LayerImage, color, speed);
			layer.ColorCoroutine = null;
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
