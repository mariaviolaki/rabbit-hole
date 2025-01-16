using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
	[SerializeField][Range(1, 12)] float speed;
	[SerializeField] TextBuilder.TextMode textMode;
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI dialogueText;

	TextBuilder textBuilder;

	string[] testStrings =
	{
		//"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
		"Lorem <b>ipsum</b> dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
		"Ut <b>enim</b> ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
		"Duis <b>aute</b> irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.",
		"Excepteur <b>sint</b> occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
	};

    void Awake()
    {
		textBuilder = new TextBuilder(dialogueText);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
		{
			string text = testStrings[Random.Range(0, testStrings.Length - 1)];
			textBuilder.Speed = speed;
			textBuilder.Write(text, textMode);
		}
		else if (Input.GetKeyDown(KeyCode.A))
		{
			string text = testStrings[Random.Range(0, testStrings.Length - 1)];
			textBuilder.Speed = speed;
			textBuilder.Append(text, textMode);
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			textBuilder.Stop();
		}
	}
}
