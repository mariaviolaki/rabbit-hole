using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "FileManager", menuName = "Scriptable Objects/File Manager")]
public class FileManagerSO : ScriptableObject
{
	public Action<List<TextAsset>> OnLoadTextFiles;

	public void LoadTextFiles(AssetLabelReference assetLabel)
	{
		if (assetLabel == null) return;

		AsyncOperationHandle asyncHandle = Addressables.LoadAssetsAsync<TextAsset>(assetLabel, null);
		asyncHandle.Completed += (resultHandle) =>
		{
			if (resultHandle.Status == AsyncOperationStatus.Succeeded)
				OnLoadTextFiles?.Invoke((List<TextAsset>)resultHandle.Result);
			else
				Debug.LogError("FileManagerSO: Error while loading text assets");

			Addressables.Release(asyncHandle);
		};
	}
}
