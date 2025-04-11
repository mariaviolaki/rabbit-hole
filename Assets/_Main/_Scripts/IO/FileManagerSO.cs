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

	readonly Dictionary<string, string> characterPrefabKeys = new Dictionary<string, string>();
	readonly Dictionary<string, string> model3DPrefabKeys = new Dictionary<string, string>();
	readonly Dictionary<string, string> characterAtlasKeys = new Dictionary<string, string>();
	readonly Dictionary<string, string> dialogueFileKeys = new Dictionary<string, string>();

	void OnEnable()
	{
		// Load the addressables's metadata to easily search and load them later
		CacheLabeledAssetsIntoDictionary(characterPrefabLabel, characterPrefabKeys);
		CacheLabeledAssetsIntoDictionary(model3DPrefabLabel, model3DPrefabKeys);
		CacheLabeledAssetsIntoDictionary(characterAtlasLabel, characterAtlasKeys);
		CacheLabeledAssetsIntoDictionary(dialogueFileLabel, dialogueFileKeys);
	}

	public async Task<GameObject> LoadCharacterPrefab(string characterName)
	{
		return await LoadAsset<GameObject>(characterName, characterPrefabKeys);
	}

	public async Task<GameObject> LoadModel3DPrefab(string characterName)
	{
		return await LoadAsset<GameObject>(characterName, model3DPrefabKeys);
	}

	public async Task<SpriteAtlas> LoadCharacterAtlas(string characterName)
	{
		return await LoadAsset<SpriteAtlas>(characterName, characterAtlasKeys);
	}

	public async Task<TextAsset> LoadDialogueFile(string fileName)
	{
		return await LoadAsset<TextAsset>(fileName, dialogueFileKeys);
	}

	async Task<T> LoadAsset<T>(string assetName, Dictionary<string, string> addressableKeys) where T : UnityEngine.Object
	{
		if (!addressableKeys.ContainsKey(assetName)) return null;

		string addressableKey = addressableKeys[assetName];
		AsyncOperationHandle<T> asyncHandle = Addressables.LoadAssetAsync<T>(addressableKey);

		await asyncHandle.Task;

		if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
			return asyncHandle.Result;
		else
			Debug.LogError($"Failed to load addressable asset: {assetName}");

		Addressables.Release(asyncHandle);
		return asyncHandle.Result;
	}

	void CacheLabeledAssetsIntoDictionary(AssetLabelReference assetLabel, Dictionary<string, string> addressableKeys)
	{
		if (assetLabel == null || string.IsNullOrEmpty(assetLabel.labelString))
		{
			Debug.LogError("Invalid AssetLabelReference. Unable to load keys into dictionary.");
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
				Debug.LogError($"Unable to load keys for the given AssetLabelReference: {assetLabel.labelString}");
			}

			Addressables.Release(resultHandle);
		};
	}
}
