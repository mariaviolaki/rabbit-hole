using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
	public class GraphicsGroupManager : MonoBehaviour
	{
		[SerializeField] GraphicsLayerGroup[] layerGroups;
		[SerializeField] FileManagerSO fileManager;
		[SerializeField] GameOptionsSO gameOptions;

		Dictionary<string, GraphicsLayerGroup> layerGroupDirectory = new Dictionary<string, GraphicsLayerGroup>();

		public FileManagerSO FileManager => fileManager;
		public GameOptionsSO GameOptions => gameOptions;

		void Awake()
		{
			foreach (GraphicsLayerGroup group in layerGroups)
			{
				group.Init(this);
				layerGroupDirectory.Add(group.Name, group);
			}
		}

		void Update()
		{
			// TODO remove test functionality
			if (Input.GetKeyDown(KeyCode.Space))
			{
				GraphicsLayerGroup layerGroup = GetLayerGroup("Background");
				if (layerGroup == null) return;

				layerGroup.CreateLayer(0);
				GraphicsLayer layer0 = layerGroup.GetLayer(0);
				layer0.SetImage("Red-Forest-Front");

				layerGroup.CreateLayer(10);
				GraphicsLayer layer1 = layerGroup.GetLayer(1);
				layer1.SetImage("Green-Forest-Back");
			}
			else if (Input.GetKeyDown(KeyCode.Q))
			{
				GraphicsLayerGroup layerGroup = GetLayerGroup("Background");
				GraphicsLayer layer1 = layerGroup.GetLayer(1);
				layer1.SetImage("Red-Forest-Back");
			}
			else if (Input.GetKeyDown(KeyCode.W))
			{
				GraphicsLayerGroup layerGroup = GetLayerGroup("Background");
				GraphicsLayer layer1 = layerGroup.GetLayer(1);
				layer1.SetImage("Green-Forest-Back");
			}
			else if (Input.GetKeyDown(KeyCode.E))
			{
				GraphicsLayerGroup layerGroup = GetLayerGroup("Background");
				GraphicsLayer layer1 = layerGroup.GetLayer(1);
				layer1.SetImageInstant("Yellow-Forest-Back");
			}
			else if (Input.GetKeyDown(KeyCode.R))
			{
				GraphicsLayerGroup layerGroup = GetLayerGroup("Foreground");
				layerGroup.CreateLayer(0);
				GraphicsLayer layer0 = layerGroup.GetLayer(0);
				layer0.SetImage("Green-Forest-Front");
			}
			else if (Input.GetKeyDown(KeyCode.A))
			{
				GraphicsLayerGroup layerGroup = GetLayerGroup("Background");
				GraphicsLayer layer1 = layerGroup.GetLayer(1);
				layer1.SetVideo("Day");
			}
			else if (Input.GetKeyDown(KeyCode.S))
			{
				GraphicsLayerGroup layerGroup = GetLayerGroup("Background");
				GraphicsLayer layer1 = layerGroup.GetLayer(1);
				layer1.SetVideo("Night");
			}
			else if (Input.GetKeyDown(KeyCode.D))
			{
				GraphicsLayerGroup layerGroup = GetLayerGroup("Background");
				GraphicsLayer layer1 = layerGroup.GetLayer(1);
				layer1.SetVideo("Day", false);
			}
		}

		GraphicsLayerGroup GetLayerGroup(string name)
		{
			if (!layerGroupDirectory.ContainsKey(name)) return null;

			return layerGroupDirectory[name];
		}
	}
}
