using System;
using System.Collections;
using System.Threading.Tasks;
using Characters;
using UnityEngine;

namespace Commands
{
	public class CharactersCMD : DialogueCommand
	{
		static CharacterManager characterManager;
		public new static readonly string[] unskippableProcesses = new string[] { "CreateCharacters", "CreateCharacter" };

		public new static void Register(CommandManager commandManager)
		{
			characterManager = commandManager.GetCharacterManager();

			CommandDirectory mainDir = commandManager.GetDirectory(CommandManager.MainDirectoryName);
			CommandDirectory charactersDir = commandManager.GetDirectory(CommandManager.CharacterDirectoryName);
			CommandDirectory graphicsCharactersDir = commandManager.GetDirectory(CommandManager.GraphicsCharacterDirectoryName);
			CommandDirectory spriteCharactersDir = commandManager.GetDirectory(CommandManager.SpriteCharacterDirectoryName);
			CommandDirectory model3DCharactersDir = commandManager.GetDirectory(CommandManager.Model3DCharacterDirectoryName);

			// All Characters (cannot be called on specific ones)
			mainDir.AddCommand("CreateCharacters", new Func<string[], IEnumerator>(CreateCharacters));
			mainDir.AddCommand("CreateCharacter", new Func<string[], IEnumerator>(CreateCharacter));

			// All Characters
			charactersDir.AddCommand("SetName", new Action<string[]>(SetName));
			charactersDir.AddCommand("SetDisplayName", new Action<string[]>(SetDisplayName));

			// Graphics Characters
			graphicsCharactersDir.AddCommand("SetPriority", new Action<string[]>(SetPriority));
			graphicsCharactersDir.AddCommand("SetAnimation", new Action<string[]>(SetAnimation));
			graphicsCharactersDir.AddCommand("ShowInstant", new Action<string[]>(ShowInstant));
			graphicsCharactersDir.AddCommand("Show", new Func<string[], IEnumerator>(Show), new Func<string[], IEnumerator>(Show));
			graphicsCharactersDir.AddCommand("HideInstant", new Action<string[]>(HideInstant));
			graphicsCharactersDir.AddCommand("Hide", new Func<string[], IEnumerator>(Hide), new Func<string[], IEnumerator>(Hide));
			graphicsCharactersDir.AddCommand("SetPosInstant", new Action<string[]>(SetPosInstant));
			graphicsCharactersDir.AddCommand("SetPos", new Func<string[], IEnumerator>(SetPos), new Func<string[], IEnumerator>(SetPos));
			graphicsCharactersDir.AddCommand("SetPosXInstant", new Action<string[]>(SetPosXInstant));
			graphicsCharactersDir.AddCommand("SetPosX", new Func<string[], IEnumerator>(SetPosX), new Func<string[], IEnumerator>(SetPosX));
			graphicsCharactersDir.AddCommand("SetPosYInstant", new Action<string[]>(SetPosYInstant));
			graphicsCharactersDir.AddCommand("SetPosY", new Func<string[], IEnumerator>(SetPosY), new Func<string[], IEnumerator>(SetPosY));
			graphicsCharactersDir.AddCommand("FlipInstant", new Action<string[]>(FlipInstant));
			graphicsCharactersDir.AddCommand("Flip", new Func<string[], IEnumerator>(Flip), new Func<string[], IEnumerator>(Flip));
			graphicsCharactersDir.AddCommand("FaceLeftInstant", new Action<string[]>(FaceLeftInstant));
			graphicsCharactersDir.AddCommand("FaceLeft", new Func<string[], IEnumerator>(FaceLeft), new Func<string[], IEnumerator>(FaceLeft));
			graphicsCharactersDir.AddCommand("FaceRightInstant", new Action<string[]>(FaceRightInstant));
			graphicsCharactersDir.AddCommand("FaceRight", new Func<string[], IEnumerator>(FaceRight), new Func<string[], IEnumerator>(FaceRight));
			graphicsCharactersDir.AddCommand("HighlightInstant", new Action<string[]>(HighlightInstant));
			graphicsCharactersDir.AddCommand("Highlight", new Func<string[], IEnumerator>(Highlight), new Func<string[], IEnumerator>(Highlight));
			graphicsCharactersDir.AddCommand("UnhighlightInstant", new Action<string[]>(UnhighlightInstant));
			graphicsCharactersDir.AddCommand("Unhighlight", new Func<string[], IEnumerator>(Unhighlight), new Func<string[], IEnumerator>(Unhighlight));
			graphicsCharactersDir.AddCommand("SetColorInstant", new Action<string[]>(SetColorInstant));
			graphicsCharactersDir.AddCommand("SetColor", new Func<string[], IEnumerator>(SetColor), new Func<string[], IEnumerator>(SetColor));

			// Sprite Characters
			spriteCharactersDir.AddCommand("SetSpriteInstant", new Action<string[]>(SetSpriteInstant));
			spriteCharactersDir.AddCommand("SetSprite", new Func<string[], IEnumerator>(SetSprite), new Func<string[], IEnumerator>(SetSprite));

			// Model3D Characters
			model3DCharactersDir.AddCommand("SetExpressionInstant", new Action<string[]>(SetExpressionInstant));
			model3DCharactersDir.AddCommand("SetExpression", new Func<string[], IEnumerator>(SetExpression), new Func<string[], IEnumerator>(SetExpression));
			model3DCharactersDir.AddCommand("SetMotion", new Action<string[]>(SetMotion));
		}


		/***** All Characters *****/

		static IEnumerator CreateCharacters(string[] args)
		{
			if (args.Length == 0) yield break;

			Task task = characterManager.CreateCharacters(args);
			yield return new WaitUntil(() => task.IsCompleted);
		}

		static IEnumerator CreateCharacter(string[] args)
		{
			string shortName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			string castShortName = args.Length > 1 ? ParseArgument<string>(args[1]) : null;

			if (string.IsNullOrEmpty(shortName) || (castShortName != null && castShortName == string.Empty)) yield break;

			Task task = characterManager.CreateCharacter(shortName, castShortName);
			yield return new WaitUntil(() => task.IsCompleted);
		}

		static void SetName(string[] args)
		{
			string name = args.Length > 1 ? ParseArgument<string>(args[1]) : null;

			Character character = GetCharacterFromArgs<Character>(args);
			if (character == null) return;

			character.SetName(name);
		}

		static void SetDisplayName(string[] args)
		{
			string displayName = args.Length > 1 ? ParseArgument<string>(args[1]) : null;

			Character character = GetCharacterFromArgs<Character>(args);
			if (character == null) return;

			character.SetDisplayName(displayName);
		}


		/***** Graphics Characters *****/

		static void SetPriority(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			int priority = args.Length > 1 ? ParseArgument<int>(args[1]) : default;

			character.SetPriority(priority);
		}

		static void SetAnimation(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			string animation = args.Length > 1 ? ParseArgument<string>(args[1]) : default;

			if (args.Length > 2)
				character.SetAnimation(animation, ParseArgument<bool>(args[2])); // switch on and off
			else
				character.SetAnimation(animation); // one-time
		}

		static void ShowInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.ShowInstant();
		}

		static IEnumerator Show(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.Show(speed);
		}

		static void HideInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.HideInstant();
		}

		static IEnumerator Hide(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.Hide(speed);
		}

		static void SetPosInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			float xPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;
			float yPos = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			character.SetPositionInstant(new Vector2(xPos, yPos));
		}

		static IEnumerator SetPos(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float xPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;
			float yPos = args.Length > 2 ? ParseArgument<float>(args[2]) : default;
			float speed = args.Length > 3 ? ParseArgument<float>(args[3]) : default;

			yield return character.SetPosition(new Vector2(xPos, yPos), speed);
		}

		static void SetPosXInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			float xPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			character.SetPositionXInstant(xPos);
		}

		static IEnumerator SetPosX(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float xPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;
			float speed = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			yield return character.SetPositionX(xPos, speed);
		}

		static void SetPosYInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			float yPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			character.SetPositionYInstant(yPos);
		}

		static IEnumerator SetPosY(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float yPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;
			float speed = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			yield return character.SetPositionY(yPos, speed);
		}

		static void FlipInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.FlipInstant();
		}

		static IEnumerator Flip(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.Flip(speed);
		}

		static void FaceLeftInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.FaceLeftInstant();
		}

		static IEnumerator FaceLeft(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.FaceLeft(speed);
		}

		static void FaceRightInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.FaceRightInstant();
		}

		static IEnumerator FaceRight(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.FaceRight(speed);
		}

		static void HighlightInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.HighlightInstant();
		}

		static IEnumerator Highlight(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.Highlight(speed);
		}

		static void UnhighlightInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.UnhighlightInstant();
		}

		static IEnumerator Unhighlight(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.Unhighlight(speed);
		}

		static void SetColorInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			string hexColor = args.Length > 1 ? ParseArgument<string>(args[1]) : default;

			character.SetColorInstant(GetColorFromHex(hexColor));
		}

		static IEnumerator SetColor(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			string hexColor = args.Length > 1 ? ParseArgument<string>(args[1]) : default;
			float speed = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			yield return character.SetColor(GetColorFromHex(hexColor), speed);
		}


		/***** Sprite Characters *****/

		static void SetSpriteInstant(string[] args)
		{
			SpriteCharacter character = GetCharacterFromArgs<SpriteCharacter>(args);
			if (character == null) return;

			SpriteLayerType layerType = args.Length > 1 ? ParseArgument<SpriteLayerType>(args[1]) : default;
			string spriteName = args.Length > 2 ? ParseArgument<string>(args[2]) : default;

			character.SetSpriteInstant(layerType, spriteName);
		}

		static IEnumerator SetSprite(string[] args)
		{
			SpriteCharacter character = GetCharacterFromArgs<SpriteCharacter>(args);
			if (character == null) yield break;

			SpriteLayerType layerType = args.Length > 1 ? ParseArgument<SpriteLayerType>(args[1]) : default;
			string spriteName = args.Length > 2 ? ParseArgument<string>(args[2]) : default;
			float speed = args.Length > 3 ? ParseArgument<float>(args[3]) : default;

			yield return character.SetSprite(layerType, spriteName, speed);
		}


		/***** Model 3D Characters *****/

		static void SetExpressionInstant(string[] args)
		{
			Model3DCharacter character = GetCharacterFromArgs<Model3DCharacter>(args);
			if (character == null) return;

			string expressionName = args.Length > 1 ? ParseArgument<string>(args[1]) : default;

			character.SetExpressionInstant(expressionName);
		}

		static IEnumerator SetExpression(string[] args)
		{
			Model3DCharacter character = GetCharacterFromArgs<Model3DCharacter>(args);
			if (character == null) yield break;

			string expressionName = args.Length > 1 ? ParseArgument<string>(args[1]) : default;
			float speed = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			yield return character.SetExpression(expressionName, speed);
		}

		static void SetMotion(string[] args)
		{
			Model3DCharacter character = GetCharacterFromArgs<Model3DCharacter>(args);
			if (character == null) return;

			string motionName = args.Length > 1 ? ParseArgument<string>(args[1]) : default;

			character.SetMotion(motionName);
		}


		/***** Helper Functions *****/

		static T GetCharacterFromArgs<T>(string[] commandArgs) where T : Character
		{
			string name = commandArgs.Length > 0 ? ParseArgument<string>(commandArgs[0]) : null;
			if (string.IsNullOrEmpty(name)) return null;

			Character character = characterManager.GetCharacter(name);
			if (character == null || character is not T) return null;

			return (T)character;
		}
	}
}
