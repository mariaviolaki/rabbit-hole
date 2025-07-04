using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;

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
	[SerializeField] AssetLabelReference dialogueCharactersLabel;
	[SerializeField] AssetLabelReference dialogueEndingsLabel;
	[SerializeField] AssetLabelReference backgroundImageLabel;
	[SerializeField] AssetLabelReference characterAtlasLabel;
	[SerializeField] AssetLabelReference model3DPrefabLabel;
	[SerializeField] AssetLabelReference characterPrefabLabel;

	readonly Dictionary<string, string> videoPaths = new(StringComparer.OrdinalIgnoreCase);

	readonly Dictionary<string, string> audioKeys = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, string> dialogueKeys = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, string> backgroundImageKeys = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, string> characterAtlasKeys = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, string> model3DPrefabKeys = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, string> characterPrefabKeys = new(StringComparer.OrdinalIgnoreCase);

	readonly Dictionary<string, AsyncOperationHandle<AudioClip>> audioHandles = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, AsyncOperationHandle<TextAsset>> dialogueHandles = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, AsyncOperationHandle<Sprite>> backgroundImageHandles = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, AsyncOperationHandle<SpriteAtlas>> characterAtlasHandles = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, AsyncOperationHandle<GameObject>> model3DPrefabHandles = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, AsyncOperationHandle<GameObject>> characterPrefabHandles = new(StringComparer.OrdinalIgnoreCase);


	void OnEnable()
	{
		// Load the addressables's metadata to easily search and load them later
		CacheLabeledAssetsIntoDictionary(videoPaths, videoLabel);
		CacheLabeledAssetsIntoDictionary(audioKeys, ambientAudioLabel, musicAudioLabel, sfxAudioLabel, voiceAudioLabel);
		CacheLabeledAssetsIntoDictionary(dialogueKeys, dialogueLabel, dialogueCharactersLabel, dialogueEndingsLabel);
		CacheLabeledAssetsIntoDictionary(backgroundImageKeys, backgroundImageLabel);
		CacheLabeledAssetsIntoDictionary(characterAtlasKeys, characterAtlasLabel);
		CacheLabeledAssetsIntoDictionary(model3DPrefabKeys, model3DPrefabLabel);
		CacheLabeledAssetsIntoDictionary(characterPrefabKeys, characterPrefabLabel);
	}

	public string GetAddressablePath(string fileName, AssetLabelReference assetLabel) => assetLabel.labelString + "/" + fileName;

	// TODO test if this works on other platforms
	public string GetVideoUrl(string fileName) => videoPaths[GetAddressablePath(fileName, videoLabel)];

	public IEnumerator LoadAudio(string audioName, AssetLabelReference audioLabel) => LoadAsset(GetAddressablePath(audioName, audioLabel), audioKeys, audioHandles);
	public IEnumerator LoadDialogue(string addressablePath) => LoadAsset(addressablePath, dialogueKeys, dialogueHandles);
	public IEnumerator LoadBackgroundImage(string imageName) => LoadAsset(GetAddressablePath(imageName, backgroundImageLabel), backgroundImageKeys, backgroundImageHandles);
	public IEnumerator LoadCharacterAtlas(string characterName) => LoadAsset(GetAddressablePath(characterName, characterAtlasLabel), characterAtlasKeys, characterAtlasHandles);
	public IEnumerator LoadModel3DPrefab(string characterName) => LoadAsset(GetAddressablePath(characterName, model3DPrefabLabel), model3DPrefabKeys, model3DPrefabHandles);
	public IEnumerator LoadCharacterPrefab(string characterName) => LoadAsset(GetAddressablePath(characterName, characterPrefabLabel), characterPrefabKeys, characterPrefabHandles);

	public AudioClip GetAudio(string audioName, AssetLabelReference audioLabel) => GetAsset(GetAddressablePath(audioName, audioLabel), audioHandles);
	public TextAsset GetDialogueFile(string addressablePath) => GetAsset(addressablePath, dialogueHandles);
	public Sprite GetBackgroundImage(string imageName) => GetAsset(GetAddressablePath(imageName, backgroundImageLabel), backgroundImageHandles);
	public SpriteAtlas GetCharacterAtlas(string characterName) => GetAsset(GetAddressablePath(characterName, characterAtlasLabel), characterAtlasHandles);
	public GameObject GetModel3DPrefab(string characterName) => GetAsset(GetAddressablePath(characterName, model3DPrefabLabel), model3DPrefabHandles);
	public GameObject GetCharacterPrefab(string characterName) => GetAsset(GetAddressablePath(characterName, characterPrefabLabel), characterPrefabHandles);

	public void UnloadAudio(string audioName, AssetLabelReference audioLabel) => UnloadAsset(GetAddressablePath(audioName, audioLabel), audioHandles);
	public void UnloadDialogue(string addressablePath) => UnloadAsset(addressablePath, dialogueHandles);
	public void UnloadBackgroundImage(string imageName) => UnloadAsset(GetAddressablePath(imageName, backgroundImageLabel), backgroundImageHandles);
	public void UnloadCharacterAtlas(string characterName) => UnloadAsset(GetAddressablePath(characterName, characterAtlasLabel), characterAtlasHandles);
	public void UnloadModel3DPrefab(string characterName) => UnloadAsset(GetAddressablePath(characterName, model3DPrefabLabel), model3DPrefabHandles);
	public void UnloadCharacterPrefab(string characterName) => UnloadAsset(GetAddressablePath(characterName, characterPrefabLabel), characterPrefabHandles);

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

	void CacheLabeledAssetsIntoDictionary(Dictionary<string, string> addressableKeys, params AssetLabelReference[] assetLabels)
	{
		foreach (AssetLabelReference assetLabel in assetLabels)
		{
			if (assetLabel == null || string.IsNullOrEmpty(assetLabel.labelString))
			{
				Debug.LogWarning("Invalid AssetLabelReference. Unable to load keys into dictionary.");
				return;
			}

			// Get the addressable location metadata for all files with the same asset label
			AsyncOperationHandle<IList<IResourceLocation>> asyncHandle = Addressables.LoadResourceLocationsAsync(assetLabel.labelString);
			asyncHandle.Completed += (resultHandle) =>
			{
				if (resultHandle.Status == AsyncOperationStatus.Succeeded)
				{
					foreach (IResourceLocation location in resultHandle.Result)
					{
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

				Addressables.Release(resultHandle);
			};
		}
	}
}
