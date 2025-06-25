using System.Collections.Generic;
using UnityEngine;

namespace Variables
{
	[CreateAssetMenu(fileName = "Dialogue Tag Bank", menuName = "Scriptable Objects/Dialogue Tag Bank")]
	public class DialogueTagBankSO : ScriptableObject
	{
		[SerializeField] DialogueTag[] dialogueTags;

		readonly Dictionary<string, DialogueTag> tags = new();

		public Dictionary<string, DialogueTag> Tags { get { return tags; } }

		void OnEnable()
		{
			// Convert the array to a dictionary for fast lookups
			foreach (DialogueTag tag in dialogueTags)
			{
				tags.Add(tag.Name, tag);
			}
		}
	}
}
