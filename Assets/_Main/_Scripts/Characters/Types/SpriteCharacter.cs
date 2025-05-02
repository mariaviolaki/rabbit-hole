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
					spriteLayers[layer] = new CharacterSpriteLayer(layer, image, manager);
			}
		}

		public void SetSpriteInstant(SpriteLayerType layerType, string spriteName)
		{
			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return;

			manager.StopProcess(ref spriteCoroutine);
			spriteLayers[layerType].LayerImage.sprite = sprite;
		}
		public Coroutine SetSprite(SpriteLayerType layerType, string spriteName, float speed = 0)
		{
			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return null;

			bool isSkipped = manager.StopProcess(ref spriteCoroutine);

			spriteCoroutine = manager.StartCoroutine(ChangeSprite(spriteLayers[layerType], sprite, speed, isSkipped));
			return spriteCoroutine;
		}

		public override void FlipInstant()
		{
			manager.StopProcess(ref directionCoroutine);

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				Transform layerTransform = layer.LayerImage.transform;
				layerTransform.localScale = new Vector3(-layerTransform.localScale.x, 1, 1);
			}

			isFacingRight = !isFacingRight;
		}
		protected override IEnumerator FlipDirection(float speed, bool isSkipped)
		{
			yield return FadeImage(canvasGroup, false, speed, isSkipped);

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				Transform layerTransform = layer.LayerImage.transform;
				layerTransform.localScale = new Vector3(-layerTransform.localScale.x, 1, 1);
			}

			yield return FadeImage(canvasGroup, true, speed, isSkipped);

			isFacingRight = !isFacingRight;
			directionCoroutine = null;
		}

		public override void ChangeBrightnessInstant(bool isHighlighted)
		{
			manager.StopProcess(ref brightnessCoroutine);

			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				layer.StopBrightnessCoroutine();
				layer.LayerImage.color = isHighlighted ? LightColor : DarkColor;
			}

			this.isHighlighted = isHighlighted;
		}
		protected override IEnumerator ChangeBrightness(bool isHighlighted, float speed, bool isSkipped)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				Coroutine brightnessCoroutine = manager.StartCoroutine(SetLayerImageBrightness(layer, isHighlighted, speed, isSkipped));
				layer.SetBrightnessCoroutine(brightnessCoroutine);
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
				layer.StopColorCoroutine();
				layer.LayerImage.color = color;
			}

			LightColor = color;
		}
		protected override IEnumerator ChangeColor(Color color, float speed, bool isSkipped)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				Coroutine colorCoroutine = manager.StartCoroutine(ColorLayerImage(layer, color, speed, isSkipped));
				layer.SetColorCoroutine(colorCoroutine);
			}

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingColor)) yield return null;

			LightColor = color;
			colorCoroutine = null;
		}

		IEnumerator ChangeSprite(CharacterSpriteLayer spriteLayer, Sprite sprite, float speed, bool isSkipped)
		{
			yield return FadeImage(spriteLayer.LayerCanvasGroup, false, speed, isSkipped);

			spriteLayer.LayerImage.sprite = sprite;

			yield return FadeImage(spriteLayer.LayerCanvasGroup, true, speed, isSkipped);

			spriteCoroutine = null;
		}

		IEnumerator SetLayerImageBrightness(CharacterSpriteLayer layer, bool isHighlighted, float speed, bool isSkipped)
		{
			yield return SetImageBrightness(layer.LayerImage, isHighlighted, speed, isSkipped);
			layer.StopBrightnessCoroutine();
		}

		IEnumerator ColorLayerImage(CharacterSpriteLayer layer, Color color, float speed, bool isSkipped)
		{
			yield return ColorImage(layer.LayerImage, color, speed, isSkipped);
			layer.StopColorCoroutine();
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
