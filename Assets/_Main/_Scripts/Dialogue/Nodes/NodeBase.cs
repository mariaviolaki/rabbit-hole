using Dialogue;
using System.Collections;
using UnityEngine;

public abstract class NodeBase
{
	protected DialogueTreeNode treeNode;
	protected DialogueFlowController flowController;
	protected Coroutine executionCoroutine;
	bool isExecuting;

	public DialogueTreeNode TreeNode => treeNode;
	public bool IsExecuting => isExecuting;

	abstract protected void ParseTreeNode();
	abstract protected IEnumerator ExecuteLogic();

	virtual public void SpeedUpExecution() { }
	virtual public void CancelExecution() { }

	public NodeBase(DialogueTreeNode treeNode, DialogueFlowController flowController)
	{
		this.treeNode = treeNode;
		this.flowController = flowController;
		isExecuting = false;
	}

	virtual public void StartExecution()
	{
		isExecuting = true;
	}
}
