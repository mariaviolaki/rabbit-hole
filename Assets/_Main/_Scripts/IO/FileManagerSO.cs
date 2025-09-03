using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;

namespace IO
{
	[CreateAssetMenu(fileName = "File Manager", menuName = "Scriptable Objects/File Manager")]
	public class FileManagerSO : ScriptableObject
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] AssetLabelReference videoLabel;
		[SerializeField] AssetLabelReference ambientAudioLabel;
		[SerializeField] AssetLabelReference musicAudioLabel;
		[SerializeField] AssetLabelReference sfxAudioLabel;
		[SerializeField] AssetLabelReference voiceAudioLabel;
		[SerializeField] AssetLabelReference dialogueLabel;
		[SerializeField] AssetLabelReference imageLabel;
		[SerializeField] AssetLabelReference characterAtlasLabel;
		[SerializeField] AssetLabelReference model3DPrefabLabel;

		readonly Dictionary<AssetType, bool> initializedKeys = new();

		readonly Dictionary<string, string> videoPaths = new(StringComparer.OrdinalIgnoreCase);

		readonly Dictionary<string, string> audioKeys = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, string> dialogueKeys = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, string> imageKeys = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, string> characterAtlasKeys = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, string> model3DPrefabKeys = new(StringComparer.OrdinalIgnoreCase);

		readonly Dictionary<string, AsyncOperationHandle<AudioClip>> audioHandles = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, AsyncOperationHandle<TextAsset>> dialogueHandles = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, AsyncOperationHandle<Sprite>> imageHandles = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, AsyncOperationHandle<SpriteAtlas>> characterAtlasHandles = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, AsyncOperationHandle<GameObject>> model3DPrefabHandles = new(StringComparer.OrdinalIgnoreCase);

		public Dictionary<AssetType, bool> InitializedKeys => initializedKeys;
		public Dictionary<string, string> DialogueKeys => dialogueKeys;

		void OnEnable()
		{
			// Load the addressables's metadata to easily search and load them later
			CacheLabeledAssetsIntoDictionary(AssetType.Video, videoPaths, videoLabel);
			CacheLabeledAssetsIntoDictionary(AssetType.Audio, audioKeys, ambientAudioLabel, musicAudioLabel, sfxAudioLabel, voiceAudioLabel);
			CacheLabeledAssetsIntoDictionary(AssetType.Dialogue, dialogueKeys, dialogueLabel);
			CacheLabeledAssetsIntoDictionary(AssetType.Image, imageKeys, imageLabel);
			CacheLabeledAssetsIntoDictionary(AssetType.CharacterAtlas, characterAtlasKeys, characterAtlasLabel);
			CacheLabeledAssetsIntoDictionary(AssetType.Model3DPrefab, model3DPrefabKeys, model3DPrefabLabel);
		}

		public string GetAddressablePath(string fileName, AssetLabelReference assetLabel) => assetLabel.labelString + "/" + fileName;

		// TODO test if this works on other platforms
		public string GetVideoUrl(string fileName)
		{
			string addressablePath = GetAddressablePath(fileName, videoLabel);
			if (videoPaths.TryGetValue(addressablePath, out var videoPath)) return videoPath;

			Debug.LogWarning($"Unable to locate video path for '{fileName}'. Video URL could not be loaded.");
			return null;
		}

		public IEnumerator LoadAudio(string audioName, AssetLabelReference audioLabel) => LoadAsset(GetAddressablePath(audioName, audioLabel), audioKeys, audioHandles);
		public IEnumerator LoadDialogue(string addressablePath) => LoadAsset(addressablePath, dialogueKeys, dialogueHandles);
		public IEnumerator LoadImage(string imageName) => LoadAsset(GetAddressablePath(imageName, imageLabel), imageKeys, imageHandles);
		public IEnumerator LoadCharacterAtlas(string characterName) => LoadAsset(GetAddressablePath(characterName, characterAtlasLabel), characterAtlasKeys, characterAtlasHandles);
		public IEnumerator LoadModel3DPrefab(string characterName) => LoadAsset(GetAddressablePath(characterName, model3DPrefabLabel), model3DPrefabKeys, model3DPrefabHandles);

		public AudioClip GetAudio(string audioName, AssetLabelReference audioLabel) => GetAsset(GetAddressablePath(audioName, audioLabel), audioHandles);
		public TextAsset GetDialogueFile(string addressablePath) => GetAsset(addressablePath, dialogueHandles);
		public Sprite GetImage(string imageName) => GetAsset(GetAddressablePath(imageName, imageLabel), imageHandles);
		public SpriteAtlas GetCharacterAtlas(string characterName) => GetAsset(GetAddressablePath(characterName, characterAtlasLabel), characterAtlasHandles);
		public GameObject GetModel3DPrefab(string characterName) => GetAsset(GetAddressablePath(characterName, model3DPrefabLabel), model3DPrefabHandles);

		public void UnloadAudio(string audioName, AssetLabelReference audioLabel) => UnloadAsset(GetAddressablePath(audioName, audioLabel), audioHandles);
		public void UnloadDialogue(string addressablePath) => UnloadAsset(addressablePath, dialogueHandles);
		public void UnloadImage(string imageName) => UnloadAsset(GetAddressablePath(imageName, imageLabel), imageHandles);
		public void UnloadCharacterAtlas(string characterName) => UnloadAsset(GetAddressablePath(characterName, characterAtlasLabel), characterAtlasHandles);
		public void UnloadModel3DPrefab(string characterName) => UnloadAsset(GetAddressablePath(characterName, model3DPrefabLabel), model3DPrefabHandles);

		IEnumerator LoadAsset<T>(string name, Dictionary<string, string> keys, Dictionary<string, AsyncOperationHandle<T>> handles)
			where T : UnityEngine.Object
		{
			if (!keys.TryGetValue(name, out string addressableKey))
			{
				Debug.LogWarning($"Addressable asset not found: {name}");
				yield break;
			}

			// The asset is already loaded
			if (handles.ContainsKey(name)) yield break;

			AsyncOperationHandle<T> asyncHandle = Addressables.LoadAssetAsync<T>(addressableKey);
			yield return asyncHandle.WaitForCompletion();

			if (asyncHandle.Status != AsyncOperationStatus.Succeeded)
			{
				Debug.LogWarning($"Failed to load addressable asset: {name}");
				UnloadAsset(name, handles);
				yield break;
			}

			// Check once more if the asset is loaded
			if (handles.ContainsKey(name))
				Addressables.Release(asyncHandle);
			else
				handles.Add(name, asyncHandle);
		}

		T GetAsset<T>(string name, Dictionary<string, AsyncOperationHandle<T>> handles) where T : UnityEngine.Object
		{
			if (!handles.TryGetValue(name, out AsyncOperationHandle<T> handle)) return null;
			return handle.Result;
		}

		void UnloadAsset<T>(string name, Dictionary<string, AsyncOperationHandle<T>> handles) where T : UnityEngine.Object
		{
			if (!handles.TryGetValue(name, out AsyncOperationHandle<T> handle)) return;

			Addressables.Release(handles[name]);
			handles.Remove(name);
		}

		void CacheLabeledAssetsIntoDictionary(AssetType assetType, Dictionary<string, string> addressableKeys, params AssetLabelReference[] assetLabels)
		{
			int remainingLabels = assetLabels.Length;

			foreach (AssetLabelReference assetLabel in assetLabels)
			{
				if (assetLabel == null || string.IsNullOrEmpty(assetLabel.labelString))
				{
					if (--remainingLabels == 0)
						initializedKeys[assetType] = true;

					Debug.LogWarning("Invalid AssetLabelReference. Unable to load keys into dictionary.");
					continue;
				}

				// Get the addressable location metadata for all files with the same asset label
				AsyncOperationHandle<IList<IResourceLocation>> asyncHandle = Addressables.LoadResourceLocationsAsync(assetLabel.labelString);
				asyncHandle.Completed += (resultHandle) =>
				{
					if (resultHandle.Status == AsyncOperationStatus.Succeeded)
					{
						foreach (IResourceLocation location in resultHandle.Result)
						{
							if (string.IsNullOrEmpty(System.IO.Path.GetExtension(location.PrimaryKey))) continue;

							// Map the location of each file to its label + name
							string assetName = System.IO.Path.GetFileNameWithoutExtension(location.PrimaryKey);
							string assetKey = assetLabel.labelString + "/" + assetName;
							addressableKeys[assetKey] = location.PrimaryKey;
						}
					}
					else
					{
						Debug.LogWarning($"Unable to load keys for the given AssetLabelReference: {assetLabel.labelString}");
					}

					if (--remainingLabels == 0)
						initializedKeys[assetType] = true;

					Addressables.Release(resultHandle);
				};
			}
		}
	}
}
