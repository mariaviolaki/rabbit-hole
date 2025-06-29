using Dialogue;
using Visuals;
using System;
using UnityEngine;

namespace Commands
{
	public class VisualsCMD : DialogueCommand
	{
		static VisualGroupManager visualGroupManager;

		public new static void Register(CommandManager commandManager)
		{
			visualGroupManager = commandManager.Visuals;

			CommandBank bank = commandManager.GetBank(CommandManager.MainBankName);

			// Background
			bank.AddCommand("CreateBackground", new Action<DialogueCommandArguments>(CreateBackground));
			bank.AddCommand("ClearBackground", new Func<DialogueCommandArguments, Coroutine>(ClearBackground), CommandSkipType.Transition);
			bank.AddCommand("SetBackgroundImage", new Func<DialogueCommandArguments, Coroutine>(SetBackgroundImage), CommandSkipType.Transition);
			bank.AddCommand("SetBackgroundVideo", new Func<DialogueCommandArguments, Coroutine>(SetBackgroundVideo), CommandSkipType.Transition);

			// Foreground
			bank.AddCommand("CreateForeground", new Action<DialogueCommandArguments>(CreateForeground));
			bank.AddCommand("ClearForeground", new Func<DialogueCommandArguments, Coroutine>(ClearForeground), CommandSkipType.Transition);
			bank.AddCommand("SetForegroundImage", new Func<DialogueCommandArguments, Coroutine>(SetForegroundImage), CommandSkipType.Transition);
			bank.AddCommand("SetForegroundVideo", new Func<DialogueCommandArguments, Coroutine>(SetForegroundVideo), CommandSkipType.Transition);

			// Cinematic
			bank.AddCommand("CreateCinematic", new Action<DialogueCommandArguments>(CreateCinematic));
			bank.AddCommand("ClearCinematic", new Func<DialogueCommandArguments, Coroutine>(ClearCinematic), CommandSkipType.Transition);
			bank.AddCommand("SetCinematicImage", new Func<DialogueCommandArguments, Coroutine>(SetCinematicImage), CommandSkipType.Transition);
			bank.AddCommand("SetCinematicVideo", new Func<DialogueCommandArguments, Coroutine>(SetCinematicVideo), CommandSkipType.Transition);
		}


		/***** Background *****/

		static void CreateBackground(DialogueCommandArguments args)
		{
			CreateVisualGroup(VisualType.Background, args);
		}

		static Coroutine ClearBackground(DialogueCommandArguments args)
		{
			return ClearVisualGroup(VisualType.Background, args);
		}

		static Coroutine SetBackgroundImage(DialogueCommandArguments args)
		{
			return SetVisualGroupImage(VisualType.Background, args);
		}

		static Coroutine SetBackgroundVideo(DialogueCommandArguments args)
		{
			return SetVisualGroupVideo(VisualType.Background, args);
		}


		/***** Foreground *****/

		static void CreateForeground(DialogueCommandArguments args)
		{
			CreateVisualGroup(VisualType.Foreground, args);
		}

		static Coroutine ClearForeground(DialogueCommandArguments args)
		{
			return ClearVisualGroup(VisualType.Foreground, args);
		}

		static Coroutine SetForegroundImage(DialogueCommandArguments args)
		{
			return SetVisualGroupImage(VisualType.Foreground, args);
		}

		static Coroutine SetForegroundVideo(DialogueCommandArguments args)
		{
			return SetVisualGroupVideo(VisualType.Foreground, args);
		}


		/***** Cinematic *****/

		static void CreateCinematic(DialogueCommandArguments args)
		{
			CreateVisualGroup(VisualType.Cinematic, args);
		}

		static Coroutine ClearCinematic(DialogueCommandArguments args)
		{
			return ClearVisualGroup(VisualType.Cinematic, args);
		}

		static Coroutine SetCinematicImage(DialogueCommandArguments args)
		{
			return SetVisualGroupImage(VisualType.Cinematic, args);
		}

		static Coroutine SetCinematicVideo(DialogueCommandArguments args)
		{
			return SetVisualGroupVideo(VisualType.Cinematic, args);
		}


		/***** All Visual Groups *****/

		static void CreateVisualGroup(VisualType visualType, DialogueCommandArguments args)
		{
			int layerCount = args.Get(0, "layers", 1);

			visualGroupManager.Create(visualType, layerCount);
		}

		static Coroutine ClearVisualGroup(VisualType visualType, DialogueCommandArguments args)
		{
			int layerDepth = args.Get(0, "layer", -1);
			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			return visualGroupManager.Clear(visualType, layerDepth, isImmediate, speed);
		}

		static Coroutine SetVisualGroupImage(VisualType visualType, DialogueCommandArguments args)
		{
			string imageName = args.Get(0, "name", "");
			int layerDepth = args.Get(1, "layer", 0);
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			return visualGroupManager.SetImage(visualType, layerDepth, imageName, isImmediate, speed);
		}

		static Coroutine SetVisualGroupVideo(VisualType visualType, DialogueCommandArguments args)
		{
			string videoName = args.Get(0, "name", "");
			int layerDepth = args.Get(1, "layer", 0);
			bool isMuted = args.Get(2, "mute", false);
			bool isImmediate = args.Get(3, "immediate", false);
			float speed = args.Get(4, "speed", 0f);

			return visualGroupManager.SetVideo(visualType, layerDepth, videoName, isMuted, isImmediate, speed);
		}
	}
}
