using Characters;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Variables;

namespace History
{
	[System.Serializable]
	public class HistoryCharacterData
	{
		[SerializeField] List<HistoryCharacter> historyCharacters = new();

		public HistoryCharacterData(CharacterManager characterManager)
		{
			Dictionary<string, Character> characters = characterManager.GetCharacters();
			foreach (Character character in characters.Values)
			{
				if (!character.IsVisible || character is not GraphicsCharacter graphicsCharacter) continue;

				HistoryCharacter historyCharacter = new HistoryCharacter();
				historyCharacters.Add(historyCharacter);

				historyCharacter.type = graphicsCharacter.Data.Type;
				historyCharacter.shortName = graphicsCharacter.Data.ShortName;
				historyCharacter.name = graphicsCharacter.Data.Name;
				historyCharacter.displayName = graphicsCharacter.Data.DisplayName;

				historyCharacter.priority = graphicsCharacter.HierarchyPriority;
				historyCharacter.isVisible = graphicsCharacter.IsVisible;
				historyCharacter.position = graphicsCharacter.Position;
				historyCharacter.color = graphicsCharacter.LightColor;
				historyCharacter.isHighlighted = graphicsCharacter.IsHighlighted;
				historyCharacter.isFacingRight = graphicsCharacter.IsFacingRight;
				historyCharacter.animations = GetHistoryAnimations(graphicsCharacter.RootAnimator);

				if (graphicsCharacter is SpriteCharacter spriteCharacter)
				{
					foreach (CharacterSpriteLayer layer in spriteCharacter.SpriteLayers.Values)
					{
						HistorySpriteLayer layerData = new(layer.LayerType, layer.SpriteName);
						historyCharacter.spriteLayers.Add(layerData);
					}
				}
				else if (graphicsCharacter is Model3DCharacter model3DCharacter)
				{
					historyCharacter.modelExpression = model3DCharacter.Expression;
				}
			}
		}

		public IEnumerator Load(CharacterManager characterManager, GameOptionsSO gameOptions)
		{
			float fadeSpeed = gameOptions.General.SkipTransitionSpeed;

			List<KeyValuePair<int, string>> characterPriorities = new();
			bool areSorted = true;

			foreach (HistoryCharacter historyCharacter in historyCharacters)
			{
				GraphicsCharacter character = characterManager.GetCharacter(historyCharacter.shortName) as GraphicsCharacter;
				if (character == null) continue;

				characterPriorities.Add(new(historyCharacter.priority, historyCharacter.shortName));

				if (historyCharacter.priority != character.HierarchyPriority)
					areSorted = false;
				if (historyCharacter.isVisible != character.IsVisible)
					yield return character.SetVisibility(historyCharacter.isVisible, false, fadeSpeed);
				if (historyCharacter.position != character.Position)
					yield return character.SetPosition(historyCharacter.position.x, historyCharacter.position.y, false, fadeSpeed);
				if (historyCharacter.color != character.LightColor)
					yield return character.SetColor(historyCharacter.color, false, fadeSpeed);
				if (historyCharacter.isHighlighted != character.IsHighlighted)
					yield return character.SetHighlighted(historyCharacter.isHighlighted, false, fadeSpeed);
				if (historyCharacter.isFacingRight != character.IsFacingRight)
					yield return character.Flip(false, fadeSpeed);
				if (historyCharacter.name != character.Data.Name)
					character.SetName(historyCharacter.name);
				if (historyCharacter.displayName != character.Data.DisplayName)
					character.SetDisplayName(historyCharacter.displayName);

				LoadAnimations(character.RootAnimator, historyCharacter.animations);

				if (character is SpriteCharacter spriteChar && historyCharacter.type == CharacterType.Sprite)
				{
					foreach (var historySpriteLayer in historyCharacter.spriteLayers)
					{
							CharacterSpriteLayer spriteLayer = spriteChar.SpriteLayers[historySpriteLayer.layerType];
						if (spriteLayer.SpriteName != historySpriteLayer.spriteName)
							yield return spriteChar.SetSprite(historySpriteLayer.spriteName, historySpriteLayer.layerType, false, fadeSpeed);
					}
				}
				else if (character is Model3DCharacter model3DChar && historyCharacter.type == CharacterType.Model3D)
				{
					if (historyCharacter.modelExpression != model3DChar.Expression)
						yield return model3DChar.SetExpression(historyCharacter.modelExpression, false, fadeSpeed);
				}
			}

			if (!areSorted)
			{
				string[] shortNames = characterPriorities.OrderByDescending(pair => pair.Key).Select(pair => pair.Value).ToArray();
				characterManager.SetPriority(shortNames);
			}
		}

		List<HistoryAnimationData> GetHistoryAnimations(Animator animator)
		{
			List<HistoryAnimationData> animations = new();

			foreach (AnimatorControllerParameter animatorParameter in animator.parameters)
			{
				DataTypeEnum dataType = Utilities.GetDataTypeEnum(animatorParameter.type);
				if (dataType == DataTypeEnum.None) continue;

				HistoryAnimationData historyAnimation = new()
				{
					type = dataType,
					name = animatorParameter.name,
					value = GetAnimatorParameterValue(animator, animatorParameter.name, animatorParameter.type)
				};
				animations.Add(historyAnimation);
			}

			return animations;
		}

		void LoadAnimations(Animator animator, List<HistoryAnimationData> historyAnimations)
		{
			foreach (HistoryAnimationData historyAnimation in historyAnimations)
			{
				string value = historyAnimation.value;
				switch (historyAnimation.type)
				{
					case DataTypeEnum.Bool:
						animator.SetBool(historyAnimation.name, bool.Parse(value));
						break;
					case DataTypeEnum.Int:
						animator.SetInteger(historyAnimation.name, int.Parse(value));
						break;
					case DataTypeEnum.Float:
						animator.SetFloat(historyAnimation.name, float.Parse(value));
						break;
				}
			}
		}

		string GetAnimatorParameterValue(Animator animator, string name, AnimatorControllerParameterType type)
		{
			switch (type)
			{
				case AnimatorControllerParameterType.Bool:
					return animator.GetBool(name).ToString();
				case AnimatorControllerParameterType.Int:
					return animator.GetInteger(name).ToString();
				case AnimatorControllerParameterType.Float:
					return animator.GetFloat(name).ToString();
				default:
					return null;
			}
		}
	}
}
