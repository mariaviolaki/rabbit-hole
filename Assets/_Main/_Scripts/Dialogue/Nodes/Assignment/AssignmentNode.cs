using System.Collections;
using System.Globalization;
using Variables;

namespace Dialogue
{
	public class AssignmentNode : NodeBase
	{
		string variableString;
		string assignmentOperatorString;
		string evaluatedValueString;

		public AssignmentNode(DialogueTreeNode treeNode, DialogueFlowController flowController) : base(treeNode, flowController)
		{
		}

		public override void StartExecution()
		{
			if (IsExecuting) return;
			base.StartExecution();

			if (treeNode.Data.Count != 3)
			{
				flowController.ProceedToNode(treeNode.NextId);
				return;
			}

			ParseTreeNode();
			flowController.Dialogue.StartCoroutine(ExecuteLogic());
		}

		protected override void ParseTreeNode()
		{
			variableString = treeNode.Data[0].Trim();
			assignmentOperatorString = treeNode.Data[1].Trim();
			string valueString = treeNode.Data[2].Trim();
			evaluatedValueString = ScriptLogicUtilities.EvaluateExpression(valueString, flowController.Game.Variables);
		}

		protected override IEnumerator ExecuteLogic()
		{
			yield return base.ExecuteLogic();

			AssignValue(variableString, assignmentOperatorString, evaluatedValueString, flowController.Game.Variables);
			flowController.ProceedToNode(treeNode.NextId);
			executionCoroutine = null;
			yield break;
		}

		void AssignValue(string variableString, string assignmentOperator, string valueString, VariableManager variableManager)
		{
			string newValue;
			string variableName = variableString.StartsWith(VariableManager.VariablePrefix)
				? variableString.Substring(1)
				: variableString;

			if (assignmentOperator == "=")
			{
				newValue = ScriptLogicUtilities.EvaluateStringValue(valueString, variableManager);
			}
			else
			{
				string calculationOperator = assignmentOperator.Substring(0, 1);
				newValue = ScriptLogicUtilities.CalculateValue(variableString, calculationOperator, valueString, variableManager);
			}

			if (bool.TryParse(newValue, out bool parsedBool))
				variableManager.Set(variableName, parsedBool);
			else if (int.TryParse(newValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
				variableManager.Set(variableName, parsedInt);
			else if (float.TryParse(newValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedFloat))
				variableManager.Set(variableName, parsedFloat);
			else
				variableManager.Set(variableName, ScriptLogicUtilities.RemoveStringQuotes(newValue));
		}
	}
}
