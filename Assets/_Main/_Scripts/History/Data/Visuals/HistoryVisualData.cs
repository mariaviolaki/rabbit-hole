using System.Collections.Generic;
using UnityEngine;
using Visuals;

namespace History
{
	[System.Serializable]
	public class HistoryVisualData
	{
		[SerializeField] List<HistoryVisual> visuals = new();

		public HistoryVisualData(VisualGroupManager visualGroupManager)
		{
			// Only cache the layers with active visuals to save memory
			foreach (VisualLayerGroup visualGroup in visualGroupManager.VisualGroups.Values)
			{
				foreach (VisualLayer visualLayer in visualGroup.Layers.Values)
				{
					if (visualLayer.VisualName == null) continue;

					HistoryVisual historyVisual = new();
					visuals.Add(historyVisual);

					historyVisual.visualType = visualGroup.Type;
					historyVisual.layerDepth = visualLayer.Depth;
					historyVisual.isImage = visualLayer.IsImage;
					historyVisual.visualName = visualLayer.VisualName;
					historyVisual.isMuted = visualLayer.IsMuted;
				}
			}
		}

		public void Load(VisualGroupManager visualManager, GameOptionsSO gameOptions)
		{
			float fadeSpeed = gameOptions.General.SkipTransitionSpeed;

			// Mirror the stucture of visuals used in the manager for efficient lookups
			Dictionary<VisualType, Dictionary<int, HistoryVisual>> mappedVisuals = GetMappedVisuals();

			// Iterate through all the visual layers in the manager and update those which are different from the history state
			foreach (VisualLayerGroup visualGroup in visualManager.VisualGroups.Values)
			{
				foreach (VisualLayer visualLayer in visualGroup.Layers.Values)
				{
					HistoryVisual historyVisual = null;
					if (mappedVisuals.TryGetValue(visualGroup.Type, out Dictionary<int, HistoryVisual> visualLayers))
						visualLayers.TryGetValue(visualLayer.Depth, out historyVisual);

					if (historyVisual?.visualName == visualLayer.VisualName) continue;

					if (historyVisual == null)
					{
						visualManager.Clear(visualGroup.Type, visualLayer.Depth, false, fadeSpeed);
					}
					else if (historyVisual.isImage)
					{
						visualManager.SetImage(
							historyVisual.visualType, historyVisual.layerDepth, historyVisual.visualName, false, fadeSpeed);
					}
					else
					{
						visualManager.SetVideo(
							historyVisual.visualType, historyVisual.layerDepth, historyVisual.visualName, historyVisual.isMuted, false, fadeSpeed);
					}
				}
			}
		}

		Dictionary<VisualType, Dictionary<int, HistoryVisual>> GetMappedVisuals()
		{
			Dictionary<VisualType, Dictionary<int, HistoryVisual>> mappedVisuals = new();
			foreach (HistoryVisual historyVisual in visuals)
			{
				if (!mappedVisuals.TryGetValue(historyVisual.visualType, out var visualLayers))
					mappedVisuals[historyVisual.visualType] = visualLayers = new();

				visualLayers.TryAdd(historyVisual.layerDepth, historyVisual);
			}

			return mappedVisuals;
		}
	}
}
