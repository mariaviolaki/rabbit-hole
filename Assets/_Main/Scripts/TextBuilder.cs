using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class TextBuilder
{
	const int minSpeed = 1;
	const int maxSpeed = 10; // instant

	Coroutine buildProcess;
	TMP_Text tmpText;
	int speed;

	public int Speed { get { return speed; } set { speed = Mathf.Clamp(value, minSpeed, maxSpeed); } }

	public TextBuilder(TMP_Text tmpText, int speed)
	{
		this.tmpText = tmpText;
		this.speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
	}

	public bool BuildNew(string newText)
	{
		bool isBuilding = buildProcess != null;
		if (isBuilding)
		{
			CompleteProcess();
		}

		// If the player interacts before the text is completed, complete it immediately
		string text = isBuilding ? tmpText.text : newText;
		int textSpeed = isBuilding ? maxSpeed : speed;

		buildProcess = tmpText.StartCoroutine(Build(text, textSpeed));

		// Return whether the new text provided was used or not
		return !isBuilding;
	}

	IEnumerator Build(string text, int textSpeed)
	{
		int textCount = text.Count();

		tmpText.text = text;
		tmpText.maxVisibleCharacters = 0;
		tmpText.ForceMeshUpdate();

		while (tmpText.maxVisibleCharacters < textCount)
		{
			// Skip some frames for speed 1-5
			int iterations = textSpeed;
			while (iterations < maxSpeed)
			{
				iterations += textSpeed;
				yield return null;
			}

			// After some frames are skipped or when speed is 6-10
			// Immediately reveal 1-5 characters or the entire string
			tmpText.maxVisibleCharacters += GetCharactersPerIteration(textCount, textSpeed);
			tmpText.ForceMeshUpdate();
			yield return null;
		}

		CompleteProcess();
	}

	void CompleteProcess()
	{
		tmpText.StopCoroutine(buildProcess);
		buildProcess = null;
	}

	int GetCharactersPerIteration(int textCount, int textSpeed)
	{
		// If speed is 10, all characters are revealed instantly
		if (textSpeed == maxSpeed) return textCount;
		// For speed 1-5, 1 character is revealed
		else if (textSpeed < 6) return 1;
		// For speed 6-9, 2-5 characters are revealed
		else return textSpeed - 4;
	}
}
