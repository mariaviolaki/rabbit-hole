using Dialogue;
using Graphics;
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
			CreateVisualGroup(VisualGroupManager.BackgroundName, args);
		}

		static IEnumerator ClearBackground(DialogueCommandArguments args)
		{
			yield return ClearVisualGroup(VisualGroupManager.BackgroundName, args);
		}

		static IEnumerator SetBackgroundImage(DialogueCommandArguments args)
		{
			yield return SetVisualGroupImage(VisualGroupManager.BackgroundName, args);
		}

		static IEnumerator SetBackgroundVideo(DialogueCommandArguments args)
		{
			yield return SetVisualGroupVideo(VisualGroupManager.BackgroundName, args);
		}


		/***** Foreground *****/

		static void CreateForeground(DialogueCommandArguments args)
		{
			CreateVisualGroup(VisualGroupManager.ForegroundName, args);
		}

		static IEnumerator ClearForeground(DialogueCommandArguments args)
		{
			yield return ClearVisualGroup(VisualGroupManager.ForegroundName, args);
		}

		static IEnumerator SetForegroundImage(DialogueCommandArguments args)
		{
			yield return SetVisualGroupImage(VisualGroupManager.ForegroundName, args);
		}

		static IEnumerator SetForegroundVideo(DialogueCommandArguments args)
		{
			yield return SetVisualGroupVideo(VisualGroupManager.ForegroundName, args);
		}


		/***** Cinematic *****/

		static void CreateCinematic(DialogueCommandArguments args)
		{
			CreateVisualGroup(VisualGroupManager.CinematicName, args);
		}

		static IEnumerator ClearCinematic(DialogueCommandArguments args)
		{
			yield return ClearVisualGroup(VisualGroupManager.CinematicName, args);
		}

		static IEnumerator SetCinematicImage(DialogueCommandArguments args)
		{
			yield return SetVisualGroupImage(VisualGroupManager.CinematicName, args);
		}

		static IEnumerator SetCinematicVideo(DialogueCommandArguments args)
		{
			yield return SetVisualGroupVideo(VisualGroupManager.CinematicName, args);
		}


		/***** All Visual Groups *****/

		static void CreateVisualGroup(string visualGroupName, DialogueCommandArguments args)
		{
			VisualLayerGroup layerGroup = visualGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) return;

			int layerCount = args.Get(0, "layers", 1);

			layerGroup.CreateLayers(layerCount);
		}

		static IEnumerator ClearVisualGroup(string visualGroupName, DialogueCommandArguments args)
		{
			VisualLayerGroup layerGroup = visualGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) yield break;

			int layerDepth = args.Get(0, "layer", -1);
			bool isImmediate = args.Get(1, "immediate", false);
			float speed = args.Get(2, "speed", 0f);

			yield return layerGroup.Clear(layerDepth, isImmediate, speed);
		}

		static IEnumerator SetVisualGroupImage(string visualGroupName, DialogueCommandArguments args)
		{
			VisualLayerGroup layerGroup = visualGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) yield break;

			string imageName = args.Get(0, "name", "");
			int layerDepth = args.Get(1, "layer", 0);
			bool isImmediate = args.Get(2, "immediate", false);
			float speed = args.Get(3, "speed", 0f);

			VisualLayer layer = layerGroup.GetLayer(layerDepth);
			if (layer == null) yield break;

			yield return layer.SetImage(imageName, isImmediate, speed);
		}

		static IEnumerator SetVisualGroupVideo(string visualGroupName, DialogueCommandArguments args)
		{
			VisualLayerGroup layerGroup = visualGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) yield break;

			string videoName = args.Get(0, "name", "");
			int layerDepth = args.Get(1, "layer", 0);
			bool isMuted = args.Get(2, "mute", false);
			bool isImmediate = args.Get(3, "immediate", false);
			float speed = args.Get(4, "speed", 0f);

			VisualLayer layer = layerGroup.GetLayer(layerDepth);
			if (layer == null) yield break;

			yield return layer.SetVideo(videoName, isMuted, isImmediate, speed);
		}
	}
}
