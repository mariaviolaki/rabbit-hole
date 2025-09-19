using Gameplay;
using History;
using IO;
using System.Collections.Generic;
using UnityEngine;
using Visuals;

namespace Game
{
	public class GameProgressManager : MonoBehaviour
	{
		[SerializeField] SaveFileManagerSO saveFileManager;
		[SerializeField] GameManager gameManager;

		PlayerProgress progress;
		HashSet<string> readLines;
		HashSet<UnlockedCG> unlockedCGs;

		bool hasPendingProgress = false;

		public string PlayerName => progress.playerName;
		public int SaveMenuPage => progress.saveMenuPage;

		public bool HasReadLine(string lineId) => readLines.Contains(lineId);
		public bool HasCG(CharacterRoute route, int num) => unlockedCGs.Contains(new UnlockedCG(route, num));

		void Awake()
		{
			LoadPlayerProgress();

			readLines = progress.readLines == null ? new HashSet<string>() : new HashSet<string>(progress.readLines);
			unlockedCGs = progress.unlockedCGs == null ? new HashSet<UnlockedCG>() : new HashSet<UnlockedCG>(progress.unlockedCGs);
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

		public void UnlockCG(UnlockedCG cg)
		{
			if (cg == null || unlockedCGs.Contains(cg)) return;

			unlockedCGs.Add(cg);
			progress.unlockedCGs.Add(cg);
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
