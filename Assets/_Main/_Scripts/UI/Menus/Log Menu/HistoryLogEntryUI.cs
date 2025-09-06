using History;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup), typeof(Button))]
	public class HistoryLogEntryUI : MonoBehaviour
	{
		[SerializeField] CanvasGroup canvasGroup;
		[SerializeField] TextMeshProUGUI nameText;
		[SerializeField] TextMeshProUGUI dialogueText;
		[SerializeField] Button logEntryButton;
		[SerializeField] Button rewindButton;
		[SerializeField] Button voiceButton;

		int logIndex;
		int historyIndex;
		LinkedListNode<HistoryState> historyNode;

		public event Action<int> OnSelect;
		public event Action<LinkedListNode<HistoryState>> OnRewindHistory;
		public event Action OnPlayVoice;

		public int HistoryIndex => historyIndex;
		public LinkedListNode<HistoryState> HistoryNode => historyNode;

		void OnEnable()
		{
			logEntryButton.onClick.AddListener(Select);
			rewindButton.onClick.AddListener(RewindHistory);
			voiceButton.onClick.AddListener(PlayVoice);
		}

		void OnDisable()
		{
			logEntryButton.onClick.RemoveListener(Select);
			rewindButton.onClick.RemoveListener(RewindHistory);
			voiceButton.onClick.RemoveListener(PlayVoice);
		}

		public void Initialize(int logIndex)
		{
			this.logIndex = logIndex;
		}

		public void SetHistoryState(LinkedListNode<HistoryState> historyNode, int historyIndex = -1)
		{
			this.historyIndex = historyNode == null ? -1 : historyIndex;
			this.historyNode = historyNode;
			
			if (historyNode == null)
			{
				canvasGroup.alpha = 0f;
				logEntryButton.interactable = false;
			}
			else
			{
				HistoryDialogueData historyDialogue = historyNode.Value.Dialogue;
				nameText.text = historyDialogue.SpeakerText;
				dialogueText.text = historyDialogue.DialogueText;
				canvasGroup.alpha = 1f;
				logEntryButton.interactable = true;
			}
		}

		void Select()
		{
			OnSelect?.Invoke(logIndex);
		}

		void RewindHistory()
		{
			OnRewindHistory?.Invoke(historyNode);
		}

		// TODO: Implement voice playback
		void PlayVoice()
		{
			OnPlayVoice?.Invoke();
		}
	}
}
