namespace Dialogue
{
	public class DialogueTextSegment
	{
		public string Text { get; private set; }
		public SegmentStartMode StartMode { get; private set; }
		public float WaitTime { get; private set; }

		public bool IsAppended { get { return StartMode == SegmentStartMode.InputAppend || StartMode == SegmentStartMode.AutoAppend; } }
		public bool IsAuto { get { return StartMode == SegmentStartMode.AutoClear || StartMode == SegmentStartMode.AutoAppend; } }

		public DialogueTextSegment(string text, SegmentStartMode startMode, float waitTime = 0)
		{
			Text = text;
			StartMode = startMode;
			WaitTime = waitTime;
		}
	}
}
