using System.Threading.Tasks;

namespace Characters
{
	public class TextCharacter : Character
	{
		protected override Task Init()
		{
			return Task.CompletedTask;
		}
	}
}
