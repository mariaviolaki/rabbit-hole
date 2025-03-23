using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Characters
{
	public class SpriteCharacter : Character
	{
		const string LayerContainerName = "Layers";
		const float MoveSpeedMultiplier = 100000f;

		RectTransform root;
		CanvasGroup canvasGroup;
		Animator animator;
		SpriteAtlas spriteAtlas;
		Dictionary<SpriteLayerType, CharacterSpriteLayer> spriteLayers;

		Coroutine visibilityProcess;
		Coroutine moveProcess;

		protected override async Task Init()
		{
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

		public void SetSprite(SpriteLayerType layerType, string spriteName)
		{
			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return;

			spriteLayers[layerType].SetSprite(sprite);
		}

		public Coroutine TransitionSprite(SpriteLayerType layerType, string spriteName, float transitionSpeed = 0)
		{
			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return null;

			return spriteLayers[layerType].TransitionSprite(sprite, transitionSpeed);
		}

		public override void SetPosition(Vector2 normalizedPos)
		{
			root.position = GetTargetPosition(normalizedPos);
		}

		public override Coroutine MoveToPosition(Vector2 normalizedPos, float speed)
		{
			Manager.StopProcess(ref moveProcess);

			moveProcess = Manager.StartCoroutine(MoveCharacter(normalizedPos, speed));
			return moveProcess;
		}

		public override Coroutine Show()
		{
			Manager.StopProcess(ref visibilityProcess);

			visibilityProcess = Manager.StartCoroutine(ChangeVisibility(true));
			return visibilityProcess;
		}

		public override Coroutine Hide()
		{
			Manager.StopProcess(ref visibilityProcess);

			visibilityProcess = Manager.StartCoroutine(ChangeVisibility(false));
			return visibilityProcess;
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

			visibilityProcess = null;
		}

		IEnumerator MoveCharacter(Vector2 normalizedPos, float speed)
		{
			Vector2 startPos = root.position;
			Vector2 endPos = GetTargetPosition(normalizedPos);
			float sqrDistance = (endPos - startPos).sqrMagnitude;

			// Move at a constant speed towards the target location
			float distancePercent = 0f;
			while (distancePercent < 1f)
			{
				distancePercent += (speed * MoveSpeedMultiplier * Time.deltaTime) / sqrDistance;
				distancePercent = Mathf.Clamp01(distancePercent);
				root.position = Vector2.Lerp(startPos, endPos, distancePercent);

				yield return null;
			}

			root.position = endPos;
			moveProcess = null;
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
