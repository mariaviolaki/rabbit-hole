using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentContainerSpawner : MonoBehaviour
{
	[Tooltip("The root container of any object which should persist across scenes")]
	[SerializeField] GameObject persistentContainerPrefab;

	static bool hasSpawned;

	void Awake()
	{
		if (!hasSpawned)
		{
			GameObject persistentContainer = Instantiate(persistentContainerPrefab);
			DontDestroyOnLoad(persistentContainer);
			hasSpawned = true;
		}

		Destroy(gameObject);
	}
}
