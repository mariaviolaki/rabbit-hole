using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
	[CreateAssetMenu(fileName = "Font Directory", menuName = "Scriptable Objects/Font Directory")]
	public class FontDirectorySO : ScriptableObject
	{
		[SerializeField] FontData[] fontData;
		Dictionary<string, FontData> fonts = new();

		public Dictionary<string, FontData> Fonts { get { return fonts; } }

		void OnEnable()
		{
			// Convert the array to a dictionary for fast lookups
			foreach (FontData font in fontData)
			{
				fonts.Add(font.Name, font);
			}
		}
	}
}
