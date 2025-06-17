using System.Collections;
using UI;

namespace Dialogue
{
	public class InputLogicSegment : LogicSegmentBase
	{
		public static new string Keyword => "Input";

		readonly InputManagerSO inputManager;
		readonly VisualNovelUI visualNovelUI;
		readonly DialogueStack dialogueStack;
		string title = "";
		string input = null;

		public InputLogicSegment(DialogueSystem dialogueSystem, string rawData) : base(dialogueSystem, rawData)
		{
			inputManager = dialogueSystem.InputManager;
			visualNovelUI = dialogueSystem.UI;
			dialogueStack = dialogueSystem.Reader.Stack;

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
				dialogueStack.Proceed();
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
			if (rawData.StartsWith('"') && rawData.EndsWith('"'))
			{
				title = rawData.Substring(1, rawData.Length - 2);
			}
		}
	}
}
