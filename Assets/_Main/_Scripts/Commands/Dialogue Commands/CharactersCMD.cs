using System;
using System.Collections;
using Characters;
using Dialogue;
using UnityEngine;

namespace Commands
{
	public class CharactersCMD : DialogueCommand
	{
		static CharacterManager characterManager;
		public new static readonly string[] blockingProcesses = new string[] { "CreateCharacters", "CreateCharacter" };

		public new static void Register(CommandManager commandManager)
		{
			characterManager = commandManager.GetCharacterManager();

			CommandDirectory mainDir = commandManager.GetDirectory(CommandManager.MainDirectoryName);
			CommandDirectory charactersDir = commandManager.GetDirectory(CommandManager.CharacterDirectoryName);
			CommandDirectory graphicsCharactersDir = commandManager.GetDirectory(CommandManager.GraphicsCharacterDirectoryName);
			CommandDirectory spriteCharactersDir = commandManager.GetDirectory(CommandManager.SpriteCharacterDirectoryName);
			CommandDirectory model3DCharactersDir = commandManager.GetDirectory(CommandManager.Model3DCharacterDirectoryName);

			// All Characters (cannot be called on specific ones)
			mainDir.AddCommand("CreateCharacters", new Func<DialogueCommandArguments, IEnumerator>(CreateCharacters));
			mainDir.AddCommand("CreateCharacter", new Func<DialogueCommandArguments, IEnumerator>(CreateCharacter));

			// All Characters
			charactersDir.AddCommand("SetName", new Action<DialogueCommandArguments>(SetName));
			charactersDir.AddCommand("SetDisplayName", new Action<DialogueCommandArguments>(SetDisplayName));

			// Graphics Characters
			graphicsCharactersDir.AddCommand("SetPriority", new Action<DialogueCommandArguments>(SetPriority));
			graphicsCharactersDir.AddCommand("SetAnimation", new Action<DialogueCommandArguments>(SetAnimation));
			graphicsCharactersDir.AddCommand("Show", new Func<DialogueCommandArguments, IEnumerator>(Show), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("Hide", new Func<DialogueCommandArguments, IEnumerator>(Hide), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("SetPosition", new Func<DialogueCommandArguments, IEnumerator>(SetPosition), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("Flip", new Func<DialogueCommandArguments, IEnumerator>(Flip), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("FaceLeft", new Func<DialogueCommandArguments, IEnumerator>(FaceLeft), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("FaceRight", new Func<DialogueCommandArguments, IEnumerator>(FaceRight), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("Highlight", new Func<DialogueCommandArguments, IEnumerator>(Highlight), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("Unhighlight", new Func<DialogueCommandArguments, IEnumerator>(Unhighlight), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("SetColor", new Func<DialogueCommandArguments, IEnumerator>(SetColor), CommandSkipType.Transition);

			// Sprite Characters
			spriteCharactersDir.AddCommand("SetSprite", new Func<DialogueCommandArguments, IEnumerator>(SetSprite), CommandSkipType.Transition);

			// Model3D Characters
			model3DCharactersDir.AddCommand("SetExpression", new Func<DialogueCommandArguments, IEnumerator>(SetExpression), CommandSkipType.Transition);
			model3DCharactersDir.AddCommand("SetMotion", new Action<DialogueCommandArguments>(SetMotion));
		}


		/***** All Characters *****/

		static IEnumerator CreateCharacters(DialogueCommandArguments args)
		{
			if (args.IndexedArguments.Count == 0) yield break;

			yield return characterManager.CreateCharacters(args.IndexedArguments);
		}

		static IEnumerator CreateCharacter(DialogueCommandArguments args)
		{
			string shortName = args.Get(0, "shortName", "");
			string castShortName = args.Get(1, "castShortName", "");

			if (string.IsNullOrWhiteSpace(shortName)) yield break;

			yield return characterManager.CreateCharacter(shortName, castShortName);
		}

		static void SetName(DialogueCommandArguments args)
		{
			string name = args.Get(1, "name", "");

			Character character = GetCharacterFromArgs<Character>(args);
			if (character == null) return;

			character.SetName(name);
		}

		static void SetDisplayName(DialogueCommandArguments args)
		{
			string displayName = args.Get(1, "displayName", "");

			Character character = GetCharacterFromArgs<Character>(args);
			if (character == null) return;

			character.SetDisplayName(displayName);
		}


		/***** Graphics Characters *****/

		static void SetPriority(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			int priority = args.Get(1, "priority", 0);

			character.SetPriority(priority);
		}

		static void SetAnimation(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return;

			string name = args.Get(1, "name", "");

			if (args.Has<bool>(2, "status"))
				character.SetAnimation(name, args.Get(2, "status", false)); // switch on and off
			else
				character.SetAnimation(name); // one-time
		}

		static IEnumerator Show(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			yield return character.Show(isImmediate, speed);
		}

		static IEnumerator Hide(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			yield return character.Hide(isImmediate, speed);
		}

		static IEnumerator SetPosition(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			float xPos = args.Get(1, "x", float.NaN);
			float yPos = args.Get(2, "y", float.NaN);
			bool isImmediate = args.Get(3, "immediate", false);
			float speed = args.Get(4, "speed", 0f);

			yield return character.SetPosition(xPos, yPos, isImmediate, speed);
		}

		static IEnumerator Flip(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			yield return character.Flip(isImmediate, speed);
		}

		static IEnumerator FaceLeft(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			yield return character.FaceLeft(isImmediate, speed);
		}

		static IEnumerator FaceRight(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			yield return character.FaceRight(isImmediate, speed);
		}

		static IEnumerator Highlight(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			yield return character.Highlight(isImmediate, speed);
		}

		static IEnumerator Unhighlight(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			yield return character.Unhighlight(isImmediate, speed);
		}

		static IEnumerator SetColor(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) yield break;

			string hexColor = args.Get(1, "color", "");
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			yield return character.SetColor(Utilities.GetColorFromHex(hexColor), isImmediate, speed);
		}


		/***** Sprite Characters *****/

		static IEnumerator SetSprite(DialogueCommandArguments args)
		{
			SpriteCharacter character = GetCharacterFromArgs<SpriteCharacter>(args);
			if (character == null) yield break;

			SpriteLayerType layerType = args.Get(1, "layer", SpriteLayerType.None);
			string spriteName = args.Get(2, "name", "");
			bool isImmediate = args.Get(3, "immediate", false);
			float speed = args.Get(4, "speed", 0f);

			yield return character.SetSprite(layerType, spriteName, isImmediate, speed);
		}


		/***** Model 3D Characters *****/

		static IEnumerator SetExpression(DialogueCommandArguments args)
		{
			Model3DCharacter character = GetCharacterFromArgs<Model3DCharacter>(args);
			if (character == null) yield break;

			string expressionName = args.Get(1, "name", "");
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			yield return character.SetExpression(expressionName, isImmediate, speed);
		}

		static void SetMotion(DialogueCommandArguments args)
		{
			Model3DCharacter character = GetCharacterFromArgs<Model3DCharacter>(args);
			if (character == null) return;

			string motionName = args.Get(1, "name", "");

			character.SetMotion(motionName);
		}


		/***** Helper Functions *****/

		static T GetCharacterFromArgs<T>(DialogueCommandArguments args) where T : Character
		{
			string shortName = args.Get(0, "shortName", "");
			if (string.IsNullOrWhiteSpace(shortName)) return null;

			Character character = characterManager.GetCharacter(shortName);
			if (character == null || character is not T) return null;

			return (T)character;
		}
	}
}
