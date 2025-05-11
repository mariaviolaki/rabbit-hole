using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "File Manager", menuName = "Scriptable Objects/File Manager")]
public class FileManagerSO : ScriptableObject
{
	[SerializeField] AssetLabelReference characterPrefabLabel;
	[SerializeField] AssetLabelReference model3DPrefabLabel;
	[SerializeField] AssetLabelReference characterAtlasLabel;
	[SerializeField] AssetLabelReference dialogueFileLabel;
	[SerializeField] AssetLabelReference backgroundImageLabel;

	readonly Dictionary<string, string> characterPrefabKeys = new();
	readonly Dictionary<string, string> model3DPrefabKeys = new();
	readonly Dictionary<string, string> characterAtlasKeys = new();
	readonly Dictionary<string, string> dialogueFileKeys = new();
	readonly Dictionary<string, string> backgroundImageKeys = new();

	readonly Dictionary<string, AsyncOperationHandle<GameObject>> characterPrefabHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<GameObject>> model3DPrefabHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<SpriteAtlas>> characterAtlasHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<TextAsset>> dialogueFileHandles = new();
	readonly Dictionary<string, AsyncOperationHandle<Sprite>> backgroundImageHandles = new();

	void OnEnable()
	{
		// Load the addressables's metadata to easily search and load them later
		CacheLabeledAssetsIntoDictionary(characterPrefabLabel, characterPrefabKeys);
		CacheLabeledAssetsIntoDictionary(model3DPrefabLabel, model3DPrefabKeys);
		CacheLabeledAssetsIntoDictionary(characterAtlasLabel, characterAtlasKeys);
		CacheLabeledAssetsIntoDictionary(dialogueFileLabel, dialogueFileKeys);
		CacheLabeledAssetsIntoDictionary(backgroundImageLabel, backgroundImageKeys);
	}

	public async Task<GameObject> LoadCharacterPrefab(string characterName)
	{
		return await LoadAsset(characterName, characterPrefabKeys, characterPrefabHandles);
	}

	public async Task<GameObject> LoadModel3DPrefab(string characterName)
	{
		return await LoadAsset(characterName, model3DPrefabKeys, model3DPrefabHandles);
	}

	public async Task<SpriteAtlas> LoadCharacterAtlas(string characterName)
	{
		return await LoadAsset(characterName, characterAtlasKeys, characterAtlasHandles);
	}

	public async Task<TextAsset> LoadDialogueFile(string fileName)
	{
		return await LoadAsset(fileName, dialogueFileKeys, dialogueFileHandles);
	}

	public async Task<Sprite> LoadBackgroundImage(string imageName)
	{
		return await LoadAsset(imageName, backgroundImageKeys, backgroundImageHandles);
	}

	public void UnloadCharacterPrefab(string characterName)
	{
		UnloadAsset(characterName, characterPrefabHandles);
	}

	public void UnloadModel3DPrefab(string characterName)
	{
		UnloadAsset(characterName, model3DPrefabHandles);
	}

	public void UnloadCharacterAtlas(string characterName)
	{
		UnloadAsset(characterName, characterAtlasHandles);
	}

	public void UnloadDialogueFile(string fileName)
	{
		UnloadAsset(fileName, dialogueFileHandles);
	}

	public void UnloadBackgroundImage(string imageName)
	{
		UnloadAsset(imageName, backgroundImageHandles);
	}

	async Task<T> LoadAsset<T>(string name, Dictionary<string, string> keys, Dictionary<string, AsyncOperationHandle<T>> handles)
		where T : UnityEngine.Object
	{
		if (!keys.ContainsKey(name))
		{
			Debug.LogWarning($"Addressable asset not found: {name}");
			return null;
		}

		if (handles.ContainsKey(name))
			return handles[name].Result;

		string addressableKey = keys[name];
		AsyncOperationHandle<T> asyncHandle = Addressables.LoadAssetAsync<T>(addressableKey);

		await asyncHandle.Task;
		handles.Add(name, asyncHandle);

		if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
			return asyncHandle.Result;

		Debug.LogWarning($"Failed to load addressable asset: {name}");
		UnloadAsset(name, handles);

		return null;
	}

	void UnloadAsset<T>(string name, Dictionary<string, AsyncOperationHandle<T>> handles) where T : UnityEngine.Object
	{
		if (!handles.ContainsKey(name)) return;

		Addressables.Release(handles[name]);
		handles.Remove(name);
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
