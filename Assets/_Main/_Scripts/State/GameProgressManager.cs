using History;
using IO;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
	public class GameProgressManager : MonoBehaviour
	{
		[SerializeField] SaveFileManagerSO saveFileManager;
		[SerializeField] GameManager gameManager;

		PlayerProgress progress;
		HashSet<string> readLines;

		bool hasPendingProgress = false;

		public string PlayerName => progress.playerName;
		public int SaveMenuPage => progress.saveMenuPage;

		public bool HasReadLine(string lineId) => readLines.Contains(lineId);

		void Awake()
		{
			LoadPlayerProgress();

			if (progress.readLines == null)
				readLines = new HashSet<string>();
			else
				readLines = new HashSet<string>(progress.readLines);
		}

		void OnApplicationQuit()
		{
			SavePlayerProgress();
		}

		public bool SavePlayerProgress()
		{
			if (!hasPendingProgress) return true;

			if (saveFileManager.SavePlayerProgress(progress))
			{
				hasPendingProgress = false;
				return true;
			}

			return false;
		}

		public void ResetReadLines()
		{
			readLines.Clear();
			progress.readLines.Clear();
		}

		public void AddReadLine(string lineId)
		{
			if (string.IsNullOrWhiteSpace(lineId) || readLines.Contains(lineId)) return;

			readLines.Add(lineId);
			progress.readLines.Add(lineId);
			hasPendingProgress = true;
		}

		public void SetPlayerName(string playerName)
		{
			if (progress.playerName == playerName) return;

			progress.playerName = playerName;
			hasPendingProgress = true;
		}

		public void SetSaveMenuPage(int pageNumber)
		{
			if (progress.saveMenuPage == pageNumber) return;

			progress.saveMenuPage = pageNumber;
			hasPendingProgress = true;
		}

		void LoadPlayerProgress()
		{
			if (saveFileManager.HasProgressSave())
			{
				progress = saveFileManager.LoadPlayerProgress();
				if (progress != null) return;
			}

			progress = new();
			saveFileManager.SavePlayerProgress(progress);
		}
	}
}
