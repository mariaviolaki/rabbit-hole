using Dialogue;
using Visuals;
using System;
using UnityEngine;
using System.Collections;

namespace Commands
{
	public class VisualsCMD : DialogueCommand
	{
		static CommandManager commandManager;
		static VisualGroupManager visualGroupManager;

		public new static void Register(CommandManager manager)
		{
			commandManager = manager;
			visualGroupManager = manager.Visuals;

			CommandBank bank = manager.GetBank(CommandManager.MainBankName);

			// Background
			bank.AddCommand("CreateBackground", new Func<DialogueCommandArguments, CommandProcessBase>(CreateBackground));
			bank.AddCommand("ClearBackground", new Func<DialogueCommandArguments, CommandProcessBase>(ClearBackground));
			bank.AddCommand("SetBackgroundImage", new Func<DialogueCommandArguments, CommandProcessBase>(SetBackgroundImage));
			bank.AddCommand("SetBackgroundVideo", new Func<DialogueCommandArguments, CommandProcessBase>(SetBackgroundVideo));

			// Foreground
			bank.AddCommand("CreateForeground", new Func<DialogueCommandArguments, CommandProcessBase>(CreateForeground));
			bank.AddCommand("ClearForeground", new Func<DialogueCommandArguments, CommandProcessBase>(ClearForeground));
			bank.AddCommand("SetForegroundImage", new Func<DialogueCommandArguments, CommandProcessBase>(SetForegroundImage));
			bank.AddCommand("SetForegroundVideo", new Func<DialogueCommandArguments, CommandProcessBase>(SetForegroundVideo));

			// Cinematic
			bank.AddCommand("CreateCinematic", new Func<DialogueCommandArguments, CommandProcessBase>(CreateCinematic));
			bank.AddCommand("ClearCinematic", new Func<DialogueCommandArguments, CommandProcessBase>(ClearCinematic));
			bank.AddCommand("SetCinematicImage", new Func<DialogueCommandArguments, CommandProcessBase>(SetCinematicImage));
			bank.AddCommand("SetCinematicVideo", new Func<DialogueCommandArguments, CommandProcessBase>(SetCinematicVideo));
		}


		/***** Background *****/

		static CommandProcessBase CreateBackground(DialogueCommandArguments args)
		{
			return CreateVisualGroup(VisualType.Background, args);
		}

		static CommandProcessBase ClearBackground(DialogueCommandArguments args)
		{
			return ClearVisualGroup(VisualType.Background, args);
		}

		static CommandProcessBase SetBackgroundImage(DialogueCommandArguments args)
		{
			return SetVisualGroupImage(VisualType.Background, args);
		}

		static CommandProcessBase SetBackgroundVideo(DialogueCommandArguments args)
		{
			return SetVisualGroupVideo(VisualType.Background, args);
		}


		/***** Foreground *****/

		static CommandProcessBase CreateForeground(DialogueCommandArguments args)
		{
			return CreateVisualGroup(VisualType.Foreground, args);
		}

		static CommandProcessBase ClearForeground(DialogueCommandArguments args)
		{
			return ClearVisualGroup(VisualType.Foreground, args);
		}

		static CommandProcessBase SetForegroundImage(DialogueCommandArguments args)
		{
			return SetVisualGroupImage(VisualType.Foreground, args);
		}

		static CommandProcessBase SetForegroundVideo(DialogueCommandArguments args)
		{
			return SetVisualGroupVideo(VisualType.Foreground, args);
		}


		/***** Cinematic *****/

		static CommandProcessBase CreateCinematic(DialogueCommandArguments args)
		{
			return CreateVisualGroup(VisualType.Cinematic, args);
		}

		static CommandProcessBase ClearCinematic(DialogueCommandArguments args)
		{
			return ClearVisualGroup(VisualType.Cinematic, args);
		}

		static CommandProcessBase SetCinematicImage(DialogueCommandArguments args)
		{
			return SetVisualGroupImage(VisualType.Cinematic, args);
		}

		static CommandProcessBase SetCinematicVideo(DialogueCommandArguments args)
		{
			return SetVisualGroupVideo(VisualType.Cinematic, args);
		}


		/***** All Visual Groups *****/

		static CommandProcessBase CreateVisualGroup(VisualType visualType, DialogueCommandArguments args)
		{
			int layerCount = args.Get(0, "layers", 1);

			void action() => visualGroupManager.Create(visualType, layerCount);
			return new ActionCommandProcess(action);
		}

		static CommandProcessBase ClearVisualGroup(VisualType visualType, DialogueCommandArguments args)
		{
			int layerDepth = args.Get(0, "layer", -1);
			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			void runFunc() => visualGroupManager.Clear(visualType, layerDepth, isImmediate, speed);
			void skipFunc() => visualGroupManager.SkipTransition(visualType, layerDepth);
			bool isCompletedFunc() => !visualGroupManager.IsTransitioning(visualType, layerDepth);

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase SetVisualGroupImage(VisualType visualType, DialogueCommandArguments args)
		{
			string imageName = args.Get(0, "name", "");
			int layerDepth = args.Get(1, "layer", 0);
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			IEnumerator processFunc() => visualGroupManager.SetImage(visualType, layerDepth, imageName, isImmediate, speed);
			void skipFunc() => visualGroupManager.SkipTransition(visualType, layerDepth);
			bool isCompletedFunc() => !visualGroupManager.IsTransitioning(visualType, layerDepth);

			return new TransitionCommandProcess(commandManager, processFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase SetVisualGroupVideo(VisualType visualType, DialogueCommandArguments args)
		{
			string videoName = args.Get(0, "name", "");
			int layerDepth = args.Get(1, "layer", 0);
			float volume = args.Get(2, "volume", 0.5f);
			bool isMuted = args.Get(3, "mute", false);
			bool isImmediate = args.Get(4, "immediate", false);
			float speed = args.Get(5, "speed", 0f);

			IEnumerator processFunc() => visualGroupManager.SetVideo(visualType, layerDepth, videoName, volume, isMuted, isImmediate, speed);
			void skipFunc() => visualGroupManager.SkipTransition(visualType, layerDepth);
			bool isCompletedFunc() => !visualGroupManager.IsTransitioning(visualType, layerDepth);

			return new TransitionCommandProcess(commandManager, processFunc, skipFunc, isCompletedFunc);
		}
	}
}
