using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
	[CreateAssetMenu(fileName = "Font Bank", menuName = "Scriptable Objects/Font Bank")]
	public class FontBankSO : ScriptableObject
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
