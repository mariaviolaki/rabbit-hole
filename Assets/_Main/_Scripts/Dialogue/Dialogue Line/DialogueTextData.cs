using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueTextData
{
	public enum StartMode
	{
		None, InputClear, InputAppend, AutoClear, AutoAppend
	}

	public struct Segment
	{
		public string Text { get; private set; }
		public StartMode Mode { get; private set; }
		public float WaitTime { get; private set; }

		public bool IsAppended { get { return Mode == StartMode.InputAppend || Mode == StartMode.AutoAppend; } }
		public bool IsAuto { get { return Mode == StartMode.AutoClear || Mode == StartMode.AutoAppend; } }

		public Segment(string text, StartMode startMode, float waitTime = 0)
		{
			Text = text;
			Mode = startMode;
			WaitTime = waitTime;
		}
	}

	const string SegmentDelimiterPattern = @"\{[ac](\s\d+(\.\d+)?)?\}";

	public List<Segment> Segments { get; private set; }

	public DialogueTextData(string rawText)
	{
		ParseTextSegments(rawText);
	}

	void ParseTextSegments(string dialogueText)
	{
		Segments = new List<Segment>();

		Regex delimiterRegex = new Regex(SegmentDelimiterPattern);
		MatchCollection delimiterMatches = delimiterRegex.Matches(dialogueText);

		// Segment separators are not necessary in the text - dialogue text might be the only segment
		int firstSegmentEnd = delimiterMatches.Count > 0 ? delimiterMatches[0].Index : dialogueText.Length;
		Segment firstSegment = new Segment(dialogueText.Substring(0, firstSegmentEnd), StartMode.None);
		Segments.Add(firstSegment);

		// Split the dialogue into multiple text segments, each displayed separately
		if (delimiterMatches.Count > 0)
		{
			for (int i = 0; i < delimiterMatches.Count; i++)
			{
				Match match = delimiterMatches[i];
				Match nextMatch = (i + 1 == delimiterMatches.Count) ? null : delimiterMatches[i + 1];

				Segments.Add(GetSegmentBetweenMatches(dialogueText, match, nextMatch));
			}
		}
	}

	Segment GetSegmentBetweenMatches(string rawText, Match match, Match nextMatch)
	{
		// The text to be displayed
		int textStart = match.Index + match.Length;
		int textLength = (nextMatch == null) ? (rawText.Length - textStart) : (nextMatch.Index - textStart);
		string text = rawText.Substring(textStart, textLength);

		// How this text will be displayed
		string startModeText = match.Value.Substring(1, match.Length - 2);
		string[] startModeParams = startModeText.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
		StartMode startMode = GetStartModeFromText(startModeParams);

		// Optionally, wait for a few seconds before automatically showing the text
		float waitTime = 0f;
		if (startModeParams.Length > 1)
			float.TryParse(startModeParams[1], out waitTime);

		return new Segment(text, startMode, waitTime);
	}

	StartMode GetStartModeFromText(string[] startModeParams)
	{
		string startModeKeyword = startModeParams[0];

		if (startModeParams.Length == 1)
		{
			// No wait time was defined - so we wait for player input
			if (startModeKeyword == "c") return StartMode.InputClear;
			else if (startModeKeyword == "a") return StartMode.InputAppend;
		}
		else
		{
			// We wait for a set amount of time before showing the next text segment
			if (startModeKeyword == "c") return StartMode.AutoClear;
			else if (startModeKeyword == "a") return StartMode.AutoAppend;
		}

		return StartMode.None;
	}
}
