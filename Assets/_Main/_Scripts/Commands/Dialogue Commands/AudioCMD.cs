using Audio;
using Dialogue;
using System;

namespace Commands
{
	public class AudioCMD : DialogueCommand
	{
		static AudioManager audioManager;

		public new static void Register(CommandManager commandManager)
		{
			audioManager = commandManager.GetAudioManager();

			CommandDirectory directory = commandManager.GetDirectory(CommandManager.AudioDirectoryName);

			// Initialize Layers
			directory.AddCommand("CreateAmbient", new Action<DialogueCommandArguments>(CreateAmbient));
			directory.AddCommand("CreateMusic", new Action<DialogueCommandArguments>(CreateMusic));
			directory.AddCommand("CreateSFX", new Action<DialogueCommandArguments>(CreateSFX));
			directory.AddCommand("CreateVoice", new Action<DialogueCommandArguments>(CreateVoice));

			// Play Audio
			directory.AddCommand("PlayAmbient", new Action<DialogueCommandArguments>(PlayAmbient));
			directory.AddCommand("PlayMusic", new Action<DialogueCommandArguments>(PlayMusic));
			directory.AddCommand("PlaySFX", new Action<DialogueCommandArguments>(PlaySFX));
			directory.AddCommand("PlayVoice", new Action<DialogueCommandArguments>(PlayVoice));

			// Stop Audio
			directory.AddCommand("StopAmbient", new Action<DialogueCommandArguments>(StopAmbient));
			directory.AddCommand("StopMusic", new Action<DialogueCommandArguments>(StopMusic));
			directory.AddCommand("StopSFX", new Action<DialogueCommandArguments>(StopSFX));
			directory.AddCommand("StopVoice", new Action<DialogueCommandArguments>(StopVoice));
		}


		/***** Initialize Layers *****/

		static void CreateAmbient(DialogueCommandArguments args)
		{
			int layerCount = args.Get(0, "layers", 1);
			
			audioManager.Create(AudioType.Ambient, layerCount);
		}

		static void CreateMusic(DialogueCommandArguments args)
		{
			int layerCount = args.Get(0, "layers", 1);

			audioManager.Create(AudioType.Music, layerCount);
		}

		static void CreateSFX(DialogueCommandArguments args)
		{
			audioManager.Create(AudioType.SFX, 1);
		}

		static void CreateVoice(DialogueCommandArguments args)
		{
			int layerCount = args.Get(0, "layers", 1);

			audioManager.Create(AudioType.Voice, layerCount);
		}


		/***** Play Audio *****/

		static void PlayAmbient(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			float volume = args.Get(1, "volume", 0.5f);
			float pitch = args.Get(2, "pitch", 1f);
			bool isLooping = args.Get(3, "loop", true);
			bool isImmediate = args.Get(4, "immediate", false);
			float fadeSpeed = args.Get(5, "speed", 0f);
			int layerNum = args.Get(6, "layer", 0);

			audioManager.Play(AudioType.Ambient, fileName, volume, pitch, isLooping, isImmediate, fadeSpeed, layerNum);
		}

		static void PlayMusic(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			float volume = args.Get(1, "volume", 0.5f);
			float pitch = args.Get(2, "pitch", 1f);
			bool isLooping = args.Get(3, "loop", true);
			bool isImmediate = args.Get(4, "immediate", false);
			float fadeSpeed = args.Get(5, "speed", 0f);
			int layerNum = args.Get(6, "layer", 0);

			audioManager.Play(AudioType.Music, fileName, volume, pitch, isLooping, isImmediate, fadeSpeed, layerNum);
		}

		static void PlaySFX(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			float volume = args.Get(1, "volume", 0.5f);
			float pitch = args.Get(2, "pitch", 1f);
			bool isLooping = args.Get(3, "loop", false);
			bool isImmediate = args.Get(4, "immediate", true);
			float fadeSpeed = args.Get(5, "speed", 0f);

			audioManager.Play(AudioType.SFX, fileName, volume, pitch, isLooping, isImmediate, fadeSpeed, 0);
		}

		static void PlayVoice(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			float volume = args.Get(1, "volume", 0.5f);
			float pitch = args.Get(2, "pitch", 1f);
			bool isLooping = args.Get(3, "loop", false);
			bool isImmediate = args.Get(4, "immediate", true);
			float fadeSpeed = args.Get(5, "speed", 0f);
			int layerNum = args.Get(6, "layer", 0);

			audioManager.Play(AudioType.Voice, fileName, volume, pitch, isLooping, isImmediate, fadeSpeed, layerNum);
		}


		/***** Stop Audio *****/

		static void StopAmbient(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			bool isImmediate = args.Get(1, "immediate", false);
			float fadeSpeed = args.Get(2, "speed", 0f);
			int layerNum = args.Get(3, "layer", 0);

			audioManager.Stop(AudioType.Ambient, fileName, isImmediate, fadeSpeed, layerNum);
		}

		static void StopMusic(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			bool isImmediate = args.Get(1, "immediate", false);
			float fadeSpeed = args.Get(2, "speed", 0f);
			int layerNum = args.Get(3, "layer", 0);

			audioManager.Stop(AudioType.Music, fileName, isImmediate, fadeSpeed, layerNum);
		}

		static void StopSFX(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			bool isImmediate = args.Get(1, "immediate", false);
			float fadeSpeed = args.Get(2, "speed", 0f);

			audioManager.Stop(AudioType.SFX, fileName, isImmediate, fadeSpeed, 0);
		}

		static void StopVoice(DialogueCommandArguments args)
		{
			string fileName = args.Get(0, "name", "");
			bool isImmediate = args.Get(1, "immediate", false);
			float fadeSpeed = args.Get(2, "speed", 0f);
			int layerNum = args.Get(3, "layer", 0);

			audioManager.Stop(AudioType.Voice, fileName, isImmediate, fadeSpeed, layerNum);
		}
	}
}
