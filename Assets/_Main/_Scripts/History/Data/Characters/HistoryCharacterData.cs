using Characters;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Variables;
using VN;

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
				HistoryCharacter historyCharacter = new HistoryCharacter();
				historyCharacters.Add(historyCharacter);

				historyCharacter.type = character.Data.Type;
				historyCharacter.shortName = character.Data.ShortName;
				historyCharacter.name = character.Data.Name;

				if (character is not GraphicsCharacter graphicsCharacter) continue;

				historyCharacter.priority = graphicsCharacter.HierarchyPriority;
				historyCharacter.isVisible = graphicsCharacter.IsVisible;
				historyCharacter.position = graphicsCharacter.Position;
				historyCharacter.color = graphicsCharacter.LightColor;
				historyCharacter.isHighlighted = graphicsCharacter.IsHighlighted;
				historyCharacter.direction = graphicsCharacter.Direction;
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

		public void Load(CharacterManager characterManager, VNOptionsSO vnOptions)
		{
			List<KeyValuePair<int, string>> characterPriorities = new();
			bool areSorted = true;

			HashSet<string> historyShortNames = new();

			foreach (HistoryCharacter historyCharacter in historyCharacters)
			{
				Character character = characterManager.GetCharacter(historyCharacter.shortName);
				if (character == null) continue;

				// Keep track of all characters present at this point in time
				historyShortNames.Add(historyCharacter.shortName);

				// Update any non-graphic related data
				if (historyCharacter.name != character.Data.Name)
					character.SetName(historyCharacter.name);
				
				if (character is not GraphicsCharacter graphicsCharacter) continue;

				// Update all graphics characters parameters
				LoadGraphicsCharacterHistory(graphicsCharacter, historyCharacter, vnOptions, ref characterPriorities, ref areSorted);
			}

			// Hide any new characters which were not present in history
			foreach (Character character in characterManager.GetCharacters().Values)
			{
				if (historyShortNames.Contains(character.Data.ShortName) || character is not GraphicsCharacter graphicsCharacter) continue;

				if (graphicsCharacter.IsVisible)
					graphicsCharacter.Hide();
			}

			if (!areSorted)
			{
				string[] shortNames = characterPriorities.OrderByDescending(pair => pair.Key).Select(pair => pair.Value).ToArray();
				characterManager.SetPriority(shortNames);
			}
		}

		void LoadGraphicsCharacterHistory(GraphicsCharacter graphicsCharacter, HistoryCharacter historyCharacter,
			VNOptionsSO vnOptions, ref List<KeyValuePair<int, string>> characterPriorities, ref bool areSorted)
		{
			float fadeSpeed = vnOptions.General.SkipTransitionSpeed;

			characterPriorities.Add(new(historyCharacter.priority, historyCharacter.shortName));

			if (historyCharacter.priority != graphicsCharacter.HierarchyPriority)
				areSorted = false;
			if (historyCharacter.isVisible != graphicsCharacter.IsVisible)
				graphicsCharacter.SetVisibility(historyCharacter.isVisible, false, fadeSpeed);
			if (historyCharacter.position != graphicsCharacter.Position)
				graphicsCharacter.SetPosition(historyCharacter.position.x, historyCharacter.position.y, true);
			if (historyCharacter.color != graphicsCharacter.LightColor)
				graphicsCharacter.SetColor(historyCharacter.color, false, fadeSpeed);
			if (historyCharacter.isHighlighted != graphicsCharacter.IsHighlighted)
				graphicsCharacter.SetHighlighted(historyCharacter.isHighlighted, false, fadeSpeed);
			if (historyCharacter.direction != graphicsCharacter.Direction)
				graphicsCharacter.SetDirection(historyCharacter.direction, false, fadeSpeed);

			LoadAnimations(graphicsCharacter.RootAnimator, historyCharacter.animations);

			if (graphicsCharacter is SpriteCharacter spriteChar && historyCharacter.type == CharacterType.Sprite)
			{
				foreach (var historySpriteLayer in historyCharacter.spriteLayers)
				{
					CharacterSpriteLayer spriteLayer = spriteChar.SpriteLayers[historySpriteLayer.layerType];
					if (spriteLayer.SpriteName != historySpriteLayer.spriteName)
						spriteChar.SetSprite(historySpriteLayer.spriteName, historySpriteLayer.layerType, false, fadeSpeed);
				}
			}
			else if (graphicsCharacter is Model3DCharacter model3DChar && historyCharacter.type == CharacterType.Model3D)
			{
				if (historyCharacter.modelExpression != model3DChar.Expression)
					model3DChar.SetExpression(historyCharacter.modelExpression, false, fadeSpeed);
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
