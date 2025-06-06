using GameIO;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "File Manager", menuName = "Scriptable Objects/File Manager")]
public class FileManagerSO : ScriptableObject
{
	static readonly string StreamingAssetsVideoPath = Path.Combine(Application.streamingAssetsPath, "Videos");

	[SerializeField] GameOptionsSO gameOptions;
	[SerializeField] AssetLabelReference characterPrefabLabel;
	[SerializeField] AssetLabelReference model3DPrefabLabel;
	[SerializeField] AssetLabelReference characterAtlasLabel;
	[SerializeField] AssetLabelReference dialogueFileLabel;
	[SerializeField] AssetLabelReference backgroundImageLabel;
	[SerializeField] AssetLabelReference ambientAudioLabel;
	[SerializeField] AssetLabelReference musicAudioLabel;
	[SerializeField] AssetLabelReference sfxAudioLabel;
	[SerializeField] AssetLabelReference voiceAudioLabel;

	readonly Dictionary<string, string> characterPrefabKeys = new();
	readonly Dictionary<string, string> model3DPrefabKeys = new();
	readonly Dictionary<string, string> characterAtlasKeys = new();
	readonly Dictionary<string, string> dialogueFileKeys = new();
	readonly Dictionary<string, string> backgroundImageKeys = new();
	readonly Dictionary<string, string> ambientAudioKeys = new();
	readonly Dictionary<string, string> musicAudioKeys = new();
	readonly Dictionary<string, string> sfxAudioKeys = new();
	readonly Dictionary<string, string> voiceAudioKeys = new();

	readonly Dictionary<string, AsyncOperationHandle<GameObject>> characterPrefabHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<GameObject>> model3DPrefabHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<SpriteAtlas>> characterAtlasHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<TextAsset>> dialogueFileHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<Sprite>> backgroundImageHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<AudioClip>> ambientAudioHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<AudioClip>> musicAudioHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<AudioClip>> sfxAudioHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<AudioClip>> voiceAudioHandles = new();

	void OnEnable()
	{
		// Load the addressables's metadata to easily search and load them later
		CacheLabeledAssetsIntoDictionary(characterPrefabLabel, characterPrefabKeys);
		CacheLabeledAssetsIntoDictionary(model3DPrefabLabel, model3DPrefabKeys);
		CacheLabeledAssetsIntoDictionary(characterAtlasLabel, characterAtlasKeys);
		CacheLabeledAssetsIntoDictionary(dialogueFileLabel, dialogueFileKeys);
		CacheLabeledAssetsIntoDictionary(backgroundImageLabel, backgroundImageKeys);
		CacheLabeledAssetsIntoDictionary(ambientAudioLabel, ambientAudioKeys);
		CacheLabeledAssetsIntoDictionary(musicAudioLabel, musicAudioKeys);
		CacheLabeledAssetsIntoDictionary(sfxAudioLabel, sfxAudioKeys);
		CacheLabeledAssetsIntoDictionary(voiceAudioLabel, voiceAudioKeys);
	}

	public string GetVideoUrl(string fileName) =>
		Path.Combine(StreamingAssetsVideoPath, GetFileNameWithExtension(fileName, gameOptions.IO.VideoExtension));

	public IEnumerator LoadCharacterPrefab(string characterName) => LoadAsset(characterName, characterPrefabKeys, characterPrefabHandles);
	public IEnumerator LoadModel3DPrefab(string characterName) => LoadAsset(characterName, model3DPrefabKeys, model3DPrefabHandles);
	public IEnumerator LoadCharacterAtlas(string characterName) => LoadAsset(characterName, characterAtlasKeys, characterAtlasHandles);
	public IEnumerator LoadDialogueFile(string fileName) => LoadAsset(fileName, dialogueFileKeys, dialogueFileHandles);
	public IEnumerator LoadBackgroundImage(string imageName) => LoadAsset(imageName, backgroundImageKeys, backgroundImageHandles);
	public IEnumerator LoadAmbientAudio(string audioName) => LoadAsset(audioName, ambientAudioKeys, ambientAudioHandles);
	public IEnumerator LoadMusicAudio(string audioName) => LoadAsset(audioName, musicAudioKeys, musicAudioHandles);
	public IEnumerator LoadSFXAudio(string audioName) => LoadAsset(audioName, sfxAudioKeys, sfxAudioHandles);
	public IEnumerator LoadVoiceAudio(string audioName) => LoadAsset(audioName, voiceAudioKeys, voiceAudioHandles);

	public GameObject GetCharacterPrefab(string characterName) => GetAsset(characterName, characterPrefabHandles);
	public GameObject GetModel3DPrefab(string characterName) => GetAsset(characterName, model3DPrefabHandles);
	public SpriteAtlas GetCharacterAtlas(string characterName) => GetAsset(characterName, characterAtlasHandles);
	public TextAsset GetDialogueFile(string fileName) => GetAsset(fileName, dialogueFileHandles);
	public Sprite GetBackgroundImage(string imageName) => GetAsset(imageName, backgroundImageHandles);
	public AudioClip GetAmbientAudio(string audioName) => GetAsset(audioName, ambientAudioHandles);
	public AudioClip GetMusicAudio(string audioName) => GetAsset(audioName, musicAudioHandles);
	public AudioClip GetSFXAudio(string audioName) => GetAsset(audioName, sfxAudioHandles);
	public AudioClip GetVoiceAudio(string audioName) => GetAsset(audioName, voiceAudioHandles);

	public void UnloadCharacterPrefab(string characterName) => UnloadAsset(characterName, characterPrefabHandles);
	public void UnloadModel3DPrefab(string characterName) => UnloadAsset(characterName, model3DPrefabHandles);
	public void UnloadCharacterAtlas(string characterName) => UnloadAsset(characterName, characterAtlasHandles);
	public void UnloadDialogueFile(string fileName) => UnloadAsset(fileName, dialogueFileHandles);
	public void UnloadBackgroundImage(string imageName) => UnloadAsset(imageName, backgroundImageHandles);
	public void UnloadAmbientAudio(string audioName) => UnloadAsset(audioName, ambientAudioHandles);
	public void UnloadMusicAudio(string audioName) => UnloadAsset(audioName, musicAudioHandles);
	public void UnloadSFXAudio(string audioName) => UnloadAsset(audioName, sfxAudioHandles);
	public void UnloadVoiceAudio(string audioName) => UnloadAsset(audioName, voiceAudioHandles);

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

	string GetFileNameWithExtension(string fileName, FileExtension defaultExtension)
	{
		int extensionIndex = fileName.LastIndexOf('.');
		string extensionString = extensionIndex == -1 ? null : fileName.Substring(extensionIndex);
		if (extensionString != null) return fileName;

		if (defaultExtension == FileExtension.None)
			Debug.LogWarning($"No default extension specified for '{fileName}'.");

		return fileName + "." + defaultExtension.ToString();
	}

	void CacheLabeledAssetsIntoDictionary(AssetLabelReference assetLabel, Dictionary<string, string> addressableKeys)
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
					// Map the location of each file to its name
					string assetName = System.IO.Path.GetFileNameWithoutExtension(location.PrimaryKey);
					addressableKeys[assetName] = location.PrimaryKey;
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
