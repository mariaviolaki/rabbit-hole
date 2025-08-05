using System;

namespace Variables
{
	public class ScriptVariable
	{
		object value;
		readonly DataTypeEnum dataType;
		readonly Func<object> getter;
		readonly Action<object> setter;

		public DataTypeEnum GetDataType() => dataType;
		public object Get() => getter();
		public void Set(object newValue) => setter(newValue);

		public ScriptVariable(object defaultValue, Func<object> getter = null, Action<object> setter = null)
		{
			value = defaultValue;
			this.dataType = Utilities.GetDataTypeEnum(value);
			this.getter = getter ?? (() => this.value);
			this.setter = setter ?? ((newValue) => this.value = newValue);
		}		
	}
}
