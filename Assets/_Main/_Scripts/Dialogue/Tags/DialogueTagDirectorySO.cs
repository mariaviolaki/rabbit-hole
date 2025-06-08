using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
	[CreateAssetMenu(fileName = "Dialogue Tag Directory", menuName = "Scriptable Objects/Dialogue Tag Directory")]
	public class DialogueTagDirectorySO : ScriptableObject
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
