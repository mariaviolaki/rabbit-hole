using Dialogue;
using History;
using IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class LogMenuUI : MonoBehaviour
	{
		const float MoveAnimationSpeedMultiplier = 0.00002f;
		const float ScrollTransitionMultiplier = 0.008f;
		const float MinScrollbarSize = 0.05f;

		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] CanvasGroup canvasGroup;
		[SerializeField] Button backButton;
		[SerializeField] HistoryLogEntryUI logEntryPrefab;
		[SerializeField] VerticalLayoutGroup contentLayoutGroup;
		[SerializeField] RectTransform viewportRectTransform;
		[SerializeField] Scrollbar scrollbar;
		[SerializeField] float closedPositionOffset;
		[SerializeField] MenusUI menus;
		[SerializeField] HistoryManager historyManager;
		[SerializeField] DialogueManager dialogueManager;

		RectTransform contentRectTransform;
		UITransitionHandler transitionHandler;

		LinkedListNode<HistoryState> currentNode;
		HistoryLogEntryUI[] logEntries;
		float[] logEntryPositions;
		int historyStateCount;
		int lastHistoryIndex;
		int logEntryCount;
		float logEntryHeight;
		int currentIndex;
		int visibleLogsCount;
		Vector2 openPosition;

		bool isTransitioning = false;

		public event Action OnClose;

		public bool IsTransitioning => isTransitioning;
		public bool IsVisible => canvasGroup.alpha == 1f;
		public bool IsHidden => canvasGroup.alpha == 0f;

		int GetCenterIndex() => Mathf.RoundToInt(logEntryCount / 2);

		void Awake()
		{
			transitionHandler = new UITransitionHandler(gameOptions);
			InitializeLogs();

			SetHiddenImmediate();
		}

		public void SetVisibleImmediate() => canvasGroup.alpha = 1f;
		public void SetHiddenImmediate() => canvasGroup.alpha = 0f;

		public IEnumerator Open(bool isImmediate = false, float transitionSpeed = 0)
		{
			if (IsVisible || isTransitioning) yield break;
			isTransitioning = true;

			if (!PrepareOpen())
			{
				isTransitioning = false;
				OnClose?.Invoke();
				yield break;
			}

			transitionSpeed = (transitionSpeed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : transitionSpeed;
			yield return OpenProcess(isImmediate, transitionSpeed);

			isTransitioning = false;
		}

		public IEnumerator Close(bool isImmediate = false, float transitionSpeed = 0)
		{
			if (IsHidden || isTransitioning) yield break;
			isTransitioning = true;

			transitionSpeed = (transitionSpeed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : transitionSpeed;
			yield return CloseProcess(isImmediate, transitionSpeed);

			isTransitioning = false;
			OnClose?.Invoke();
		}

		IEnumerator OpenProcess(bool isImmediate = false, float speed = 0)
		{
			if (isImmediate)
			{
				SetVisibleImmediate();
			}
			else
			{
				speed = (speed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : speed;
				Vector2 closedPosition = new(openPosition.x, openPosition.y + closedPositionOffset);

				List<IEnumerator> transitionProcesses = new()
				{
					transitionHandler.SetVisibility(canvasGroup, true, speed),
					MoveToPosition(closedPosition, openPosition, speed)
				};

				yield return Utilities.RunConcurrentProcesses(this, transitionProcesses);
			}
		}

		IEnumerator CloseProcess(bool isImmediate = false, float speed = 0)
		{
			if (isImmediate)
			{
				SetHiddenImmediate();
			}
			else
			{
				speed = (speed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : speed;
				Vector2 closedPosition = new (openPosition.x, openPosition.y + closedPositionOffset);

				List<IEnumerator> transitionProcesses = new()
				{
					transitionHandler.SetVisibility(canvasGroup, false, speed),
					MoveToPosition(openPosition, closedPosition, speed)
				};

				yield return Utilities.RunConcurrentProcesses(this, transitionProcesses);
			}

			CompleteClose();			
		}

		IEnumerator MoveToPosition(Vector2 startPos, Vector2 endPos, float speed)
		{
			float distance = (endPos - startPos).sqrMagnitude;
			float duration = (1 / speed) * MoveAnimationSpeedMultiplier * distance;

			if (duration <= 0) yield break;

			float progress = 0f;
			while (progress < duration)
			{
				progress += Time.deltaTime;

				float smoothProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / duration));
				viewportRectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothProgress);

				yield return null;
			}

			viewportRectTransform.anchoredPosition = endPos;
		}

		bool PrepareOpen()
		{
			if (!GetHistoryData()) return false;

			ClearHistoryLogs();
			PrepareHistoryLogs();
			PrepareScrollbar();
			SubscribeListeners();

			return true;
		}

		void CompleteClose()
		{
			UnsubscribeListeners();
		}

		void Scroll(float offset)
		{
			if (offset > 0f)
				Scroll(currentIndex + 1);
			else if (offset < 0f)
				Scroll(currentIndex - 1);
		}

		void Scroll(int index)
		{
			// Don't overlap transitions
			if (isTransitioning) return;
			isTransitioning = true;

			// Don't scroll to an invalid index
			if (index < 0 || index >= logEntryCount || index == currentIndex || logEntries[index].HistoryNode == null)
			{
				isTransitioning = false;
				return;
			}

			bool isScrollDown = index < currentIndex;

			// Don't scroll past the newest/oldest entries
			if ((isScrollDown && currentNode.Next == null) || (!isScrollDown && currentNode.Previous == null))
			{
				isTransitioning = false;
				return;
			}

			// Don't scroll to more recent logs if currently viewing history
			if (isScrollDown && historyManager.HistoryNode != null && historyManager.HistoryNode == currentNode.Next)
			{
				isTransitioning = false;
				return;
			}

			// Don't show newer logs if they are marked as invalid by the history manager
			if (isScrollDown && (logEntries[index].HistoryIndex == -1 || logEntries[index].HistoryIndex < lastHistoryIndex))
			{
				isTransitioning = false;
				return;
			}

			StartCoroutine(MoveToEntry(index));
		}

		void RewindHistory(LinkedListNode<HistoryState> historyNode)
		{
			if (historyNode == null || isTransitioning) return;
			isTransitioning = true;

			StartCoroutine(RewindHistoryProcess(historyNode, false, gameOptions.General.SkipTransitionSpeed));
		}

		IEnumerator RewindHistoryProcess(LinkedListNode<HistoryState> historyNode, bool isImmediate = false, float speed = 0f)
		{
			yield return historyManager.Load(historyNode);
			yield return CloseProcess(isImmediate, speed);

			isTransitioning = false;
			OnClose?.Invoke();
		}

		IEnumerator MoveToEntry(int entryIndex)
		{
			int oldLogIndex = currentIndex;
			float startPosY = logEntryPositions[oldLogIndex];
			float endPosY = logEntryPositions[entryIndex];

			float distance = Mathf.Abs(startPosY - endPosY);
			float progress = 0f;
			float duration = (1f / gameOptions.General.TransitionSpeed) * ScrollTransitionMultiplier * distance;

			SetNewHistoryNode(entryIndex);

			while (progress < duration)
			{
				progress += Time.deltaTime;

				float smoothProgress = Mathf.SmoothStep(0, 1, Mathf.Clamp01(progress / duration));
				float newPosY = Mathf.Lerp(startPosY, endPosY, smoothProgress);
				contentRectTransform.localPosition = new Vector2(contentRectTransform.localPosition.x, newPosY);

				yield return null;
			}
			contentRectTransform.localPosition = new Vector2(contentRectTransform.localPosition.x, endPosY);

			ClearOldHistoryNode(oldLogIndex);
			RecenterLogEntries(oldLogIndex);

			float currentHistoryNum = logEntries[currentIndex].HistoryIndex;
			scrollbar.SetValueWithoutNotify(currentHistoryNum / (historyStateCount - 1));

			isTransitioning = false;
		}

		void SetNewHistoryNode(int newLogIndex)
		{
			if (newLogIndex < currentIndex)
			{
				currentNode = currentNode.Next;

				// Don't show newer logs when navigating back to history
				bool isBeyondHistory = historyManager.HistoryNode != null && currentNode.Next == historyManager.HistoryNode;
				// Don't show newer logs if they are marked as invalid by the history manager
				bool isValidNewHistory = logEntries[newLogIndex].HistoryIndex - 1 >= lastHistoryIndex;

				// Show the next, more recent log after the new one
				if (currentNode.Next != null && !isBeyondHistory && isValidNewHistory)
				{
					HistoryLogEntryUI newerLog = logEntries[newLogIndex - 1];
					newerLog.SetHistoryState(currentNode.Next, logEntries[newLogIndex].HistoryIndex - 1);
				}
			}
			else
			{
				currentNode = currentNode.Previous;

				// Show the next, older log before the new one
				if (currentNode.Previous != null)
				{
					HistoryLogEntryUI olderLog = logEntries[newLogIndex + 1];
					olderLog.SetHistoryState(currentNode.Previous, logEntries[newLogIndex].HistoryIndex + 1);
				}
			}

			currentIndex = newLogIndex;
		}

		void ClearOldHistoryNode(int oldLogIndex)
		{
			int clearLogIndex = currentIndex < oldLogIndex ? oldLogIndex + 1 : oldLogIndex - 1;
			if (clearLogIndex < 0 || clearLogIndex >= logEntries.Length) return;

			logEntries[clearLogIndex].SetHistoryState(null);
		}

		void RecenterLogEntries(int oldLogIndex)
		{
			if (currentIndex < oldLogIndex)
			{
				for (int i = logEntryCount - 1; i > 0; i--)
				{
					logEntries[i].SetHistoryState(logEntries[i - 1].HistoryNode, logEntries[i - 1].HistoryIndex);
				}

				logEntries[0].SetHistoryState(null);
				currentIndex++;
			}
			else
			{
				for (int i = 0; i < logEntries.Length - 1; i++)
				{
					logEntries[i].SetHistoryState(logEntries[i + 1].HistoryNode, logEntries[i + 1].HistoryIndex);
				}

				logEntries[logEntries.Length - 1].SetHistoryState(null);
				currentIndex--;
			}

			float centerPosY = logEntryPositions[currentIndex];
			contentRectTransform.localPosition = new Vector2(contentRectTransform.localPosition.x, centerPosY);
		}

		bool GetHistoryData()
		{
			if (historyManager.IsUpdatingHistory) return false;

			// Cache the state of history to limit scrolling and available entries
			historyStateCount = historyManager.GetHistoryStateCount();
			if (historyStateCount == 0) return false;

			// Don't show any logs if there is no history
			currentNode = historyManager.HistoryNode?.Previous;
			if (currentNode == null) return false;

			return true;
		}

		void ClearHistoryLogs()
		{
			currentIndex = -1;

			foreach (HistoryLogEntryUI logEntry in logEntries)
			{
				logEntry.SetHistoryState(null);
			}
		}

		void PrepareHistoryLogs()
		{
			// Start the logs from the most recent entry
			currentIndex = GetCenterIndex();
			logEntries[currentIndex].SetHistoryState(currentNode, lastHistoryIndex);

			LinkedListNode<HistoryState> visibleNode = currentNode;
			int visibleLogsFromCenter = Mathf.CeilToInt((visibleLogsCount - 1) / 2);

			// Show all the history states that are visible above the first one
			for (int i = 1; i <= visibleLogsFromCenter; i++)
			{
				visibleNode = visibleNode.Previous;
				if (visibleNode == null) break;

				logEntries[currentIndex + i].SetHistoryState(visibleNode, lastHistoryIndex + i);
			}

			// Scroll down to the most recent entry
			contentRectTransform.localPosition = new Vector2(contentRectTransform.localPosition.x, logEntryPositions[currentIndex]);
		}

		void PrepareScrollbar()
		{
			scrollbar.interactable = false;

			if (historyStateCount <= 1)
				scrollbar.size = 1f;
			else
				scrollbar.size = Mathf.Max(1.0f / historyStateCount, MinScrollbarSize);

			scrollbar.SetValueWithoutNotify(0f);
		}

		void InitializeLogs()
		{
			// Viewport needs to be initialized by Unity to get its actual height
			Canvas.ForceUpdateCanvases();

			// Initialize the start position of the log panel
			openPosition = viewportRectTransform.anchoredPosition;

			// Initialize the containers that will be used to show history
			logEntryCount = contentLayoutGroup.transform.childCount;
			logEntries = new HistoryLogEntryUI[logEntryCount];
			logEntryPositions = new float[logEntryCount];

			contentRectTransform = contentLayoutGroup.GetComponent<RectTransform>();

			LayoutElement entryLayoutElement = contentLayoutGroup.transform.GetChild(0).GetComponent<LayoutElement>();
			logEntryHeight = entryLayoutElement.preferredHeight;
			visibleLogsCount = Mathf.CeilToInt(viewportRectTransform.rect.height / (logEntryHeight + contentLayoutGroup.spacing));

			// Initialize the fixed count of children that will be used for scrolling
			int centerIndex = GetCenterIndex();
			InitializeLogEntry(centerIndex, 0);

			for (int i = centerIndex - 1, offset = -1; i >= 0; i--, offset--)
			{
				InitializeLogEntry(i, offset);
			}

			for (int i = centerIndex + 1, offset = 1; i < logEntryCount; i++, offset++)
			{
				InitializeLogEntry(i, offset);
			}
		}

		void InitializeLogEntry(int childIndex, float offsetFromCenter)
		{
			int logEntryIndex = logEntryCount - childIndex - 1;

			HistoryLogEntryUI logEntry = contentLayoutGroup.transform.GetChild(childIndex).GetComponent<HistoryLogEntryUI>();
			logEntry.Initialize(logEntryIndex);

			logEntries[logEntryIndex] = logEntry;
			logEntryPositions[logEntryIndex] = offsetFromCenter * (logEntryHeight + contentLayoutGroup.spacing);
		}

		void SubscribeListeners()
		{
			inputManager.OnScroll += Scroll;
			backButton.onClick.AddListener(menus.CloseMenu);

			foreach (HistoryLogEntryUI logEntry in logEntries)
			{
				logEntry.OnSelect += Scroll;
				logEntry.OnRewindHistory += RewindHistory;
			}
		}

		void UnsubscribeListeners()
		{
			inputManager.OnScroll -= Scroll;
			backButton.onClick.RemoveListener(menus.CloseMenu);

			foreach (HistoryLogEntryUI logEntry in logEntries)
			{
				logEntry.OnSelect -= Scroll;
				logEntry.OnRewindHistory -= RewindHistory;
			}
		}
	}
}
