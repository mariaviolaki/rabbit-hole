using System;
using System.Threading.Tasks;

namespace Commands
{
	public class AudioCMD : DialogueCommand
	{
		static AudioManager audioManager;

		public new static void Register(CommandManager commandManager)
		{
			audioManager = commandManager.GetAudioManager();

			CommandDirectory directory = commandManager.GetDirectory(CommandManager.AudioDirectoryName);

			directory.AddCommand("PlayAmbient", new Func<string[], Task>(PlayAmbient));
			directory.AddCommand("PlayMusic", new Func<string[], Task>(PlayMusic));
			directory.AddCommand("PlaySFX", new Func<string[], Task>(PlaySFX));
			directory.AddCommand("PlayVoice", new Func<string[], Task>(PlayVoice));
			directory.AddCommand("StopAmbient", new Action<string[]>(StopAmbient));
			directory.AddCommand("StopMusic", new Action<string[]>(StopMusic));
			directory.AddCommand("StopSFX", new Action<string[]>(StopSFX));
			directory.AddCommand("StopVoice", new Action<string[]>(StopVoice));
		}

		static async Task PlayAmbient(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : true;

			await audioManager.PlayAmbient(fileName, volume, pitch, isLooping);
		}

		static async Task PlayMusic(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : true;

			await audioManager.PlayMusic(fileName, volume, pitch, isLooping);
		}

		static async Task PlaySFX(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : false;

			await audioManager.PlaySFX(fileName, volume, pitch, isLooping);
		}

		static async Task PlayVoice(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;
			float volume = args.Length > 1 ? ParseArgument<float>(args[1]) : 0.5f;
			float pitch = args.Length > 2 ? ParseArgument<float>(args[2]) : 1f;
			bool isLooping = args.Length > 3 ? ParseArgument<bool>(args[3]) : false;

			await audioManager.PlayVoice(fileName, volume, pitch, isLooping);
		}

		static void StopAmbient(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;

			audioManager.StopAmbient(fileName);
		}

		static void StopMusic(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;

			audioManager.StopMusic(fileName);
		}

		static void StopSFX(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;

			audioManager.StopSFX(fileName);
		}

		static void StopVoice(string[] args)
		{
			string fileName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;

			audioManager.StopVoice(fileName);
		}
	}
}
