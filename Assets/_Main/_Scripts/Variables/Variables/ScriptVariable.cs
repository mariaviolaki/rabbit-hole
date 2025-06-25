using System;

namespace Variables
{
	public class ScriptVariable
	{
		object value;
		readonly ScriptVariableDataType dataType;
		readonly Func<object> getter;
		readonly Action<object> setter;

		public ScriptVariableDataType GetDataType() => dataType;
		public object Get() => getter();
		public void Set(object newValue) => setter(newValue);

		public ScriptVariable(object defaultValue, Func<object> getter = null, Action<object> setter = null)
		{
			value = defaultValue;
			this.dataType = GetScriptDataType();
			this.getter = getter ?? (() => this.value);
			this.setter = setter ?? ((newValue) => this.value = newValue);
		}

		ScriptVariableDataType GetScriptDataType()
		{
			return value switch
			{
				bool => ScriptVariableDataType.Bool,
				int => ScriptVariableDataType.Int,
				float => ScriptVariableDataType.Float,
				_ => ScriptVariableDataType.String
			};
		}
	}
}
