using Dialogue;
using System.Collections;
using UI;

namespace Logic
{
	public class InputLogicSegment : LogicSegmentBase
	{
		const string keyword = "input";
		public static new bool Matches(string rawLine) => StartsWithKeyword(rawLine, keyword);

		readonly InputManagerSO inputManager;
		readonly VisualNovelUI visualNovelUI;
		string title = "";
		string input = null;

		public InputLogicSegment(DialogueSystem dialogueSystem, string rawLine) : base(dialogueSystem, rawLine)
		{
			inputManager = dialogueSystem.InputManager;
			visualNovelUI = dialogueSystem.UI;

			ParseTitle();
		}

		public override IEnumerator Execute()
		{
			inputManager.OnClearInput += HandleOnClearInputEvent;
			inputManager.OnSubmitInput += HandleOnSubmitInputEvent;

			try
			{
				yield return visualNovelUI.ShowInput(title);
				while (input == null) yield return null;
			}
			finally
			{
				inputManager.OnClearInput -= HandleOnClearInputEvent;
				inputManager.OnSubmitInput -= HandleOnSubmitInputEvent;
			}
		}

		void HandleOnClearInputEvent() => HandleInputEvent(null);
		void HandleOnSubmitInputEvent(string input) => HandleInputEvent(input);
		void HandleInputEvent(string input) => this.input = input;

		void ParseTitle()
		{
			// Get the data that follows after the input keyword
			string rawData = rawLine.Substring(keyword.Length).TrimStart();

			if (rawData.StartsWith('"') && rawData.EndsWith('"'))
			{
				title = rawData.Substring(1, rawData.Length - 2);
			}
		}
	}
}
