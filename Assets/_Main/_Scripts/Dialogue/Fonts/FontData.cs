using TMPro;
using UnityEngine;

namespace Dialogue
{
	[System.Serializable]
	public class FontData
	{
		[SerializeField] TMP_FontAsset fontAsset;
		[SerializeField] float sizeMultiplier;

		public TMP_FontAsset FontAsset { get { return fontAsset; } }
		public string Name { get { return fontAsset.name; } }
		public float SizeMultiplier { get { return sizeMultiplier; } }
	}
}