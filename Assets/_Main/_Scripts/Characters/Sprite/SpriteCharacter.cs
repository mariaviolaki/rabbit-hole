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
		[SerializeField] GameObject spriteLayerPrefab;

		readonly Dictionary<SpriteLayerType, CharacterSpriteLayer> primarySpriteLayers = new();
		readonly Dictionary<SpriteLayerType, CharacterSpriteLayer> secondarySpriteLayers = new();
		readonly Dictionary<string, Sprite> sprites = new(StringComparer.OrdinalIgnoreCase);

		SpriteAtlas spriteAtlas;

		public Dictionary<SpriteLayerType, CharacterSpriteLayer> SpriteLayers => primarySpriteLayers;

		public bool IsTransitioningSprite()
		{
			foreach (CharacterSpriteLayer layer in primarySpriteLayers.Values)
			{
				if (layer.SpriteStatus != TransitionStatus.Completed)
					return true;
			}
			return false;
		}

		protected override void Update()
		{
			base.Update();

			TransitionColor();
		}

		override public IEnumerator Initialize(CharacterManager manager, CharacterData data)
		{
			yield return base.Initialize(manager, data);

			yield return manager.FileManager.LoadCharacterAtlas(data.CastName);
			spriteAtlas = manager.FileManager.GetCharacterAtlas(data.CastName);
			if (spriteAtlas == null) yield break;

			InitSpriteLayers();
			InitSpriteAtlas();
			SetColorImmediate();
		}

		public void SetSprite(string spriteName, SpriteLayerType layerType, bool isImmediate = false, float speed = 0)
		{
			layerType = GetLayerTypeFromInput(layerType);

			Sprite sprite = GetSprite(layerType, spriteName);
			if (sprite == null) return;

			CharacterSpriteLayer primaryLayer = primarySpriteLayers[layerType];
			CharacterSpriteLayer secondaryLayer = secondarySpriteLayers[layerType];

			primaryLayer.SetSprite(sprite, isImmediate, speed);
			secondaryLayer.SetSprite(sprite, isImmediate, speed);
		}

		public void SkipSpriteTransition()
		{
			ForEachLayer(primarySpriteLayers, layer => layer.SkipSpriteTransition());
			ForEachLayer(secondarySpriteLayers, layer => layer.SkipSpriteTransition());
		}

		protected override void SetColorImmediate()
		{
			Color targetColor = GetDisplayColor();
			ForEachLayer(primarySpriteLayers, layer => layer.SetColorImmediate(targetColor));
			ForEachLayer(secondarySpriteLayers, layer => layer.SetColorImmediate(targetColor));
			colorStatus = TransitionStatus.Completed;
		}

		protected override void TransitionColor()
		{
			if (colorStatus == TransitionStatus.Completed) return;

			Color targetColor = GetDisplayColor();
			float speed = colorSpeed * Time.deltaTime;
			ForEachLayer(primarySpriteLayers, layer => layer.TransitionColor(targetColor, speed));
			ForEachLayer(secondarySpriteLayers, layer => layer.TransitionColor(targetColor, speed));

			if (Utilities.AreApproximatelyEqual(primarySpriteLayers.First().Value.DisplayColor, targetColor))
			{
				ForEachLayer(primarySpriteLayers, layer => layer.SetColorImmediate(targetColor));
				ForEachLayer(secondarySpriteLayers, layer => layer.SetColorImmediate(targetColor));
				colorStatus = TransitionStatus.Completed;
			}
		}

		Sprite GetSprite(SpriteLayerType layerType, string spriteName)
		{
			if (!primarySpriteLayers.ContainsKey(layerType))
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

		void ForEachLayer(Dictionary<SpriteLayerType, CharacterSpriteLayer> spriteLayers, Action<CharacterSpriteLayer> action)
		{
			foreach (CharacterSpriteLayer spriteLayer in spriteLayers.Values)
			{
				action(spriteLayer);
			}
		}

		SpriteLayerType GetLayerTypeFromInput(SpriteLayerType layerInput)
		{
			if (layerInput != SpriteLayerType.None) return layerInput;

			// Get the default layer if invalid input was provided
			if (primarySpriteLayers.Count > 1) return SpriteLayerType.Face;
			else if (primarySpriteLayers.Count == 1) return SpriteLayerType.Body;
			else return SpriteLayerType.None;
		}

		public string GetRawSpriteName(string spriteName)
		{
			if (string.IsNullOrWhiteSpace(spriteName)) return string.Empty;

			if (!spriteName.EndsWith("(Clone)")) return spriteName;
			return spriteName.Substring(0, spriteName.Length - "(Clone)".Length);
		}

		void InitSpriteLayers()
		{
			Transform primaryLayerContainer = animator.transform.GetChild(0).GetChild(0);
			Transform secondaryLayerContainer = animator.transform.GetChild(1).GetChild(0);

			foreach (CharacterSpriteLayerData layerData in data.SpriteLayers)
			{
				SpriteLayerType layerType = layerData.LayerType;
				Sprite defaultSprite = layerData.DefaultSprite;

				GameObject primaryLayer = Instantiate(spriteLayerPrefab, primaryLayerContainer, false);
				primarySpriteLayers[layerType] = primaryLayer.GetComponent<CharacterSpriteLayer>();
				primarySpriteLayers[layerType].Initialize(this, layerType, primaryLayer.transform, defaultSprite);

				GameObject secondaryLayer = Instantiate(spriteLayerPrefab, secondaryLayerContainer, false);
				secondarySpriteLayers[layerType] = secondaryLayer.GetComponent<CharacterSpriteLayer>();
				secondarySpriteLayers[layerType].Initialize(this, layerType, secondaryLayer.transform, defaultSprite);
			}
		}

		void InitSpriteAtlas()
		{
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
	}
}
