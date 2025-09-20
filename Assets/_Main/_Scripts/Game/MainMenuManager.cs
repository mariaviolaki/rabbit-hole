using IO;
using System.Collections;
using UnityEngine;

namespace Game
{
	public class MainMenuManager : MonoBehaviour
	{
		[SerializeField] AssetManagerSO assetManager;
		[SerializeField] string musicTitle;

		GameManager gameManager;
	
		void Start()
		{
			gameManager = FindAnyObjectByType<GameManager>();

			StartCoroutine(PlayMusic());
		}

		IEnumerator PlayMusic()
		{
			while (!assetManager.InitializedKeys.TryGetValue(AssetType.Audio, out bool value) || !value) yield return null;

			yield return gameManager.Audio.Play(Audio.AudioType.Music, musicTitle, isLooping: true);
		}
	}
}
