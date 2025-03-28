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
		const float MoveSpeedMultiplier = 100f;

		CanvasGroup canvasGroup;
		SpriteAtlas spriteAtlas;
		Dictionary<SpriteLayerType, CharacterSpriteLayer> spriteLayers;

		Coroutine colorCoroutine;
		Coroutine brightnessCoroutine;
		Coroutine visibilityCoroutine;
		Coroutine moveCoroutine;
		Coroutine directionCoroutine;

		protected override async Task Init()
		{
			await base.Init();
			
			canvasGroup = root.GetComponent<CanvasGroup>();
			spriteLayers = new Dictionary<SpriteLayerType, CharacterSpriteLayer>();

			root.name = data.Name;
			canvasGroup.alpha = 0f;

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

		public override void SetPosition(Vector2 normalizedPos)
		{
			root.position = GetTargetPosition(normalizedPos);
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

		public override Coroutine MoveToPosition(Vector2 normalizedPos, float speed)
		{
			manager.StopProcess(ref moveCoroutine);

			moveCoroutine = manager.StartCoroutine(MoveCharacter(normalizedPos, speed));
			return moveCoroutine;
		}

		public override Coroutine Show()
		{
			manager.StopProcess(ref visibilityCoroutine);

			visibilityCoroutine = manager.StartCoroutine(ChangeVisibility(true));
			return visibilityCoroutine;
		}

		public override Coroutine Hide()
		{
			manager.StopProcess(ref visibilityCoroutine);

			visibilityCoroutine = manager.StartCoroutine(ChangeVisibility(false));
			return visibilityCoroutine;
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

		IEnumerator ChangeVisibility(bool isVisible)
		{
			float targetAlpha = isVisible ? 1f : 0f;
			float visibilityChangeSpeed = isVisible ? manager.GameOptions.CharacterShowSpeed : manager.GameOptions.CharacterHideSpeed;

			while (canvasGroup.alpha != targetAlpha)
			{
				canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, visibilityChangeSpeed * Time.deltaTime);
				yield return null;
			}

			this.isVisible = isVisible;
			visibilityCoroutine = null;
		}

		IEnumerator MoveCharacter(Vector2 normalizedPos, float speed)
		{
			Vector2 startPos = root.position;
			Vector2 endPos = GetTargetPosition(normalizedPos);
			float distance = Vector2.Distance(startPos, endPos);

			float distancePercent = 0f;
			while (distancePercent < 1f)
			{
				// Move at the same speed regardless of distance
				distancePercent += (speed * MoveSpeedMultiplier * Time.deltaTime) / distance;
				distancePercent = Mathf.Clamp01(distancePercent);
				// Move smoothly towards the start and end
				float smoothDistance = Mathf.SmoothStep(0f, 1f, distancePercent);
				root.position = Vector2.Lerp(startPos, endPos, smoothDistance);

				yield return null;
			}

			root.position = endPos;
			moveCoroutine = null;
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

		Vector2 GetTargetPosition(Vector2 normalizedPos)
		{
			Vector2 parentSize = manager.Container.rect.size;

			Vector2 imageOffset = (root.pivot * root.rect.size);
			Vector2 minPos = Vector2.zero + imageOffset;
			Vector2 maxPos = parentSize - imageOffset;

			Vector2 targetPos = normalizedPos * parentSize;
			float clampedX = Mathf.Clamp(targetPos.x, minPos.x, maxPos.x);
			float clampedY = Mathf.Clamp(targetPos.y, minPos.y, maxPos.y);

			return new Vector2(clampedX, clampedY);
		}
	}
}
