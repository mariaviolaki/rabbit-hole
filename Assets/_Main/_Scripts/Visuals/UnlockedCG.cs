using Gameplay;
using System;
using UnityEngine;

namespace Visuals
{
	[System.Serializable]
	public class UnlockedCG
	{
		[SerializeField] CharacterRoute route;
		[SerializeField] int num; // CG number (1-based)

		public CharacterRoute Route => route;
		public int Num => num;

		public UnlockedCG(CharacterRoute route, int num)
		{
			this.route = route;
			this.num = num;
		}

		public override int GetHashCode() => HashCode.Combine(route, num);
		public override bool Equals(object other)
		{
			if (other is not UnlockedCG otherCG) return false;
			return otherCG.Route == route && otherCG.num == num;
		}
	}
}
