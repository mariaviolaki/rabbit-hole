using Dialogue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Logic
{
	public class LogicSegmentManager
	{
		readonly DialogueSystem dialogueSystem;
		readonly InputManagerSO inputManager;
		readonly Dictionary<Type, Func<string, bool>> segmentMatches = new();
		readonly Stack<LogicSegmentBase> segments = new();

		public bool HasPendingLogic => segments.Count > 0;
		bool isExecuting;

		void PauseExecution() => isExecuting = false;

		public LogicSegmentManager(DialogueSystem dialogueSystem)
		{
			this.dialogueSystem = dialogueSystem;
			this.inputManager = dialogueSystem.InputManager;

			InitSegmentTypes();

			inputManager.OnBack += PauseExecution;
		}

		public void Dispose()
		{
			inputManager.OnBack -= PauseExecution;
		}

		public void Add(LogicSegmentBase logicSegment)
		{
			segments.Push(logicSegment);
		}

		public LogicSegmentBase GetLogicSegment(string rawLine)
		{
			foreach ((Type segmentType, Func<string, bool> matchesMethod) in segmentMatches)
			{
				if (matchesMethod(rawLine))
					return CreateLogicSegment(segmentType, rawLine.Trim());
			}

			return null;
		}

		public IEnumerator WaitForExecution()
		{
			if (!HasPendingLogic) yield break;

			isExecuting = true;
			LogicSegmentBase logicSegment = segments.Pop();

			if (logicSegment is BlockingLogicSegmentBase blockingLogicSegment)
			{
				IEnumerator logic = blockingLogicSegment.Execute();

				while (isExecuting && logic.MoveNext()) yield return null;

				if (!isExecuting)
					yield return blockingLogicSegment.ForceComplete();
			}	
			else if (logicSegment is NonBlockingLogicSegmentBase nonBlockingLogicSegment)
			{
				nonBlockingLogicSegment.Execute();
				yield break;
			}
		}

		LogicSegmentBase CreateLogicSegment(Type segmentType, string rawData)
		{
			object[] args = { dialogueSystem, rawData };
			LogicSegmentBase logicSegment = (LogicSegmentBase)Activator.CreateInstance(segmentType, args);

			return logicSegment;
		}

		void InitSegmentTypes()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Type[] logicSegmentTypes = assembly.GetTypes()
				.Where(t => typeof(LogicSegmentBase).IsAssignableFrom(t) && !t.IsAbstract)
				.ToArray();

			foreach (Type type in logicSegmentTypes)
			{
				// Map each type of logic class to a function that returns if a raw line handles this type of logic
				MethodInfo matchesMethod = type.GetMethod("Matches", BindingFlags.Public | BindingFlags.Static);
				if (matchesMethod == null)
				{
					Debug.LogWarning($"Unable to access 'Matches' function for {type.Name}. Class skipped.");
					continue;
				}

				segmentMatches.Add(type, (Func<string, bool>)Delegate.CreateDelegate(typeof(Func<string, bool>), matchesMethod));
			}
		}
	}
}
