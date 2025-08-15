using System;
using System.Collections;
using UnityEngine;

namespace Commands
{
	public class CommandProcessBase
	{
		protected bool isBlocking = false;
		protected bool isCompleted = true;

		public Guid Id { get; set; }
		virtual public bool IsBlocking { get { return isBlocking; } set { isBlocking = isBlocking ? isBlocking : value; } }
		virtual public bool IsCompleted => isCompleted;

		virtual public void Run() { }
		virtual public void Skip() { }
	}

	public class TransitionCommandProcess : CommandProcessBase
	{
		readonly CommandManager commandManager;
		readonly Func<IEnumerator> runProcessFunc;
		readonly Action runActionFunc;
		readonly Action skipFunc;
		readonly Func<bool> isCompletedFunc;

		Coroutine coroutine;

		override public bool IsCompleted => coroutine == null && isCompletedFunc();

		public TransitionCommandProcess(CommandManager commandManager, Func<IEnumerator> runProcessFunc, Action skipFunc, Func<bool> isCompletedFunc)
		{
			this.commandManager = commandManager;
			this.runProcessFunc = runProcessFunc;
			this.skipFunc = skipFunc;
			this.isCompletedFunc = isCompletedFunc;
		}

		public TransitionCommandProcess(Action runActionFunc, Action skipFunc, Func<bool> isCompletedFunc)
		{
			this.runActionFunc = runActionFunc;
			this.skipFunc = skipFunc;
			this.isCompletedFunc = isCompletedFunc;
		}

		override public void Run()
		{
			if (runActionFunc != null)
			{
				runActionFunc();
			}
			else if (commandManager != null && runProcessFunc != null && coroutine == null)
			{
				coroutine = commandManager.StartCoroutine(RunProcess());
			}
		}

		override public void Skip()
		{
			if (isBlocking || coroutine != null) return;

			skipFunc();
		}

		IEnumerator RunProcess()
		{
			try
			{
				yield return runProcessFunc();
			}
			finally
			{
				coroutine = null;
			}
		}
	}

	public class CoroutineCommandProcess : CommandProcessBase
	{
		readonly CommandManager commandManager;
		readonly Func<IEnumerator> processFunc;

		Coroutine coroutine;

		override public bool IsCompleted => coroutine == null;

		public CoroutineCommandProcess(CommandManager commandManager, Func<IEnumerator> processFunc, bool isBlocking)
		{
			this.commandManager = commandManager;
			this.processFunc = processFunc;
			this.isBlocking = isBlocking;
		}

		override public void Run()
		{
			if (commandManager == null || processFunc == null || coroutine != null) return;

			coroutine = commandManager.StartCoroutine(RunProcess());
		}

		override public void Skip()
		{
			if (isBlocking || coroutine == null) return;

			commandManager.StopCoroutine(coroutine);
			coroutine = null;
		}

		IEnumerator RunProcess()
		{
			try
			{
				yield return processFunc();
			}
			finally
			{
				coroutine = null;
			}
		}
	}

	public class ActionCommandProcess : CommandProcessBase
	{
		readonly Action actionFunc;

		public ActionCommandProcess(Action actionFunc)
		{
			this.actionFunc = actionFunc;
		}

		override public void Run() => actionFunc();
	}
}
