using Audio;
using Dialogue;
using System;
using System.Collections;

namespace Commands
{
	public class AudioCMD : DialogueCommand
	{
		static CommandManager commandManager;
		static AudioManager audioManager;

		public new static void Register(CommandManager manager)
		{
			commandManager = manager;
			audioManager = manager.Audio;

			CommandBank bank = manager.GetBank(CommandManager.MainBankName);

			// Initialize Layers
			bank.AddCommand("CreateAmbient", new Func<DialogueCommandArguments, CommandProcessBase>(CreateAmbient));
			bank.AddCommand("CreateMusic", new Func<DialogueCommandArguments, CommandProcessBase>(CreateMusic));
			bank.AddCommand("CreateSFX", new Func<DialogueCommandArguments, CommandProcessBase>(CreateSFX));
			bank.AddCommand("CreateVoice", new Func<DialogueCommandArguments, CommandProcessBase>(CreateVoice));

			// Play Audio
			bank.AddCommand("PlayAmbient", new Func<DialogueCommandArguments, CommandProcessBase>(PlayAmbient));
			bank.AddCommand("PlayMusic", new Func<DialogueCommandArguments, CommandProcessBase>(PlayMusic));
			bank.AddCommand("PlaySFX", new Func<DialogueCommandArguments, CommandProcessBase>(PlaySFX));
			bank.AddCommand("PlayVoice", new Func<DialogueCommandArguments, CommandProcessBase>(PlayVoice));

			// Stop Audio
			bank.AddCommand("StopAmbient", new Func<DialogueCommandArguments, CommandProcessBase>(StopAmbient));
			bank.AddCommand("StopMusic", new Func<DialogueCommandArguments, CommandProcessBase>(StopMusic));
			bank.AddCommand("StopSFX", new Func<DialogueCommandArguments, CommandProcessBase>(StopSFX));
			bank.AddCommand("StopVoice", new Func<DialogueCommandArguments, CommandProcessBase>(StopVoice));
		}


		/***** Initialize Layers *****/

		static CommandProcessBase CreateAmbient(DialogueCommandArguments args)
		{
			int layerCount = args.Get(0, "layers", 1);

			void action() => audioManager.Create(AudioType.Ambient, layerCount);
			return new ActionCommandProcess(action);
		}

		static CommandProcessBase CreateMusic(DialogueCommandArguments args)
		{
			int layerCount = args.Get(0, "layers", 1);

			void action() => audioManager.Create(AudioType.Music, layerCount);
			return new ActionCommandProcess(action);
		}

		static CommandProcessBase CreateSFX(DialogueCommandArguments args)
		{
			void action() => audioManager.Create(AudioType.SFX, 1);
			return new ActionCommandProcess(action);
		}

		static CommandProcessBase CreateVoice(DialogueCommandArguments args)
		{
			int layerCount = args.Get(0, "layers", 1);

			void action() => audioManager.Create(AudioType.Voice, layerCount);
			return new ActionCommandProcess(action);
		}


		/***** Play Audio *****/

		static CommandProcessBase PlayAmbient(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			float volume = args.Get(1, "volume", 0.5f);
			float pitch = args.Get(2, "pitch", 1f);
			bool isLooping = args.Get(3, "loop", true);
			bool isImmediate = args.Get(4, "immediate", false);
			float fadeSpeed = args.Get(5, "speed", 0f);
			int layerNum = args.Get(6, "layer", 0);

			IEnumerator processFunc() => audioManager.Play(AudioType.Ambient, fileName, volume, pitch, isLooping, isImmediate, fadeSpeed, layerNum);
			void skipFunc() { }
			bool isCompletedFunc() => true;

			return new TransitionCommandProcess(commandManager, processFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase PlayMusic(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			float volume = args.Get(1, "volume", 0.5f);
			float pitch = args.Get(2, "pitch", 1f);
			bool isLooping = args.Get(3, "loop", true);
			bool isImmediate = args.Get(4, "immediate", false);
			float fadeSpeed = args.Get(5, "speed", 0f);
			int layerNum = args.Get(6, "layer", 0);

			IEnumerator processFunc() => audioManager.Play(AudioType.Music, fileName, volume, pitch, isLooping, isImmediate, fadeSpeed, layerNum);
			void skipFunc() { }
			bool isCompletedFunc() => true;

			return new TransitionCommandProcess(commandManager, processFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase PlaySFX(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			float volume = args.Get(1, "volume", 0.5f);
			float pitch = args.Get(2, "pitch", 1f);
			bool isLooping = args.Get(3, "loop", false);
			bool isImmediate = args.Get(4, "immediate", true);
			float fadeSpeed = args.Get(5, "speed", 0f);

			IEnumerator processFunc() => audioManager.Play(AudioType.SFX, fileName, volume, pitch, isLooping, isImmediate, fadeSpeed, 0);
			void skipFunc() { }
			bool isCompletedFunc() => true;

			return new TransitionCommandProcess(commandManager, processFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase PlayVoice(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			float volume = args.Get(1, "volume", 0.5f);
			float pitch = args.Get(2, "pitch", 1f);
			bool isLooping = args.Get(3, "loop", false);
			bool isImmediate = args.Get(4, "immediate", true);
			float fadeSpeed = args.Get(5, "speed", 0f);
			int layerNum = args.Get(6, "layer", 0);

			IEnumerator processFunc() => audioManager.Play(AudioType.Voice, fileName, volume, pitch, isLooping, isImmediate, fadeSpeed, layerNum);
			void skipFunc() { }
			bool isCompletedFunc() => true;

			return new TransitionCommandProcess(commandManager, processFunc, skipFunc, isCompletedFunc);
		}


		/***** Stop Audio *****/

		static CommandProcessBase StopAmbient(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			bool isImmediate = args.Get(1, "immediate", false);
			float fadeSpeed = args.Get(2, "speed", 0f);
			int layerNum = args.Get(3, "layer", 0);

			void action() => audioManager.Stop(AudioType.Ambient, fileName, isImmediate, fadeSpeed, layerNum);
			return new ActionCommandProcess(action);
		}

		static CommandProcessBase StopMusic(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			bool isImmediate = args.Get(1, "immediate", false);
			float fadeSpeed = args.Get(2, "speed", 0f);
			int layerNum = args.Get(3, "layer", 0);

			void action() => audioManager.Stop(AudioType.Music, fileName, isImmediate, fadeSpeed, layerNum);
			return new ActionCommandProcess(action);
		}

		static CommandProcessBase StopSFX(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			bool isImmediate = args.Get(1, "immediate", false);
			float fadeSpeed = args.Get(2, "speed", 0f);

			void action() => audioManager.Stop(AudioType.SFX, fileName, isImmediate, fadeSpeed, 0);
			return new ActionCommandProcess(action);
		}

		static CommandProcessBase StopVoice(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			bool isImmediate = args.Get(1, "immediate", false);
			float fadeSpeed = args.Get(2, "speed", 0f);
			int layerNum = args.Get(3, "layer", 0);

			void action() => audioManager.Stop(AudioType.Voice, fileName, isImmediate, fadeSpeed, layerNum);
			return new ActionCommandProcess(action);
		}
	}
}
