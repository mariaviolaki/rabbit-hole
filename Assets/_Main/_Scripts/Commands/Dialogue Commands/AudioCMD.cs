using Audio;
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
			directory.AddCommand("CreateAmbient", new Action<string[]>(CreateAmbient));
			directory.AddCommand("CreateMusic", new Action<string[]>(CreateMusic));
			directory.AddCommand("CreateSFX", new Action<string[]>(CreateSFX));
			directory.AddCommand("CreateVoice", new Action<string[]>(CreateVoice));

			// Play Instantly
			directory.AddCommand("PlayAmbientInstant", new Action<string[]>(PlayAmbientInstant));
			directory.AddCommand("PlayMusicInstant", new Action<string[]>(PlayMusicInstant));
			directory.AddCommand("PlaySFXInstant", new Action<string[]>(PlaySFXInstant));
			directory.AddCommand("PlayVoiceInstant", new Action<string[]>(PlayVoiceInstant));

			// Play and Fade In
			directory.AddCommand("PlayAmbient", new Action<string[]>(PlayAmbient));
			directory.AddCommand("PlayMusic", new Action<string[]>(PlayMusic));
			directory.AddCommand("PlaySFX", new Action<string[]>(PlaySFX));
			directory.AddCommand("PlayVoice", new Action<string[]>(PlayVoice));

			// Stop Instantly
			directory.AddCommand("StopAmbientInstant", new Action<string[]>(StopAmbientInstant));
			directory.AddCommand("StopMusicInstant", new Action<string[]>(StopMusicInstant));
			directory.AddCommand("StopSFXInstant", new Action<string[]>(StopSFXInstant));
			directory.AddCommand("StopVoiceInstant", new Action<string[]>(StopVoiceInstant));

			// Fade Out and Stop
			directory.AddCommand("StopAmbient", new Action<string[]>(StopAmbient));
			directory.AddCommand("StopMusic", new Action<string[]>(StopMusic));
			directory.AddCommand("StopSFX", new Action<string[]>(StopSFX));
			directory.AddCommand("StopVoice", new Action<string[]>(StopVoice));
		}


		/***** Initialize Layers *****/

		static void CreateAmbient(string[] args)
		{
			int layerCount = args.Length > 0 ? ParseArgument<int>(args[0]) : 1;

			audioManager.Create(Audio.AudioType.Ambient, layerCount);
		}

		static void CreateMusic(string[] args)
		{
			int layerCount = args.Length > 0 ? ParseArgument<int>(args[0]) : 1;

			audioManager.Create(Audio.AudioType.Music, layerCount);
		}

		static void CreateSFX(string[] args)
		{
			audioManager.Create(Audio.AudioType.SFX, 1);
		}

		static void CreateVoice(string[] args)
		{
			int layerCount = args.Length > 0 ? ParseArgument<int>(args[0]) : 1;

			audioManager.Create(Audio.AudioType.Voice, layerCount);
		}


		/***** Play Instantly *****/

		static void PlayAmbientInstant(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : true;
			int layerNum = args.Length > 4 ? ParseArgument<int>(args[4]) : 0;

			audioManager.PlayInstant(Audio.AudioType.Ambient, fileName, volume, pitch, isLooping, layerNum);
		}

		static void PlayMusicInstant(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : true;
			int layerNum = args.Length > 4 ? ParseArgument<int>(args[4]) : 0;

			audioManager.PlayInstant(Audio.AudioType.Music, fileName, volume, pitch, isLooping, layerNum);
		}

		static void PlaySFXInstant(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : false;

			audioManager.PlayInstant(Audio.AudioType.SFX, fileName, volume, pitch, isLooping, 0);
		}

		static void PlayVoiceInstant(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : false;
			int layerNum = args.Length > 4 ? ParseArgument<int>(args[4]) : 0;

			audioManager.PlayInstant(Audio.AudioType.Voice, fileName, volume, pitch, isLooping, layerNum);
		}


		/***** Play and Fade In *****/

		static void PlayAmbient(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : true;
			float fadeSpeed = args.Length > 4 ? ParseArgument<float>(args[4]) : 0;
			int layerNum = args.Length > 5 ? ParseArgument<int>(args[5]) : 0;

			audioManager.Play(Audio.AudioType.Ambient, fileName, volume, pitch, isLooping, fadeSpeed, layerNum);
		}

		static void PlayMusic(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : true;
			float fadeSpeed = args.Length > 4 ? ParseArgument<float>(args[4]) : 0;
			int layerNum = args.Length > 5 ? ParseArgument<int>(args[5]) : 0;

			audioManager.Play(Audio.AudioType.Music, fileName, volume, pitch, isLooping, fadeSpeed, layerNum);
		}

		static void PlaySFX(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : false;
			float fadeSpeed = args.Length > 4 ? ParseArgument<float>(args[4]) : -1;

			audioManager.Play(Audio.AudioType.SFX, fileName, volume, pitch, isLooping, fadeSpeed, 0);
		}

		static void PlayVoice(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : false;
			float fadeSpeed = args.Length > 4 ? ParseArgument<float>(args[4]) : -1;
			int layerNum = args.Length > 5 ? ParseArgument<int>(args[5]) : 0;

			audioManager.Play(Audio.AudioType.Voice, fileName, volume, pitch, isLooping, fadeSpeed, layerNum);
		}


		/***** Stop Instantly *****/

		static void StopAmbientInstant(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			int layerNum = args.Length > 1 ? ParseArgument<int>(args[1]) : 0;

			audioManager.StopInstant(Audio.AudioType.Ambient, fileName, layerNum);
		}

		static void StopMusicInstant(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			int layerNum = args.Length > 1 ? ParseArgument<int>(args[1]) : 0;

			audioManager.StopInstant(Audio.AudioType.Music, fileName, layerNum);
		}

		static void StopSFXInstant(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;

			audioManager.StopInstant(Audio.AudioType.SFX, fileName, 0);
		}

		static void StopVoiceInstant(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			int layerNum = args.Length > 1 ? ParseArgument<int>(args[1]) : 0;

			audioManager.StopInstant(Audio.AudioType.Voice, fileName, layerNum);
		}


		/***** Fade Out and Stop *****/

		static void StopAmbient(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float fadeSpeed = args.Length > 1 ? ParseArgument<float>(args[1]) : 0;
			int layerNum = args.Length > 2 ? ParseArgument<int>(args[2]) : 0;

			audioManager.Stop(Audio.AudioType.Ambient, fileName, fadeSpeed, layerNum);
		}

		static void StopMusic(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float fadeSpeed = args.Length > 1 ? ParseArgument<float>(args[1]) : 0;
			int layerNum = args.Length > 2 ? ParseArgument<int>(args[2]) : 0;

			audioManager.Stop(Audio.AudioType.Music, fileName, fadeSpeed, layerNum);
		}

		static void StopSFX(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float fadeSpeed = args.Length > 1 ? ParseArgument<float>(args[1]) : 0;

			audioManager.Stop(Audio.AudioType.SFX, fileName, fadeSpeed, 0);
		}

		static void StopVoice(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : null;
			float fadeSpeed = args.Length > 1 ? ParseArgument<float>(args[1]) : 0;
			int layerNum = args.Length > 2 ? ParseArgument<int>(args[2]) : 0;

			audioManager.Stop(Audio.AudioType.Voice, fileName, fadeSpeed, layerNum);
		}
	}
}
