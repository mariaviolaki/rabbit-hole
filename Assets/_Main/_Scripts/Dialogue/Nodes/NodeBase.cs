using Commands;
using System.Collections;
using UnityEngine;

namespace Dialogue
{
	public class NodeBase
	{
		CommandManager commandManager;
		protected DialogueTreeNode treeNode;
		protected DialogueFlowController flowController;
		protected Coroutine executionCoroutine;
		protected bool isWaitingToAdvance;
		bool isExecutionCanceled;
		bool isExecuting;

		protected bool IsExecutionCanceled => isExecutionCanceled;
		public DialogueTreeNode TreeNode => treeNode;
		public bool IsExecuting => isExecuting;

		virtual protected void ParseTreeNode() { }

		virtual public void SpeedUpExecution()
		{
			isWaitingToAdvance = false;
		}

		virtual public void CancelExecution()
		{
			isWaitingToAdvance = false;
			isExecutionCanceled = true;
		}

		public NodeBase(DialogueTreeNode treeNode, DialogueFlowController flowController)
		{
			commandManager = flowController.Dialogue.Commands;
			this.treeNode = treeNode;
			this.flowController = flowController;
			isWaitingToAdvance = false;
			isExecutionCanceled = false;
			isExecuting = false;
		}

		virtual public void StartExecution()
		{
			isExecuting = true;
		}

		virtual protected IEnumerator ExecuteLogic()
		{
			isWaitingToAdvance = true;

			while (!commandManager.IsIdle())
			{
				if (!isWaitingToAdvance)
				{
					commandManager.SkipCommands();
					isWaitingToAdvance = true;
				}
				yield return null;
			}

			isWaitingToAdvance = false;
		}
	}
}
