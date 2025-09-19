using Dialogue;
using Gameplay;
using System;
using System.Collections;
using Visuals;

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

			// CGs
			bank.AddCommand("ShowCG", new Func<DialogueCommandArguments, CommandProcessBase>(ShowCG));
			bank.AddCommand("HideCG", new Func<DialogueCommandArguments, CommandProcessBase>(HideCG));

			// Background
			bank.AddCommand("SetBackground", new Func<DialogueCommandArguments, CommandProcessBase>(SetBackground));
			bank.AddCommand("ClearBackground", new Func<DialogueCommandArguments, CommandProcessBase>(ClearBackground));

			// Foreground
			bank.AddCommand("SetForeground", new Func<DialogueCommandArguments, CommandProcessBase>(SetForeground));
			bank.AddCommand("ClearForeground", new Func<DialogueCommandArguments, CommandProcessBase>(ClearForeground));

			// Cinematic
			bank.AddCommand("SetCinematic", new Func<DialogueCommandArguments, CommandProcessBase>(SetCinematic));
			bank.AddCommand("ClearCinematic", new Func<DialogueCommandArguments, CommandProcessBase>(ClearCinematic));			
		}


		/***** CGs *****/

		static CommandProcessBase ShowCG(DialogueCommandArguments args)
		{
			CharacterRoute route = args.Get(0, "route", CharacterRoute.Common);
			int num = args.Get(1, "num", 1);
			int stage = args.Get(2, "stage", 0);
			bool isImmediate = args.Get(3, "immediate", false);
			float speed = args.Get(4, "speed", 0f);

			IEnumerator processFunc() => visualGroupManager.ShowCG(route, num, stage, isImmediate, speed);
			void skipFunc() => visualGroupManager.SkipCGTransition();
			bool isCompletedFunc() => !visualGroupManager.IsCGTransitioning();

			return new TransitionCommandProcess(commandManager, processFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase HideCG(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float speed = args.Get(1, "speed", 0f);

			void runFunc() => visualGroupManager.HideCG(isImmediate, speed);
			void skipFunc() => visualGroupManager.SkipCGTransition();
			bool isCompletedFunc() => !visualGroupManager.IsCGTransitioning();

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}


		/***** Background *****/

		static CommandProcessBase SetBackground(DialogueCommandArguments args)
		{
			return SetVisual(VisualType.Background, args);
		}

		static CommandProcessBase ClearBackground(DialogueCommandArguments args)
		{
			return ClearVisual(VisualType.Background, args);
		}


		/***** Foreground *****/

		static CommandProcessBase SetForeground(DialogueCommandArguments args)
		{
			return SetVisual(VisualType.Foreground, args);
		}

		static CommandProcessBase ClearForeground(DialogueCommandArguments args)
		{
			return ClearVisual(VisualType.Foreground, args);
		}


		/***** Cinematic *****/

		static CommandProcessBase SetCinematic(DialogueCommandArguments args)
		{
			return SetVisual(VisualType.Cinematic, args);
		}

		static CommandProcessBase ClearCinematic(DialogueCommandArguments args)
		{
			return ClearVisual(VisualType.Cinematic, args);
		}


		/***** All Visual Groups *****/

		static CommandProcessBase SetVisual(VisualType visualType, DialogueCommandArguments args)
		{
			string name = args.Get(0, "name", "");
			int layerDepth = args.Get(1, "layer", 0);
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			// Video-only params
			bool isVideo = args.Get(4, "video", false);
			float volume = args.Get(5, "volume", 0.5f);
			bool isMuted = args.Get(6, "mute", false);

			IEnumerator processFunc() => isVideo
				? visualGroupManager.SetVideo(visualType, layerDepth, name, volume, isMuted, isImmediate, speed)
				: visualGroupManager.SetImage(visualType, layerDepth, name, isImmediate, speed);

			void skipFunc() => visualGroupManager.SkipTransition(visualType, layerDepth);
			bool isCompletedFunc() => !visualGroupManager.IsTransitioning(visualType, layerDepth);

			return new TransitionCommandProcess(commandManager, processFunc, skipFunc, isCompletedFunc);
		}

		static CommandProcessBase ClearVisual(VisualType visualType, DialogueCommandArguments args)
		{
			int layerDepth = args.Get(0, "layer", -1);
			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			void runFunc() => visualGroupManager.Clear(visualType, layerDepth, isImmediate, speed);
			void skipFunc() => visualGroupManager.SkipTransition(visualType, layerDepth);
			bool isCompletedFunc() => !visualGroupManager.IsTransitioning(visualType, layerDepth);

			return new TransitionCommandProcess(runFunc, skipFunc, isCompletedFunc);
		}
	}
}
