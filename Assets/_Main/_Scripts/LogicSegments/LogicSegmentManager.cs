using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Dialogue
{
	public class LogicSegmentManager
	{
		DialogueSystem dialogueSystem;
		readonly Dictionary<string, Type> segmentTypes = new(StringComparer.OrdinalIgnoreCase);
		readonly Stack<LogicSegmentBase> segments = new();

		public bool HasPendingLogic => segments.Count > 0 && segments.Peek().IsComplete;

		public LogicSegmentManager(DialogueSystem dialogueSystem)
		{
			this.dialogueSystem = dialogueSystem;

			InitSegmentTypes();
		}

		public void Add(LogicSegmentBase logicSegment)
		{
			segments.Push(logicSegment);
		}

		public LogicSegmentBase GetLogicSegment(string rawLine)
		{
			int firstSpaceIndex = rawLine.IndexOf(' ');
			string keyword = rawLine;
			string rawData = "";

			if (firstSpaceIndex > -1)
			{
				keyword = rawLine.Substring(0, firstSpaceIndex);
				rawData = rawLine.Substring(firstSpaceIndex).Trim();
			}

			if (segmentTypes.ContainsKey(keyword))
				return CreateLogicSegment(keyword, rawData);

			return null;
		}

		public IEnumerator WaitForExecution()
		{
			if (!HasPendingLogic) yield break;

			LogicSegmentBase logicSegment = segments.Pop();
			yield return logicSegment.Execute();
		}

		LogicSegmentBase CreateLogicSegment(string keyword, string rawData)
		{
			object[] args = { dialogueSystem, rawData };
			LogicSegmentBase logicSegment = (LogicSegmentBase)Activator.CreateInstance(segmentTypes[keyword], args);

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
				PropertyInfo keywordProperty = type.GetProperty("Keyword", BindingFlags.Public | BindingFlags.Static);
				string keyword = keywordProperty?.GetValue(null) as string;

				if (string.IsNullOrWhiteSpace(keyword))
				{
					Debug.LogWarning($"Unable to access 'Keyword' property for {type.Name}. Class skipped.");
					continue;
				}
				if (segmentTypes.ContainsKey(keyword))
				{
					Debug.LogWarning($"Duplicate Keyword '{keyword}' found in {type.Name}. Class skipped.");
					continue;
				}

				segmentTypes.Add(keyword, type);
			}
		}
	}
}
