using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

[CreateAssetMenu(fileName = "File Manager", menuName = "Scriptable Objects/File Manager")]
public class FileManagerSO : ScriptableObject
{
	[SerializeField] AssetLabelReference characterPrefabLabel;
	[SerializeField] AssetLabelReference dialogueFileLabel;

	readonly Dictionary<string, string> characterPrefabKeys = new Dictionary<string, string>();
	readonly Dictionary<string, string> dialogueFileKeys = new Dictionary<string, string>();

	void OnEnable()
	{
		CacheLabeledAssetsIntoDictionary(characterPrefabLabel, characterPrefabKeys);
		CacheLabeledAssetsIntoDictionary(dialogueFileLabel, dialogueFileKeys);
	}

	public async Task<GameObject> LoadCharacterPrefab(string characterName)
	{
		return await LoadAsset<GameObject>(characterName, characterPrefabKeys);
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

		// Get the addressable loction metadata for all files with the same asset label
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
