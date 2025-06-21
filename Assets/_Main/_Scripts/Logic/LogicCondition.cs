namespace Logic
{
	public class LogicCondition
	{
		readonly string condition;
		readonly LogicBlock logicBlock;

		public string Condition => condition;
		public LogicBlock Block => logicBlock;

		public LogicCondition(string condition, LogicBlock logicBlock)
		{
			this.condition = condition;
			this.logicBlock = logicBlock;
		}
	}
}
