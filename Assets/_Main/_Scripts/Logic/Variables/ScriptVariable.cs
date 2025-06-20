using System;

namespace Logic
{
	public abstract class ScriptVariable
	{
		public abstract object Get();
		public abstract void Set(object newValue);
	}

	public class ScriptVariable<T> : ScriptVariable
	{
		T value;
		readonly Func<T> getter;
		readonly Action<T> setter;

		public ScriptVariable(T defaultValue = default, Func<T> getter = null, Action<T> setter = null)
		{
			value = defaultValue;
			this.getter = getter ?? (() => this.value);
			this.setter = setter ?? ((newValue) => this.value = newValue);
		}

		public override object Get() => getter();
		public override void Set(object newValue) => setter((T)newValue);
	}
}
