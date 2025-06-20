using System;
using UnityEngine;

namespace Dialogue
{
	[System.Serializable]
	public class DialogueTag
	{
		[SerializeField] string name;
		[SerializeField] string defaultValue;
		Func<string> currentValue;

		public string Name => name;
		public string DefaultValue => defaultValue;
		public Func<string> CurrentValue
		{
			get { return currentValue == null ? () => defaultValue : currentValue; }
			set { currentValue = value; }
		}
	}
}
