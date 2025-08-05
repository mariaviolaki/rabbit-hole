using Characters;
using UI;

namespace Dialogue
{
	public class DialogueSpeakerHandler
	{
		readonly CharacterManager characterManager;
		readonly VisualNovelUI visualNovelUI;

		public DialogueSpeakerHandler(CharacterManager characterManager, VisualNovelUI visualNovelUI)
		{
			this.characterManager = characterManager;
			this.visualNovelUI = visualNovelUI;
		}

		public void SetSpeaker(string shortName, SpriteLayerType layerType, string visualName, float xPos, float yPos)
		{
			if (string.IsNullOrWhiteSpace(shortName))
			{
				visualNovelUI.Dialogue.HideSpeaker();
				return;
			}

			Character character = characterManager.GetCharacter(shortName);

			ChangeSpeakerPosition(character, xPos, yPos);
			ChangeSpeakerGraphics(character, layerType, visualName);
			SetSpeakerName(character.Data);
		}

		public void SetSpeakerName(CharacterData characterData)
		{
			visualNovelUI.Dialogue.ShowSpeaker(characterData);
		}

		void ChangeSpeakerPosition(Character character, float xPos, float yPos)
		{
			if (character is not GraphicsCharacter graphicsCharacter || (float.IsNaN(xPos) && float.IsNaN(yPos))) return;

			graphicsCharacter.SetPosition(xPos, yPos, false);
		}

		void ChangeSpeakerGraphics(Character character, SpriteLayerType layerType, string visualName)
		{
			if (string.IsNullOrWhiteSpace(visualName)) return;

			if (character is SpriteCharacter spriteCharacter)
				spriteCharacter.SetSprite(visualName, layerType);
			else if (character is Model3DCharacter model3DCharacter)
				model3DCharacter.SetExpression(visualName);
		}
	}
}
