namespace Variables
{
	public class ScriptVariable
	{
		object value;
		readonly DataTypeEnum dataType;

		public DataTypeEnum GetDataType() => dataType;
		public object Get() => value;
		public void Set(object newValue) => value = newValue;

		public ScriptVariable(object defaultValue)
		{
			value = defaultValue;
			dataType = Utilities.GetDataTypeEnum(value);
		}		
	}
}
