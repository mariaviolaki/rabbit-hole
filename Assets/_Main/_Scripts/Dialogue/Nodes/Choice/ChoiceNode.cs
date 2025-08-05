using IO;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Dialogue
{
	public class ChoiceNode : NodeBase
	{
		readonly InputManagerSO inputManager;
		readonly VisualNovelUI visualNovelUI;

		readonly List<DialogueChoice> choices = new();
		DialogueChoice selectedChoice;
		bool isWaitingToAdvance = true;

		public ChoiceNode(DialogueTreeNode treeNode, DialogueFlowController flowController) : base(treeNode, flowController)
		{
			inputManager = flowController.Dialogue.InputManager;
			visualNovelUI = flowController.Dialogue.UI;
		}

		public override void StartExecution()
		{
			if (IsExecuting) return;
			base.StartExecution();
			ParseTreeNode();
			executionCoroutine = flowController.Dialogue.StartCoroutine(ExecuteLogic());
		}

		protected override void ParseTreeNode()
		{
			for (int i = 0; i < treeNode.Children.Count; i++)
			{
				DialogueTreeNode branchNode = treeNode.Children[i];
				if (branchNode.Type != DialogueNodeType.ChoiceBranch) continue;

				string choiceText = branchNode.Data[0].Trim();
				choices.Add(new DialogueChoice(i, choiceText));
			}
		}

		protected override IEnumerator ExecuteLogic()
		{
			if (choices.Count == 0)
			{
				executionCoroutine = null;
				flowController.ProceedToNode(treeNode.NextId);
				yield break;
			}

			flowController.InterruptSkipDueToBlockingNode();

			inputManager.OnClearChoice += HandleOnClearChoiceEvent;
			inputManager.OnSelectChoice += HandleOnSelectChoiceEvent;

			try
			{
				isWaitingToAdvance = true;
				yield return visualNovelUI.GameplayControls.ShowChoices(choices);
				while (selectedChoice == null && isWaitingToAdvance) yield return null;

				if (!isWaitingToAdvance)
					yield return visualNovelUI.GameplayControls.ForceHideChoices();
			}
			finally
			{
				inputManager.OnClearChoice -= HandleOnClearChoiceEvent;
				inputManager.OnSelectChoice -= HandleOnSelectChoiceEvent;
				executionCoroutine = null;
				ProceedAfterChoice();
			}
		}

		public override void CancelExecution()
		{
			base.CancelExecution();
			isWaitingToAdvance = false;
		}

		void HandleOnClearChoiceEvent() => HandleChoiceEvent(null);
		void HandleOnSelectChoiceEvent(DialogueChoice choice) => HandleChoiceEvent(choice);
		void HandleChoiceEvent(DialogueChoice choice) => selectedChoice = choice;

		void ProceedAfterChoice()
		{
			if (selectedChoice != null)
			{
				DialogueTreeNode choiceBranch = treeNode.Children[selectedChoice.Index];

				// Proceed to the first node nested inside the branch
				if (choiceBranch.Children.Count > 0)
					flowController.ProceedToNode(choiceBranch.Id + 1);
				else // Proceed right after the choice node because the selected branch was empty
					flowController.ProceedToNode(treeNode.NextId);
			}
			else
			{
				// No choice was selected, proceed to the next node
				flowController.ProceedToNode(treeNode.NextId);
			}
		}
	}
}
