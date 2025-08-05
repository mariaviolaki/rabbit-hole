using IO;
using System.Collections;
using UI;
using Variables;

namespace Dialogue
{
	public class InputNode : NodeBase
	{
		const string InputVariableName = "input";

		readonly InputManagerSO inputManager;
		readonly ScriptVariableManager variableManager;
		readonly VisualNovelUI visualNovelUI;

		string title = "";
		string userInput = null;
		bool isWaitingToAdvance;

		public InputNode(DialogueTreeNode treeNode, DialogueFlowController flowController) : base(treeNode, flowController)
		{
			inputManager = flowController.Dialogue.InputManager;
			variableManager = flowController.Dialogue.VariableManager;
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
			title = treeNode.Data[0].Trim();

			if (title.StartsWith('"') && title.EndsWith('"'))
				title = title.Substring(1, title.Length - 2);
		}

		protected override IEnumerator ExecuteLogic()
		{
			flowController.InterruptSkipDueToBlockingNode();

			inputManager.OnClearInput += HandleOnClearInputEvent;
			inputManager.OnSubmitInput += HandleOnSubmitInputEvent;

			try
			{
				isWaitingToAdvance = true;
				yield return visualNovelUI.GameplayControls.ShowInput(title);
				while (userInput == null && isWaitingToAdvance) yield return null;

				if (!isWaitingToAdvance)
					yield return visualNovelUI.GameplayControls.ForceHideInput();

				if (userInput != null)
					variableManager.Set(InputVariableName, userInput);
			}
			finally
			{
				inputManager.OnClearInput -= HandleOnClearInputEvent;
				inputManager.OnSubmitInput -= HandleOnSubmitInputEvent;
				executionCoroutine = null;
				flowController.ProceedToNode(treeNode.NextId);
			}
		}

		public override void CancelExecution()
		{
			base.CancelExecution();
			isWaitingToAdvance = false;
		}

		void HandleOnClearInputEvent() => HandleInputEvent(null);
		void HandleOnSubmitInputEvent(string input) => HandleInputEvent(input);
		void HandleInputEvent(string input) => userInput = input;
	}
}
