using UnityEngine;

namespace Variables
{
	[System.Serializable]
	public class DialogueTag
	{
		[SerializeField] string name;
		[SerializeField] string defaultValue;
		string currentValue;

		public string Name => name;
		public string DefaultValue => defaultValue;
		public string CurrentValue
		{
			get { return string.IsNullOrWhiteSpace(currentValue) ? defaultValue : currentValue; }
			set { currentValue = value; }
		}
	}
}
