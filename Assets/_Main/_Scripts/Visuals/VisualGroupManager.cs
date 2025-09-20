using Dialogue;
using Game;
using Gameplay;
using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VN;

namespace Visuals
{
	public class VisualGroupManager : MonoBehaviour
	{
		[SerializeField] VisualLayerGroup[] layerGroups;
		[SerializeField] CGBankSO cgBank;
		[SerializeField] AssetManagerSO assetManager;
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] AssetLabelReference imageLabel;
		[SerializeField] AssetLabelReference cgLabel;
		[SerializeField] DialogueManager dialogueManager;

		GameProgressManager gameProgressManager;

		readonly Dictionary<VisualType, VisualLayerGroup> layerGroupBank = new();

		public AssetManagerSO Assets => assetManager;
		public VNOptionsSO Options => vnOptions;
		public AssetLabelReference ImageLabel => imageLabel;
		public AssetLabelReference CGLabel => cgLabel;
		public DialogueManager Dialogue => dialogueManager;

		public Dictionary<VisualType, VisualLayerGroup> VisualGroups => layerGroupBank;

		void Awake()
		{
			foreach (VisualLayerGroup group in layerGroups)
			{
				group.Initialize(this);
				group.CreateLayers(GetGroupLayerCount(group.Type));
				layerGroupBank.Add(group.Type, group);
			}
		}

		void Start()
		{
			gameProgressManager = FindAnyObjectByType<GameProgressManager>();
		}

		public VisualLayerGroup GetLayerGroup(VisualType visualType)
		{
			layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup);
			return visualLayerGroup;
		}

		public float GetTransitionSpeed(float speedInput, bool isTransitionSkipped)
		{
			if (isTransitionSkipped || dialogueManager.ReadMode == DialogueReadMode.Skip)
				return vnOptions.General.SkipTransitionSpeed;
			else if (speedInput <= 0)
				return vnOptions.Images.TransitionSpeed;
			else
				return speedInput;
		}

		public IEnumerator ShowCG(CharacterRoute route, int num, int stage = 0, bool isImmediate = false, float speed = 0)
		{
			CharacterCG characterCG = cgBank.GetCG(route, num, stage);
			if (characterCG == null) yield break;

			yield return assetManager.LoadImage(characterCG.ImageName, cgLabel);
			Sprite sprite = assetManager.GetImage(characterCG.ImageName, cgLabel);
			if (sprite == null) yield break;

			if (stage == 0)
				gameProgressManager.UnlockCG(new UnlockedCG(route, num));

			VisualLayerGroup visualLayerGroup = layerGroupBank[VisualType.CG];
			VisualLayer visualLayer = visualLayerGroup.GetLayer(0);
			if (visualLayer == null) yield break;

			speed = 10f;
			if (stage > 0)
				speed = 5f;

			visualLayer.SetImage(sprite, characterCG.ImageName, isImmediate, speed);
		}

		public void HideCG(bool isImmediate = false, float speed = 0)
		{
			VisualLayerGroup visualLayerGroup = layerGroupBank[VisualType.CG];
			VisualLayer visualLayer = visualLayerGroup.GetLayer(0);
			if (visualLayer == null) return;

			visualLayerGroup.Clear(0, isImmediate, speed);
		}

		public void Clear(VisualType visualType, int depth = -1, bool isImmediate = false, float speed = 0)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return;

			visualLayerGroup.Clear(depth, isImmediate, speed);
		}

		public IEnumerator SetImage(VisualType visualType, int layerDepth, string name, bool isImmediate = false, float speed = 0f)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) yield break;

			VisualLayer visualLayer = visualLayerGroup.GetLayer(layerDepth);
			if (visualLayer == null) yield break;

			// History will use this function to show an old visual instead of the specific ones for CGs
			AssetLabelReference assetLabel = visualLayerGroup.Type == VisualType.CG ? cgLabel : imageLabel;

			yield return assetManager.LoadImage(name, assetLabel);
			Sprite sprite = assetManager.GetImage(name, assetLabel);
			if (sprite == null) yield break;

			visualLayer.SetImage(sprite, name, isImmediate, speed);
		}

		public IEnumerator SetVideo(VisualType visualType, int layerDepth, string name, float volume = 0.5f, bool isMuted = false, bool isImmediate = false, float speed = 0f)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) yield break;

			VisualLayer visualLayer = visualLayerGroup.GetLayer(layerDepth);
			if (visualLayer == null) yield break;

			string path = assetManager.GetVideoUrl(name);
			if (path == null) yield break;

			yield return visualLayer.SetVideo(path, name, volume, isMuted, isImmediate, speed);
		}

		public void SkipCGTransition() => SkipTransition(VisualType.CG, 0);
		public void SkipTransition(VisualType visualType, int layerDepth)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return;

			visualLayerGroup.SkipTransitions(layerDepth);
		}

		public bool IsCGTransitioning() => IsTransitioning(VisualType.CG, 0);
		public bool IsTransitioning(VisualType visualType, int layerDepth)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return false;

			return visualLayerGroup.IsTransitioning(layerDepth);
		}

		int GetGroupLayerCount(VisualType visualType)
		{
			return visualType switch
			{
				VisualType.Background => vnOptions.Images.BackgroundLayers,
				VisualType.Foreground => vnOptions.Images.ForegroundLayers,
				VisualType.Cinematic => vnOptions.Images.CinematicLayers,
				_ => 1,
			};
		}
	}
}
