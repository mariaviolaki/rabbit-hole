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

		public new static void Register(CommandDirectory commandDirectory)
		{
			characterManager = commandDirectory.GetCharacterManager();

			// All Characters
			commandDirectory.AddCommand("CreateCharacters", new Func<string[], IEnumerator>(CreateCharacters));
			commandDirectory.AddCommand("CreateCharacter", new Func<string[], IEnumerator>(CreateCharacter));
			commandDirectory.AddCommand("SetCharacterName", new Action<string[]>(SetCharacterName));
			commandDirectory.AddCommand("SetCharacterDisplayName", new Action<string[]>(SetCharacterDisplayName));

			// Graphics Characters
			commandDirectory.AddCommand("SetCharacterPriority", new Action<string[]>(SetCharacterPriority));
			commandDirectory.AddCommand("SetCharacterAnimation", new Action<string[]>(SetCharacterAnimation));
			commandDirectory.AddCommand("ShowCharacterInstant", new Action<string[]>(ShowCharacterInstant));
			commandDirectory.AddCommand("ShowCharacter", new Func<string[], IEnumerator>(ShowCharacter), new Func<string[], IEnumerator>(ShowCharacter));
			commandDirectory.AddCommand("HideCharacterInstant", new Action<string[]>(HideCharacterInstant));
			commandDirectory.AddCommand("HideCharacter", new Func<string[], IEnumerator>(HideCharacter), new Func<string[], IEnumerator>(HideCharacter));
			commandDirectory.AddCommand("SetCharacterPosInstant", new Action<string[]>(SetCharacterPosInstant));
			commandDirectory.AddCommand("SetCharacterPos", new Func<string[], IEnumerator>(SetCharacterPos), new Func<string[], IEnumerator>(SetCharacterPos));
			commandDirectory.AddCommand("SetCharacterPosXInstant", new Action<string[]>(SetCharacterPosXInstant));
			commandDirectory.AddCommand("SetCharacterPosX", new Func<string[], IEnumerator>(SetCharacterPosX), new Func<string[], IEnumerator>(SetCharacterPosX));
			commandDirectory.AddCommand("SetCharacterPosYInstant", new Action<string[]>(SetCharacterPosYInstant));
			commandDirectory.AddCommand("SetCharacterPosY", new Func<string[], IEnumerator>(SetCharacterPosY), new Func<string[], IEnumerator>(SetCharacterPosY));
			commandDirectory.AddCommand("FlipCharacterInstant", new Action<string[]>(FlipCharacterInstant));
			commandDirectory.AddCommand("FlipCharacter", new Func<string[], IEnumerator>(FlipCharacter), new Func<string[], IEnumerator>(FlipCharacter));
			commandDirectory.AddCommand("FaceLeftCharacterInstant", new Action<string[]>(FaceLeftCharacterInstant));
			commandDirectory.AddCommand("FaceLeftCharacter", new Func<string[], IEnumerator>(FaceLeftCharacter), new Func<string[], IEnumerator>(FaceLeftCharacter));
			commandDirectory.AddCommand("FaceRightCharacterInstant", new Action<string[]>(FaceRightCharacterInstant));
			commandDirectory.AddCommand("FaceRightCharacter", new Func<string[], IEnumerator>(FaceRightCharacter), new Func<string[], IEnumerator>(FaceRightCharacter));
			commandDirectory.AddCommand("HighlightCharacterInstant", new Action<string[]>(HighlightCharacterInstant));
			commandDirectory.AddCommand("HighlightCharacter", new Func<string[], IEnumerator>(HighlightCharacter), new Func<string[], IEnumerator>(HighlightCharacter));
			commandDirectory.AddCommand("UnhighlightCharacterInstant", new Action<string[]>(UnhighlightCharacterInstant));
			commandDirectory.AddCommand("UnhighlightCharacter", new Func<string[], IEnumerator>(UnhighlightCharacter), new Func<string[], IEnumerator>(UnhighlightCharacter));
			commandDirectory.AddCommand("SetCharacterColorInstant", new Action<string[]>(SetCharacterColorInstant));
			commandDirectory.AddCommand("SetCharacterColor", new Func<string[], IEnumerator>(SetCharacterColor), new Func<string[], IEnumerator>(SetCharacterColor));

			// Sprite Characters
			commandDirectory.AddCommand("SetCharacterSpriteInstant", new Action<string[]>(SetCharacterSpriteInstant));
			commandDirectory.AddCommand("SetCharacterSprite", new Func<string[], IEnumerator>(SetCharacterSprite), new Func<string[], IEnumerator>(SetCharacterSprite));

			// Model3D Characters
			commandDirectory.AddCommand("SetCharacterExpressionInstant", new Action<string[]>(SetCharacterExpressionInstant));
			commandDirectory.AddCommand("SetCharacterExpression", new Func<string[], IEnumerator>(SetCharacterExpression), new Func<string[], IEnumerator>(SetCharacterExpression));
			commandDirectory.AddCommand("SetCharacterMotion", new Action<string[]>(SetCharacterMotion));
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

		static void SetCharacterName(string[] args)
		{
			string name = args.Length > 1 ? ParseArgument<string>(args[1]) : null;

			Character character = GetCharacterFromArgs<Character>(args);
			if (character == null) return;

			character.SetName(name);
		}

		static void SetCharacterDisplayName(string[] args)
		{
			string displayName = args.Length > 1 ? ParseArgument<string>(args[1]) : null;

			Character character = GetCharacterFromArgs<Character>(args);
			if (character == null) return;

			character.SetDisplayName(displayName);
		}


		/***** Graphics Characters *****/

		static void SetCharacterPriority(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			int priority = args.Length > 1 ? ParseArgument<int>(args[1]) : default;

			character.SetPriority(priority);
		}

		static void SetCharacterAnimation(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			string animation = args.Length > 1 ? ParseArgument<string>(args[1]) : default;

			if (args.Length > 2)
				character.SetAnimation(animation, ParseArgument<bool>(args[2])); // switch on and off
			else
				character.SetAnimation(animation); // one-time
		}

		static void ShowCharacterInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.ShowInstant();
		}

		static IEnumerator ShowCharacter(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.Show(speed);
		}

		static void HideCharacterInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.HideInstant();
		}

		static IEnumerator HideCharacter(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.Hide(speed);
		}

		static void SetCharacterPosInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			float xPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;
			float yPos = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			character.SetPositionInstant(new Vector2(xPos, yPos));
		}

		static IEnumerator SetCharacterPos(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float xPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;
			float yPos = args.Length > 2 ? ParseArgument<float>(args[2]) : default;
			float speed = args.Length > 3 ? ParseArgument<float>(args[3]) : default;

			yield return character.SetPosition(new Vector2(xPos, yPos), speed);
		}

		static void SetCharacterPosXInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			float xPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			character.SetPositionXInstant(xPos);
		}

		static IEnumerator SetCharacterPosX(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float xPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;
			float speed = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			yield return character.SetPositionX(xPos, speed);
		}

		static void SetCharacterPosYInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			float yPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			character.SetPositionYInstant(yPos);
		}

		static IEnumerator SetCharacterPosY(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float yPos = args.Length > 1 ? ParseArgument<float>(args[1]) : default;
			float speed = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			yield return character.SetPositionY(yPos, speed);
		}

		static void FlipCharacterInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.FlipInstant();
		}

		static IEnumerator FlipCharacter(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.Flip(speed);
		}

		static void FaceLeftCharacterInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.FaceLeftInstant();
		}

		static IEnumerator FaceLeftCharacter(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.FaceLeft(speed);
		}

		static void FaceRightCharacterInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.FaceRightInstant();
		}

		static IEnumerator FaceRightCharacter(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.FaceRight(speed);
		}

		static void HighlightCharacterInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.HighlightInstant();
		}

		static IEnumerator HighlightCharacter(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.Highlight(speed);
		}

		static void UnhighlightCharacterInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			character.UnhighlightInstant();
		}

		static IEnumerator UnhighlightCharacter(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return character.Unhighlight(speed);
		}

		static void SetCharacterColorInstant(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			string hexColor = args.Length > 1 ? ParseArgument<string>(args[1]) : default;

			character.SetColorInstant(GetColorFromHex(hexColor));
		}

		static IEnumerator SetCharacterColor(string[] args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			string hexColor = args.Length > 1 ? ParseArgument<string>(args[1]) : default;
			float speed = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			yield return character.SetColor(GetColorFromHex(hexColor), speed);
		}


		/***** Sprite Characters *****/

		static void SetCharacterSpriteInstant(string[] args)
		{
			SpriteCharacter character = GetCharacterFromArgs<SpriteCharacter>(args);
			if (character == null) return;

			SpriteLayerType layerType = args.Length > 1 ? ParseArgument<SpriteLayerType>(args[1]) : default;
			string spriteName = args.Length > 2 ? ParseArgument<string>(args[2]) : default;

			character.SetSpriteInstant(layerType, spriteName);
		}

		static IEnumerator SetCharacterSprite(string[] args)
		{
			SpriteCharacter character = GetCharacterFromArgs<SpriteCharacter>(args);
			if (character == null) yield break;

			SpriteLayerType layerType = args.Length > 1 ? ParseArgument<SpriteLayerType>(args[1]) : default;
			string spriteName = args.Length > 2 ? ParseArgument<string>(args[2]) : default;
			float speed = args.Length > 3 ? ParseArgument<float>(args[3]) : default;

			yield return character.SetSprite(layerType, spriteName, speed);
		}


		/***** Model 3D Characters *****/

		static void SetCharacterExpressionInstant(string[] args)
		{
			Model3DCharacter character = GetCharacterFromArgs<Model3DCharacter>(args);
			if (character == null) return;

			string expressionName = args.Length > 1 ? ParseArgument<string>(args[1]) : default;

			character.SetExpressionInstant(expressionName);
		}

		static IEnumerator SetCharacterExpression(string[] args)
		{
			Model3DCharacter character = GetCharacterFromArgs<Model3DCharacter>(args);
			if (character == null) yield break;

			string expressionName = args.Length > 1 ? ParseArgument<string>(args[1]) : default;
			float speed = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			yield return character.SetExpression(expressionName, speed);
		}

		static void SetCharacterMotion(string[] args)
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
