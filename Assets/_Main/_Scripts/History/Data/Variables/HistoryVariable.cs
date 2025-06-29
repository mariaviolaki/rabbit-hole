using Variables;

namespace History
{
	[System.Serializable]
	public class HistoryVariable
	{
		public ScriptVariableType type;
		public DataTypeEnum dataType;
		public string name;
		public string value;
	}
}
