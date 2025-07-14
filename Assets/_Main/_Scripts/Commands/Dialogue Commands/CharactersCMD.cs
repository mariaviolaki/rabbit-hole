using System;
using Characters;
using Dialogue;
using UnityEngine;

namespace Commands
{
	public class CharactersCMD : DialogueCommand
	{
		static CharacterManager characterManager;

		public new static void Register(CommandManager commandManager)
		{
			characterManager = commandManager.Characters;

			CommandBank mainDir = commandManager.GetBank(CommandManager.MainBankName);
			CommandBank charactersDir = commandManager.GetBank(CommandManager.CharacterBankName);
			CommandBank graphicsCharactersDir = commandManager.GetBank(CommandManager.GraphicsCharacterBankName);
			CommandBank spriteCharactersDir = commandManager.GetBank(CommandManager.SpriteCharacterBankName);
			CommandBank model3DCharactersDir = commandManager.GetBank(CommandManager.Model3DCharacterBankName);

			// All Characters (cannot be called on specific ones)
			mainDir.AddCommand("CreateCharacters", new Func<DialogueCommandArguments, Coroutine>(CreateCharacters), CommandSkipType.None);
			mainDir.AddCommand("CreateCharacter", new Func<DialogueCommandArguments, Coroutine>(CreateCharacter), CommandSkipType.None);
			mainDir.AddCommand("SetCharacterPriority", new Action<DialogueCommandArguments>(SetCharacterPriority));

			// All Characters
			charactersDir.AddCommand("SetName", new Action<DialogueCommandArguments>(SetName));
			charactersDir.AddCommand("SetDisplayName", new Action<DialogueCommandArguments>(SetDisplayName));

			// Graphics Characters
			graphicsCharactersDir.AddCommand("SetPriority", new Action<DialogueCommandArguments>(SetPriority));
			graphicsCharactersDir.AddCommand("SetAnimation", new Action<DialogueCommandArguments>(SetAnimation));
			graphicsCharactersDir.AddCommand("Show", new Func<DialogueCommandArguments, Coroutine>(Show), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("Hide", new Func<DialogueCommandArguments, Coroutine>(Hide), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("SetPosition", new Func<DialogueCommandArguments, Coroutine>(SetPosition), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("Flip", new Func<DialogueCommandArguments, Coroutine>(Flip), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("FaceLeft", new Func<DialogueCommandArguments, Coroutine>(FaceLeft), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("FaceRight", new Func<DialogueCommandArguments, Coroutine>(FaceRight), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("Highlight", new Func<DialogueCommandArguments, Coroutine>(Highlight), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("Unhighlight", new Func<DialogueCommandArguments, Coroutine>(Unhighlight), CommandSkipType.Transition);
			graphicsCharactersDir.AddCommand("SetColor", new Func<DialogueCommandArguments, Coroutine>(SetColor), CommandSkipType.Transition);

			// Sprite Characters
			spriteCharactersDir.AddCommand("SetSprite", new Func<DialogueCommandArguments, Coroutine>(SetSprite), CommandSkipType.Transition);

			// Model3D Characters
			model3DCharactersDir.AddCommand("SetExpression", new Func<DialogueCommandArguments, Coroutine>(SetExpression), CommandSkipType.Transition);
			model3DCharactersDir.AddCommand("SetMotion", new Action<DialogueCommandArguments>(SetMotion));
		}


		/***** All Characters *****/

		static Coroutine CreateCharacters(DialogueCommandArguments args)
		{
			if (args.IndexedArguments.Count == 0) return null;

			return characterManager.CreateCharacters(args.IndexedArguments);
		}

		static Coroutine CreateCharacter(DialogueCommandArguments args)
		{
			string shortName = args.Get(0, "shortName", "");
			string castShortName = args.Get(1, "castShortName", "");

			if (string.IsNullOrWhiteSpace(shortName)) return null;

			return characterManager.CreateCharacter(shortName, castShortName);
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

		static void SetCharacterPriority(DialogueCommandArguments args)
		{
			if (args.IndexedArguments.Count == 0) return;

			characterManager.SetPriority(args.IndexedArguments.ToArray());
		}

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

		static Coroutine Show(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			return character.Show(isImmediate, speed);
		}

		static Coroutine Hide(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			return character.Hide(isImmediate, speed);
		}

		static Coroutine SetPosition(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			float xPos = args.Get(1, "x", float.NaN);
			float yPos = args.Get(2, "y", float.NaN);
			bool isImmediate = args.Get(3, "immediate", false);
			float speed = args.Get(4, "speed", 0f);

			return character.SetPosition(xPos, yPos, isImmediate, speed);
		}

		static Coroutine Flip(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			return character.Flip(isImmediate, speed);
		}

		static Coroutine FaceLeft(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			return character.FaceLeft(isImmediate, speed);
		}

		static Coroutine FaceRight(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			return character.FaceRight(isImmediate, speed);
		}

		static Coroutine Highlight(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			return character.Highlight(isImmediate, speed);
		}

		static Coroutine Unhighlight(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			return character.Unhighlight(isImmediate, speed);
		}

		static Coroutine SetColor(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			string hexColor = args.Get(1, "color", "");
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			return character.SetColor(Utilities.GetColorFromHex(hexColor), isImmediate, speed);
		}


		/***** Sprite Characters *****/

		static Coroutine SetSprite(DialogueCommandArguments args)
		{
			SpriteCharacter character = GetCharacterFromArgs<SpriteCharacter>(args);
			if (character == null) return null;

			string spriteName = args.Get(1, "name", "");
			SpriteLayerType layerType = args.Get(2, "layer", SpriteLayerType.None);
			bool isImmediate = args.Get(3, "immediate", false);
			float speed = args.Get(4, "speed", 0f);

			return character.SetSprite(spriteName, layerType, isImmediate, speed);
		}


		/***** Model 3D Characters *****/

		static Coroutine SetExpression(DialogueCommandArguments args)
		{
			Model3DCharacter character = GetCharacterFromArgs<Model3DCharacter>(args);
			if (character == null) return null;

			string expressionName = args.Get(1, "name", "");
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			return character.SetExpression(expressionName, isImmediate, speed);
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
