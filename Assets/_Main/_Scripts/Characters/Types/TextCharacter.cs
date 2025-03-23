using System.Threading.Tasks;
using UnityEngine;

namespace Characters
{
	public class TextCharacter : Character
	{
		protected override Task Init()
		{
			Debug.Log($"Created Text Character: {Data.Name}");
			return Task.CompletedTask;
		}
	}
}
