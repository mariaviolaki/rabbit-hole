using System.Collections;

namespace Dialogue
{
	public class ConditionNode : NodeBase
	{
		DialogueTreeNode branchNode;

		public ConditionNode(DialogueTreeNode treeNode, DialogueFlowController flowController) : base(treeNode, flowController)
		{
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
			foreach (DialogueTreeNode branchNode in treeNode.Children)
			{
				if (branchNode.Type != DialogueNodeType.ConditionBranch) continue;

				string conditionType = branchNode.Data[0].Trim();

				if (conditionType == "if" || conditionType == "else if")
				{
					if (branchNode.Data.Count != 2 || string.IsNullOrWhiteSpace(branchNode.Data[1])) continue;

					// Check if the condition is successful in an if block
					string condition = branchNode.Data[1].Trim();
					string resultString = ScriptLogicUtilities.EvaluateExpression(condition, flowController.Game.Variables);
					if (!bool.TryParse(resultString, out bool resultBool) || !resultBool) continue;
				}
				else if (conditionType != "else") continue;

				this.branchNode = branchNode;
				break;
			}
		}

		protected override IEnumerator ExecuteLogic()
		{
			yield return base.ExecuteLogic();

			// Proceed to the first node nested inside the branch
			if (branchNode != null && branchNode.Children.Count > 0)
				flowController.ProceedToNode(branchNode.Id + 1);
			else // Proceed right after the condition node because the correct branch was empty
				flowController.ProceedToNode(treeNode.NextId);

			executionCoroutine = null;
			yield break;
		}
	}
}
