using Graphics;
using System;
using System.Collections;

namespace Commands
{
	public class VisualsCMD : DialogueCommand
	{
		static VisualGroupManager graphicsGroupManager;

		public new static void Register(CommandManager commandManager)
		{
			graphicsGroupManager = commandManager.GetGraphicsGroupManager();

			CommandDirectory directory = commandManager.GetDirectory(CommandManager.VisualsDirectoryName);

			// Cinematic
			directory.AddCommand("CreateCinematic", new Action<string[]>(CreateCinematic));
			directory.AddCommand("ClearCinematicInstant", new Action<string[]>(ClearCinematicInstant));
			directory.AddCommand("ClearCinematic", new Func<string[], IEnumerator>(ClearCinematic), new Action<string[]>(ClearCinematicInstant));
			directory.AddCommand("SetCinematicImageInstant", new Action<string[]>(SetCinematicImageInstant));
			directory.AddCommand("SetCinematicImage", new Func<string[], IEnumerator>(SetCinematicImage), new Action<string[]>(SetCinematicImageInstant));
			directory.AddCommand("SetCinematicVideoInstant", new Action<string[]>(SetCinematicVideoInstant));
			directory.AddCommand("SetCinematicVideo", new Func<string[], IEnumerator>(SetCinematicVideo), new Action<string[]>(SetCinematicVideoInstant));

			// Foreground
			directory.AddCommand("CreateForeground", new Action<string[]>(CreateForeground));
			directory.AddCommand("ClearForegroundInstant", new Action<string[]>(ClearForegroundInstant));
			directory.AddCommand("ClearForeground", new Func<string[], IEnumerator>(ClearForeground), new Action<string[]>(ClearForegroundInstant));
			directory.AddCommand("SetForegroundImageInstant", new Action<string[]>(SetForegroundImageInstant));
			directory.AddCommand("SetForegroundImage", new Func<string[], IEnumerator>(SetForegroundImage), new Action<string[]>(SetForegroundImageInstant));
			directory.AddCommand("SetForegroundVideoInstant", new Action<string[]>(SetForegroundVideoInstant));
			directory.AddCommand("SetForegroundVideo", new Func<string[], IEnumerator>(SetForegroundVideo), new Action<string[]>(SetForegroundVideoInstant));

			// Background
			directory.AddCommand("CreateBackground", new Action<string[]>(CreateBackground));
			directory.AddCommand("ClearBackgroundInstant", new Action<string[]>(ClearBackgroundInstant));
			directory.AddCommand("ClearBackground", new Func<string[], IEnumerator>(ClearBackground), new Action<string[]>(ClearBackgroundInstant));
			directory.AddCommand("SetBackgroundImageInstant", new Action<string[]>(SetBackgroundImageInstant));
			directory.AddCommand("SetBackgroundImage", new Func<string[], IEnumerator>(SetBackgroundImage), new Action<string[]>(SetBackgroundImageInstant));
			directory.AddCommand("SetBackgroundVideoInstant", new Action<string[]>(SetBackgroundVideoInstant));
			directory.AddCommand("SetBackgroundVideo", new Func<string[], IEnumerator>(SetBackgroundVideo), new Action<string[]>(SetBackgroundVideoInstant));
		}


		/***** Cinematic *****/

		static void CreateCinematic(string[] args)
		{
			CreateVisualGroup(VisualGroupManager.CinematicName, args);
		}

		static void ClearCinematicInstant(string[] args)
		{
			ClearVisualGroupInstant(VisualGroupManager.CinematicName, args);
		}

		static IEnumerator ClearCinematic(string[] args)
		{
			yield return ClearVisualGroup(VisualGroupManager.CinematicName, args);
		}

		static void SetCinematicImageInstant(string[] args)
		{
			SetVisualGroupImageInstant(VisualGroupManager.CinematicName, args);
		}

		static IEnumerator SetCinematicImage(string[] args)
		{
			yield return SetVisualGroupImage(VisualGroupManager.CinematicName, args);
		}

		static void SetCinematicVideoInstant(string[] args)
		{
			SetVisualGroupVideoInstant(VisualGroupManager.CinematicName, args);
		}

		static IEnumerator SetCinematicVideo(string[] args)
		{
			yield return SetVisualGroupVideo(VisualGroupManager.CinematicName, args);
		}


		/***** Foreground *****/

		static void CreateForeground(string[] args)
		{
			CreateVisualGroup(VisualGroupManager.ForegroundName, args);
		}

		static void ClearForegroundInstant(string[] args)
		{
			ClearVisualGroupInstant(VisualGroupManager.ForegroundName, args);
		}

		static IEnumerator ClearForeground(string[] args)
		{
			yield return ClearVisualGroup(VisualGroupManager.ForegroundName, args);
		}

		static void SetForegroundImageInstant(string[] args)
		{
			SetVisualGroupImageInstant(VisualGroupManager.ForegroundName, args);
		}

		static IEnumerator SetForegroundImage(string[] args)
		{
			yield return SetVisualGroupImage(VisualGroupManager.ForegroundName, args);
		}

		static void SetForegroundVideoInstant(string[] args)
		{
			SetVisualGroupVideoInstant(VisualGroupManager.ForegroundName, args);
		}

		static IEnumerator SetForegroundVideo(string[] args)
		{
			yield return SetVisualGroupVideo(VisualGroupManager.ForegroundName, args);
		}


		/***** Background *****/

		static void CreateBackground(string[] args)
		{
			CreateVisualGroup(VisualGroupManager.BackgroundName, args);
		}

		static void ClearBackgroundInstant(string[] args)
		{
			ClearVisualGroupInstant(VisualGroupManager.BackgroundName, args);
		}

		static IEnumerator ClearBackground(string[] args)
		{
			yield return ClearVisualGroup(VisualGroupManager.BackgroundName, args);
		}

		static void SetBackgroundImageInstant(string[] args)
		{
			SetVisualGroupImageInstant(VisualGroupManager.BackgroundName, args);
		}

		static IEnumerator SetBackgroundImage(string[] args)
		{
			yield return SetVisualGroupImage(VisualGroupManager.BackgroundName, args);
		}

		static void SetBackgroundVideoInstant(string[] args)
		{
			SetVisualGroupVideoInstant(VisualGroupManager.BackgroundName, args);
		}

		static IEnumerator SetBackgroundVideo(string[] args)
		{
			yield return SetVisualGroupVideo(VisualGroupManager.BackgroundName, args);
		}


		/***** All Visual Groups *****/

		static void CreateVisualGroup(string visualGroupName, string[] args)
		{
			VisualLayerGroup layerGroup = graphicsGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) return;

			int layerCount = args.Length > 0 ? ParseArgument<int>(args[0]) : default;
			layerGroup.CreateLayers(layerCount);
		}

		static void ClearVisualGroupInstant(string visualGroupName, string[] args)
		{
			VisualLayerGroup layerGroup = graphicsGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) return;

			int layerDepth = args.Length > 0 ? ParseArgument<int>(args[0]) : -1;
			layerGroup.ClearInstant(layerDepth);
		}

		static IEnumerator ClearVisualGroup(string visualGroupName, string[] args)
		{
			VisualLayerGroup layerGroup = graphicsGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) yield break;

			int layerDepth = args.Length > 0 ? ParseArgument<int>(args[0]) : -1;
			float speed = args.Length > 1 ? ParseArgument<float>(args[1]) : default;

			yield return layerGroup.Clear(layerDepth, speed);
		}

		static void SetVisualGroupImageInstant(string visualGroupName, string[] args)
		{
			VisualLayerGroup layerGroup = graphicsGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) return;

			string imageName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;
			int layerDepth = args.Length > 1 ? ParseArgument<int>(args[1]) : default;

			VisualLayer layer = layerGroup.GetLayer(layerDepth);
			if (layer == null) return;

			layer.SetImageInstant(imageName);
		}

		static IEnumerator SetVisualGroupImage(string visualGroupName, string[] args)
		{
			VisualLayerGroup layerGroup = graphicsGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) yield break;

			string imageName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;
			int layerDepth = args.Length > 1 ? ParseArgument<int>(args[1]) : default;
			float speed = args.Length > 2 ? ParseArgument<float>(args[2]) : default;

			VisualLayer layer = layerGroup.GetLayer(layerDepth);
			if (layer == null) yield break;

			yield return layer.SetImage(imageName, speed);
		}

		static void SetVisualGroupVideoInstant(string visualGroupName, string[] args)
		{
			VisualLayerGroup layerGroup = graphicsGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) return;

			string videoName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;
			int layerDepth = args.Length > 1 ? ParseArgument<int>(args[1]) : default;
			bool isMuted = args.Length > 2 ? ParseArgument<bool>(args[2]) : false;

			VisualLayer layer = layerGroup.GetLayer(layerDepth);
			if (layer == null) return;

			layer.SetVideoInstant(videoName, isMuted);
		}

		static IEnumerator SetVisualGroupVideo(string visualGroupName, string[] args)
		{
			VisualLayerGroup layerGroup = graphicsGroupManager.GetLayerGroup(visualGroupName);
			if (layerGroup == null) yield break;

			string videoName = args.Length > 0 ? ParseArgument<string>(args[0]) : default;
			int layerDepth = args.Length > 1 ? ParseArgument<int>(args[1]) : default;
			bool isMuted = args.Length > 2 ? ParseArgument<bool>(args[2]) : false;
			float speed = args.Length > 3 ? ParseArgument<float>(args[3]) : default;

			VisualLayer layer = layerGroup.GetLayer(layerDepth);
			if (layer == null) yield break;

			yield return layer.SetVideo(videoName, isMuted, speed);
		}
	}
}
