using TMPro;
using UI;
using UnityEngine;

namespace Dialogue
{
	public class ReadModeIndicatorUI : FadeableUI
	{
		[SerializeField] TextMeshProUGUI autoReadText;

		public Coroutine Show(DialogueReadMode readMode, float fadeSpeed = 0f)
		{
			autoReadText.text = readMode.ToString();
			return base.Show(fadeSpeed);
		}

		public void ShowInstant(DialogueReadMode readMode)
		{
			autoReadText.text = readMode.ToString();
			base.ShowInstant();
		}
	}
}
