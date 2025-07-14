using Dialogue;
using History;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class LogPanelUI : MonoBehaviour
	{
		const float MoveAnimationSpeedMultiplier = 0.0001f;
		const float ScrollTransitionMultiplier = 0.008f;
		const float MinScrollbarSize = 0.05f;

		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] RectTransform scrollRectTransform;
		[SerializeField] CanvasGroup canvasGroup;
		[SerializeField] Button backButton;
		[SerializeField] HistoryLogEntryUI logEntryPrefab;
		[SerializeField] VerticalLayoutGroup layoutGroup;
		[SerializeField] RectTransform viewportRectTransform;
		[SerializeField] Scrollbar scrollbar;
		[SerializeField] float closedPositionOffset;
		[SerializeField] HistoryManager historyManager;
		[SerializeField] DialogueSystem dialogueSystem;

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

		Coroutine transitionCoroutine;
		Coroutine scrollCoroutine;

		public event Action OnClose;

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

		public void ShowDefault() => Show();
		public Coroutine Show(bool isImmediate = false, float transitionSpeed = 0)
		{
			if (transitionCoroutine != null || IsVisible) return null;

			transitionSpeed = (transitionSpeed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : transitionSpeed;

			transitionCoroutine = StartCoroutine(OpenProcess(isImmediate, transitionSpeed));
			return transitionCoroutine;
		}

		public void HideDefault() => Hide();
		public Coroutine Hide(bool isImmediate = false, float transitionSpeed = 0)
		{
			if (transitionCoroutine != null || IsHidden) return null;

			transitionSpeed = (transitionSpeed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : transitionSpeed;

			transitionCoroutine = StartCoroutine(CloseProcess(isImmediate, transitionSpeed));
			return transitionCoroutine;
		}

		public IEnumerator OpenProcess(bool isImmediate = false, float speed = 0)
		{
			if (!PrepareOpen())
			{
				transitionCoroutine = null;
				OnClose?.Invoke();
				yield break;
			}

			if (isImmediate)
			{
				SetVisibleImmediate();
			}
			else
			{
				speed = (speed < Mathf.Epsilon) ? gameOptions.Dialogue.FadeTransitionSpeed : speed;
				Vector2 closedPosition = new(openPosition.x, openPosition.y + closedPositionOffset);

				List<IEnumerator> transitionProcesses = new()
				{
					transitionHandler.SetVisibility(canvasGroup, true, speed),
					MoveToPosition(closedPosition, openPosition, speed)
				};

				yield return Utilities.RunConcurrentProcesses(this, transitionProcesses);
			}

			transitionCoroutine = null;
		}

		public IEnumerator CloseProcess(bool isImmediate = false, float speed = 0)
		{
			if (isImmediate)
			{
				SetHiddenImmediate();
			}
			else
			{
				speed = (speed < Mathf.Epsilon) ? gameOptions.Dialogue.FadeTransitionSpeed : speed;
				Vector2 closedPosition = new (openPosition.x, openPosition.y + closedPositionOffset);

				List<IEnumerator> transitionProcesses = new()
				{
					transitionHandler.SetVisibility(canvasGroup, false, speed),
					MoveToPosition(openPosition, closedPosition, speed)
				};

				yield return Utilities.RunConcurrentProcesses(this, transitionProcesses);
			}

			CompleteClose();
			transitionCoroutine = null;
			OnClose?.Invoke();
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
				scrollRectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothProgress);

				yield return null;
			}

			scrollRectTransform.anchoredPosition = endPos;
		}

		bool PrepareOpen()
		{
			if (!GetHistoryData()) return false;

			ClearHistoryLogs();
			PrepareHistoryLogs();
			PrepareScrollbar();
			SubscribeListeners();
			inputManager.IsLogPanelOpen = true;
			return true;
		}

		void CompleteClose()
		{
			UnsubscribeListeners();
			inputManager.IsLogPanelOpen = false;
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
			// Don't overlap scrolling
			if (scrollCoroutine != null) return;

			// Don't scroll to an invalid index
			if (index < 0 || index >= logEntryCount || index == currentIndex || logEntries[index].HistoryNode == null) return;

			bool isScrollDown = index < currentIndex;

			// Don't scroll past the newest/oldest entries
			if ((isScrollDown && currentNode.Next == null) || (!isScrollDown && currentNode.Previous == null)) return;

			// Don't scroll to more recent logs if currently viewing history
			if (isScrollDown && historyManager.HistoryNode != null && historyManager.HistoryNode == currentNode.Next) return;

			// Don't show newer logs if they are marked as invalid by the history manager
			if (isScrollDown && (logEntries[index].HistoryIndex == -1 || logEntries[index].HistoryIndex < lastHistoryIndex)) return;

			scrollCoroutine = StartCoroutine(MoveToEntry(index));
		}

		void RewindHistory(LinkedListNode<HistoryState> historyNode)
		{
			if (historyNode == null || transitionCoroutine != null || scrollCoroutine != null) return;

			transitionCoroutine = StartCoroutine(RewindHistoryProcess(historyNode, false, gameOptions.General.SkipTransitionSpeed));
		}

		IEnumerator RewindHistoryProcess(LinkedListNode<HistoryState> historyNode, bool isImmediate = false, float speed = 0f)
		{
			historyManager.SetLogPanelRewindNode(historyNode);

			yield return CloseProcess(isImmediate, speed);
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

			scrollCoroutine = null;
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

			lastHistoryIndex = historyManager.GetLastNodeIndex();
			if (lastHistoryIndex < 0) return false;

			// Don't show any logs if there is no history
			currentNode = historyManager.GetLastNode();
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
			openPosition = scrollRectTransform.anchoredPosition;

			// Initialize the containers that will be used to show history
			logEntryCount = layoutGroup.transform.childCount;
			logEntries = new HistoryLogEntryUI[logEntryCount];
			logEntryPositions = new float[logEntryCount];

			contentRectTransform = layoutGroup.GetComponent<RectTransform>();

			LayoutElement entryLayoutElement = layoutGroup.transform.GetChild(0).GetComponent<LayoutElement>();
			logEntryHeight = entryLayoutElement.preferredHeight;
			visibleLogsCount = Mathf.CeilToInt(viewportRectTransform.rect.height / (logEntryHeight + layoutGroup.spacing));

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

			HistoryLogEntryUI logEntry = layoutGroup.transform.GetChild(childIndex).GetComponent<HistoryLogEntryUI>();
			logEntry.Initialize(logEntryIndex);

			logEntries[logEntryIndex] = logEntry;
			logEntryPositions[logEntryIndex] = offsetFromCenter * (logEntryHeight + layoutGroup.spacing);
		}

		void SubscribeListeners()
		{
			inputManager.OnScroll += Scroll;
			backButton.onClick.AddListener(HideDefault);

			foreach (HistoryLogEntryUI logEntry in logEntries)
			{
				logEntry.OnSelect += (index) => Scroll(index);
				logEntry.OnRewindHistory += (historyNode) => RewindHistory(historyNode);
			}
		}

		void UnsubscribeListeners()
		{
			inputManager.OnScroll -= Scroll;
			backButton.onClick.RemoveListener(HideDefault);

			foreach (HistoryLogEntryUI logEntry in logEntries)
			{
				logEntry.OnSelect -= (index) => Scroll(index);
				logEntry.OnRewindHistory -= (historyNode) => RewindHistory(historyNode);
			}
		}
	}
}
