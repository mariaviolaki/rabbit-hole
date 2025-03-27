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

		RectTransform root;
		CanvasGroup canvasGroup;
		Animator animator;
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

			// Load this character's prefab into the scene
			GameObject prefab = await Manager.FileManager.LoadCharacterPrefab(Data.CastName);
			GameObject rootGameObject = UnityEngine.Object.Instantiate(prefab, Manager.Container);

			root = rootGameObject.GetComponent<RectTransform>();
			canvasGroup = root.GetComponent<CanvasGroup>();
			animator = root.GetComponentInChildren<Animator>();
			spriteLayers = new Dictionary<SpriteLayerType, CharacterSpriteLayer>();

			root.name = Data.Name;
			canvasGroup.alpha = 0f;

			spriteAtlas = await Manager.FileManager.LoadCharacterAtlas(Data.CastName);

			InitSpriteLayers();

			Debug.Log($"Created Sprite Character: {Data.Name}");
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
					spriteLayers[layer] = new CharacterSpriteLayer(layer, image, Manager);
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
			if (!IsFacingRight) return null;

			return Flip(speed);
		}

		public override Coroutine FaceRight(float speed = 0)
		{
			if (IsFacingRight) return null;

			return Flip(speed);
		}

		public override Coroutine Flip(float speed = 0)
		{
			Manager.StopProcess(ref directionCoroutine);

			directionCoroutine = Manager.StartCoroutine(FlipDirection(speed));
			return directionCoroutine;
		}

		public override Coroutine Lighten(float speed = 0)
		{
			Manager.StopProcess(ref brightnessCoroutine);
			
			brightnessCoroutine = Manager.StartCoroutine(ChangeBrightness(true, speed));
			return brightnessCoroutine;
		}

		public override Coroutine Darken(float speed = 0)
		{
			Manager.StopProcess(ref brightnessCoroutine);

			brightnessCoroutine = Manager.StartCoroutine(ChangeBrightness(false, speed));
			return brightnessCoroutine;
		}

		public override Coroutine SetColor(Color color, float speed = 0)
		{
			base.SetColor(color);

			Manager.StopProcess(ref colorCoroutine);

			colorCoroutine = Manager.StartCoroutine(ChangeColor(DisplayColor, speed));
			return colorCoroutine;
		}

		public override Coroutine MoveToPosition(Vector2 normalizedPos, float speed)
		{
			Manager.StopProcess(ref moveCoroutine);

			moveCoroutine = Manager.StartCoroutine(MoveCharacter(normalizedPos, speed));
			return moveCoroutine;
		}

		public override Coroutine Show()
		{
			Manager.StopProcess(ref visibilityCoroutine);

			visibilityCoroutine = Manager.StartCoroutine(ChangeVisibility(true));
			return visibilityCoroutine;
		}

		public override Coroutine Hide()
		{
			Manager.StopProcess(ref visibilityCoroutine);

			visibilityCoroutine = Manager.StartCoroutine(ChangeVisibility(false));
			return visibilityCoroutine;
		}

		IEnumerator FlipDirection(float speed)
		{
			foreach (CharacterSpriteLayer layer in spriteLayers.Values)
			{
				if (IsFacingRight)
					layer.FaceLeft(speed);
				else
					layer.FaceRight(speed);
			}

			yield return null;
			while (spriteLayers.Values.Any(layer => layer.IsChangingDirection)) yield return null;

			IsFacingRight = !IsFacingRight;
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

			IsHighlighted = isLightColor;
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

		IEnumerator ChangeVisibility(bool isShowing)
		{
			float targetAlpha = isShowing ? 1f : 0f;
			float visibilityChangeSpeed = isShowing ? Manager.GameOptions.CharacterShowSpeed : Manager.GameOptions.CharacterHideSpeed;

			while (canvasGroup.alpha != targetAlpha)
			{
				canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, visibilityChangeSpeed * Time.deltaTime);
				yield return null;
			}

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
				Debug.LogWarning($"'{layerType}' is not a valid sprite layer for {Data.CastName}");
				return null;
			}

			Sprite sprite = spriteAtlas.GetSprite(spriteName);
			if (sprite == null)
			{
				Debug.LogWarning($"'{spriteName}' was not found in {Data.CastName}'s sprites.");
				return null;
			}

			return sprite;
		}

		Vector2 GetTargetPosition(Vector2 normalizedPos)
		{
			Vector2 parentSize = Manager.Container.rect.size;

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
