using System.Collections;

namespace Dialogue
{
	public class JumpNode : NodeBase
	{
		string sectionName;

		public string SectionName => sectionName;

		public JumpNode(DialogueTreeNode treeNode, DialogueFlowController flowController) : base(treeNode, flowController)
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
			sectionName = treeNode.Data[0].Trim();
		}

		protected override IEnumerator ExecuteLogic()
		{
			yield return base.ExecuteLogic();

			executionCoroutine = null;
			yield return flowController.JumpToSection(sectionName);
		}
	}
}
