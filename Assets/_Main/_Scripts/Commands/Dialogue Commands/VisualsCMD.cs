using Dialogue;
using Visuals;
using System;
using System.Collections;

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
			bank.AddCommand("ClearBackground", new Func<DialogueCommandArguments, IEnumerator>(ClearBackground), CommandSkipType.Transition);
			bank.AddCommand("SetBackgroundImage", new Func<DialogueCommandArguments, IEnumerator>(SetBackgroundImage), CommandSkipType.Transition);
			bank.AddCommand("SetBackgroundVideo", new Func<DialogueCommandArguments, IEnumerator>(SetBackgroundVideo), CommandSkipType.Transition);

			// Foreground
			bank.AddCommand("CreateForeground", new Action<DialogueCommandArguments>(CreateForeground));
			bank.AddCommand("ClearForeground", new Func<DialogueCommandArguments, IEnumerator>(ClearForeground), CommandSkipType.Transition);
			bank.AddCommand("SetForegroundImage", new Func<DialogueCommandArguments, IEnumerator>(SetForegroundImage), CommandSkipType.Transition);
			bank.AddCommand("SetForegroundVideo", new Func<DialogueCommandArguments, IEnumerator>(SetForegroundVideo), CommandSkipType.Transition);

			// Cinematic
			bank.AddCommand("CreateCinematic", new Action<DialogueCommandArguments>(CreateCinematic));
			bank.AddCommand("ClearCinematic", new Func<DialogueCommandArguments, IEnumerator>(ClearCinematic), CommandSkipType.Transition);
			bank.AddCommand("SetCinematicImage", new Func<DialogueCommandArguments, IEnumerator>(SetCinematicImage), CommandSkipType.Transition);
			bank.AddCommand("SetCinematicVideo", new Func<DialogueCommandArguments, IEnumerator>(SetCinematicVideo), CommandSkipType.Transition);
		}


		/***** Background *****/

		static void CreateBackground(DialogueCommandArguments args)
		{
			CreateVisualGroup(VisualType.Background, args);
		}

		static IEnumerator ClearBackground(DialogueCommandArguments args)
		{
			yield return ClearVisualGroup(VisualType.Background, args);
		}

		static IEnumerator SetBackgroundImage(DialogueCommandArguments args)
		{
			yield return SetVisualGroupImage(VisualType.Background, args);
		}

		static IEnumerator SetBackgroundVideo(DialogueCommandArguments args)
		{
			yield return SetVisualGroupVideo(VisualType.Background, args);
		}


		/***** Foreground *****/

		static void CreateForeground(DialogueCommandArguments args)
		{
			CreateVisualGroup(VisualType.Foreground, args);
		}

		static IEnumerator ClearForeground(DialogueCommandArguments args)
		{
			yield return ClearVisualGroup(VisualType.Foreground, args);
		}

		static IEnumerator SetForegroundImage(DialogueCommandArguments args)
		{
			yield return SetVisualGroupImage(VisualType.Foreground, args);
		}

		static IEnumerator SetForegroundVideo(DialogueCommandArguments args)
		{
			yield return SetVisualGroupVideo(VisualType.Foreground, args);
		}


		/***** Cinematic *****/

		static void CreateCinematic(DialogueCommandArguments args)
		{
			CreateVisualGroup(VisualType.Cinematic, args);
		}

		static IEnumerator ClearCinematic(DialogueCommandArguments args)
		{
			yield return ClearVisualGroup(VisualType.Cinematic, args);
		}

		static IEnumerator SetCinematicImage(DialogueCommandArguments args)
		{
			yield return SetVisualGroupImage(VisualType.Cinematic, args);
		}

		static IEnumerator SetCinematicVideo(DialogueCommandArguments args)
		{
			yield return SetVisualGroupVideo(VisualType.Cinematic, args);
		}


		/***** All Visual Groups *****/

		static void CreateVisualGroup(VisualType visualType, DialogueCommandArguments args)
		{
			int layerCount = args.Get(0, "layers", 1);

			visualGroupManager.Create(visualType, layerCount);
		}

		static IEnumerator ClearVisualGroup(VisualType visualType, DialogueCommandArguments args)
		{
			int layerDepth = args.Get(0, "layer", -1);
			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			yield return visualGroupManager.Clear(visualType, layerDepth, isImmediate, speed);
		}

		static IEnumerator SetVisualGroupImage(VisualType visualType, DialogueCommandArguments args)
		{
			string imageName = args.Get(0, "name", "");
			int layerDepth = args.Get(1, "layer", 0);
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			yield return visualGroupManager.SetImage(visualType, layerDepth, imageName, isImmediate, speed);
		}

		static IEnumerator SetVisualGroupVideo(VisualType visualType, DialogueCommandArguments args)
		{
			string videoName = args.Get(0, "name", "");
			int layerDepth = args.Get(1, "layer", 0);
			bool isMuted = args.Get(2, "mute", false);
			bool isImmediate = args.Get(3, "immediate", false);
			float speed = args.Get(4, "speed", 0f);

			yield return visualGroupManager.SetVideo(visualType, layerDepth, videoName, isMuted, isImmediate, speed);
		}
	}
}
