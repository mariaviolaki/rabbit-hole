using Characters;
using Dialogue;
using System;
using System.Collections;

namespace Commands
{
	public class CharactersCMD : DialogueCommand
	{
		static CommandManager commandManager;
		static CharacterManager characterManager;

		public new static void Register(CommandManager manager)
		{
			commandManager = manager;
			characterManager = manager.Characters;

			CommandBank mainDir = manager.GetBank(CommandManager.MainBankName);
			CommandBank charactersDir = manager.GetBank(CommandManager.CharacterBankName);
			CommandBank graphicsCharactersDir = manager.GetBank(CommandManager.GraphicsCharacterBankName);
			CommandBank spriteCharactersDir = manager.GetBank(CommandManager.SpriteCharacterBankName);
			CommandBank model3DCharactersDir = manager.GetBank(CommandManager.Model3DCharacterBankName);

			// All Characters (cannot be called on specific ones)
			mainDir.AddCommand("CreateCharacters", new Func<DialogueCommandArguments, CommandProcessBase>(CreateCharacters));
			mainDir.AddCommand("CreateCharacter", new Func<DialogueCommandArguments, CommandProcessBase>(CreateCharacter));
			mainDir.AddCommand("SetCharacterPriority", new Func<DialogueCommandArguments, CommandProcessBase>(SetCharacterPriority));

			// All Characters
			charactersDir.AddCommand("SetName", new Func<DialogueCommandArguments, CommandProcessBase>(SetName));

			// Graphics Characters
			graphicsCharactersDir.AddCommand("SetPriority", new Func<DialogueCommandArguments, CommandProcessBase>(SetPriority));
			graphicsCharactersDir.AddCommand("SetAnimation", new Func<DialogueCommandArguments, CommandProcessBase>(SetAnimation));
			graphicsCharactersDir.AddCommand("Show", new Func<DialogueCommandArguments, CommandProcessBase>(Show));
			graphicsCharactersDir.AddCommand("Hide", new Func<DialogueCommandArguments, CommandProcessBase>(Hide));
			graphicsCharactersDir.AddCommand("SetPosition", new Func<DialogueCommandArguments, CommandProcessBase>(SetPosition));
			graphicsCharactersDir.AddCommand("FaceLeft", new Func<DialogueCommandArguments, CommandProcessBase>(FaceLeft));
			graphicsCharactersDir.AddCommand("FaceRight", new Func<DialogueCommandArguments, CommandProcessBase>(FaceRight));
			graphicsCharactersDir.AddCommand("Highlight", new Func<DialogueCommandArguments, CommandProcessBase>(Highlight));
			graphicsCharactersDir.AddCommand("Unhighlight", new Func<DialogueCommandArguments, CommandProcessBase>(Unhighlight));
			graphicsCharactersDir.AddCommand("SetColor", new Func<DialogueCommandArguments, CommandProcessBase>(SetColor));

			// Sprite Characters
			spriteCharactersDir.AddCommand("SetSprite", new Func<DialogueCommandArguments, CommandProcessBase>(SetSprite));

			// Model3D Characters
			model3DCharactersDir.AddCommand("SetExpression", new Func<DialogueCommandArguments, CommandProcessBase>(SetExpression));
			model3DCharactersDir.AddCommand("SetMotion", new Func<DialogueCommandArguments, CommandProcessBase>(SetMotion));
		}


		/***** All Characters *****/

		static CommandProcessBase CreateCharacters(DialogueCommandArguments args)
		{
			if (args.IndexedArguments.Count == 0) return null;

			IEnumerator process() => characterManager.CreateCharacters(args.IndexedArguments);
			return new CoroutineCommandProcess(commandManager, process, true);
		}

		static CommandProcessBase CreateCharacter(DialogueCommandArguments args)
		{
			string shortName = args.Get(0, "shortName", "");
			string castShortName = args.Get(1, "castName", "");

			if (string.IsNullOrWhiteSpace(shortName)) return null;

			IEnumerator process() => characterManager.CreateCharacter(shortName, castShortName);
			return new CoroutineCommandProcess(commandManager, process, true);
		}

		static CommandProcessBase SetName(DialogueCommandArguments args)
		{
			string name = args.Get(1, "name", "");

			Character character = GetCharacterFromArgs<Character>(args);
			if (character == null) return null;

			void action() => character.SetName(name);
			return new ActionCommandProcess(action);
		}


		/***** Graphics Characters *****/

		static CommandProcessBase SetCharacterPriority(DialogueCommandArguments args)
		{
			if (args.IndexedArguments.Count == 0) return null;

			void action() => characterManager.SetPriority(args.IndexedArguments.ToArray());
			return new ActionCommandProcess(action);
		}

		static CommandProcessBase SetPriority(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			int priority = args.Get(1, "priority", 0);

			void action() => character.SetPriority(priority);
			return new ActionCommandProcess(action);
		}

		static CommandProcessBase SetAnimation(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			string name = args.Get(1, "name", "");

			Action action;

			if (args.Has<bool>(2, "status"))
				action = () => character.SetAnimation(name, args.Get(2, "status", false)); // switch on and off
			else
				action = () => character.SetAnimation(name); // one-time

			return new ActionCommandProcess(action);
		}

		static CommandProcessBase Show(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			void runFunc() => character.Show(isImmediate, speed);
			void skipFunc() => character.SkipVisibilityTransition();
			bool isCompletedFunc() => !character.IsTransitioningVisibility();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase Hide(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);
			
			void runFunc() => character.Hide(isImmediate, speed);
			void skipFunc() => character.SkipVisibilityTransition();
			bool isCompletedFunc() => !character.IsTransitioningVisibility();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase SetPosition(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			float xPos = args.Get(1, "x", float.NaN);
			float yPos = args.Get(2, "y", float.NaN);
			bool isImmediate = args.Get(3, "immediate", false);
			float speed = args.Get(4, "speed", 0f);

			void runFunc() => character.SetPosition(xPos, yPos, isImmediate, speed);
			void skipFunc() => character.SkipPositionTransition();
			bool isCompletedFunc() => !character.IsTransitioningPosition();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase FaceLeft(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			void runFunc() => character.FaceLeft(isImmediate, speed);
			void skipFunc() => character.SkipDirectionTransition();
			bool isCompletedFunc() => !character.IsTransitioningDirection();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase FaceRight(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			void runFunc() => character.FaceRight(isImmediate, speed);
			void skipFunc() => character.SkipDirectionTransition();
			bool isCompletedFunc() => !character.IsTransitioningDirection();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase Highlight(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			void runFunc() => character.Highlight(isImmediate, speed);
			void skipFunc() => character.SkipColorTransition();
			bool isCompletedFunc() => !character.IsTransitioningColor();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase Unhighlight(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			void runFunc() => character.Unhighlight(isImmediate, speed);
			void skipFunc() => character.SkipColorTransition();
			bool isCompletedFunc() => !character.IsTransitioningColor();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase SetColor(DialogueCommandArguments args)
		{
			GraphicsCharacter character = GetCharacterFromArgs<GraphicsCharacter>(args);
			if (character == null) return null;

			string hexColor = args.Get(1, "color", "");
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			void runFunc() => character.SetColor(Utilities.GetColorFromHex(hexColor), isImmediate, speed);
			void skipFunc() => character.SkipColorTransition();
			bool isCompletedFunc() => !character.IsTransitioningColor();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}


		/***** Sprite Characters *****/

		static CommandProcessBase SetSprite(DialogueCommandArguments args)
		{
			SpriteCharacter character = GetCharacterFromArgs<SpriteCharacter>(args);
			if (character == null) return null;

			string spriteName = args.Get(1, "name", "");
			SpriteLayerType layerType = args.Get(2, "layer", SpriteLayerType.None);
			bool isImmediate = args.Get(3, "immediate", false);
			float speed = args.Get(4, "speed", 0f);

			void runFunc() => character.SetSprite(spriteName, layerType, isImmediate, speed);
			void skipFunc() => character.SkipSpriteTransition();
			bool isCompletedFunc() => !character.IsTransitioningSprite();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}


		/***** Model 3D Characters *****/

		static CommandProcessBase SetExpression(DialogueCommandArguments args)
		{
			Model3DCharacter character = GetCharacterFromArgs<Model3DCharacter>(args);
			if (character == null) return null;

			string expressionName = args.Get(1, "name", "");
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			void runFunc() => character.SetExpression(expressionName, isImmediate, speed);
			void skipFunc() => character.SkipExpressionTransition();
			bool isCompletedFunc() => !character.IsTransitioningExpression();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase SetMotion(DialogueCommandArguments args)
		{
			Model3DCharacter character = GetCharacterFromArgs<Model3DCharacter>(args);
			if (character == null) return null;

			string motionName = args.Get(1, "name", "");

			void action() => character.SetMotion(motionName);
			return new ActionCommandProcess(action);
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
