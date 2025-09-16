using Characters;
using Commands;
using System.Collections;
using System.Collections.Generic;
using UI;

namespace Dialogue
{
	public class DialogueSpeakerHandler
	{
		readonly CharacterManager characterManager;
		readonly CommandManager commandManager;
		readonly VisualNovelUI visualNovelUI;
		readonly VNOptionsSO vnOptions;

		public DialogueSpeakerHandler(DialogueManager dialogueManager, VNOptionsSO vnOptions)
		{
			this.characterManager = dialogueManager.Characters;
			this.commandManager = dialogueManager.Commands;
			this.visualNovelUI = dialogueManager.VN.UI;
			this.vnOptions = vnOptions;
		}

		public IEnumerator SetSpeaker(string shortName, SpriteLayerType layerType, string visualName, float xPos, float yPos)
		{
			if (string.IsNullOrWhiteSpace(shortName))
			{
				yield return visualNovelUI.Dialogue.HideSpeaker();
				yield break;
			}

			Character speaker = characterManager.GetCharacter(shortName);
			ChangeSpeakerPosition(speaker, xPos, yPos);
			ChangeSpeakerGraphics(speaker, layerType, visualName);
			HighlightSpeaker(speaker.Data.ShortName);
			yield return SetSpeakerName(speaker.Data);
		}

		public IEnumerator SetSpeakerName(CharacterData characterData)
		{
			yield return visualNovelUI.Dialogue.ShowSpeaker(characterData);
		}

		void ChangeSpeakerPosition(Character character, float xPos, float yPos)
		{
			if (character is not GraphicsCharacter graphicsCharacter || (float.IsNaN(xPos) && float.IsNaN(yPos))) return;

			string commandName = $"{character.Data.ShortName}.SetPosition";
			List<string> argumentList = new();

			if (!float.IsNaN(xPos))
				argumentList.Add($"x = {xPos}");
			if (!float.IsNaN(yPos))
				argumentList.Add($"y = {yPos}");
				
			DialogueCommandArguments arguments = new(argumentList);
			commandManager.Execute(commandName, arguments, false);
		}

		void ChangeSpeakerGraphics(Character character, SpriteLayerType layerType, string visualName)
		{
			if (string.IsNullOrWhiteSpace(visualName)) return;

			if (character is SpriteCharacter)
			{
				string commandName = $"{character.Data.ShortName}.SetSprite";
				List<string> argumentList = new() { visualName, layerType.ToString() };
				DialogueCommandArguments arguments = new(argumentList);

				commandManager.Execute(commandName, arguments, false);
			}
			else if (character is Model3DCharacter)
			{
				string commandName = $"{character.Data.ShortName}.SetExpression";
				List<string> argumentList = new() { visualName };
				DialogueCommandArguments arguments = new(argumentList);

				commandManager.Execute(commandName, arguments, false);
			}
		}

		void HighlightSpeaker(string shortName)
		{
			if (!vnOptions.Characters.HighlightOnSpeak) return;

			foreach (Character character in characterManager.GetCharacters().Values)
			{
				if (character is not GraphicsCharacter graphicsCharacter) continue;

				string characterShortName = graphicsCharacter.Data.ShortName;
				bool isSpeaker = characterShortName == shortName;

				if (isSpeaker && !graphicsCharacter.IsHighlighted)
				{
					string commandName = $"{characterShortName}.Highlight";
					DialogueCommandArguments arguments = new(); // use default arguments

					commandManager.Execute(commandName, arguments, false);
				}
				else if (!isSpeaker && graphicsCharacter.IsHighlighted)
				{
					string commandName = $"{characterShortName}.Unhighlight";
					DialogueCommandArguments arguments = new(); // use default arguments

					commandManager.Execute(commandName, arguments, false);
				}
			}
		}
	}
}
