using Gameplay;
using System.Collections.Generic;
using UnityEngine;

namespace Visuals
{
	[CreateAssetMenu(fileName = "CG Bank", menuName = "Scriptable Objects/CG Bank")]
	public class CGBankSO : ScriptableObject
	{
		// Ordered by character, then CG number, then stage
		public List<CharacterCG> cgs = new();

		readonly Dictionary<CharacterRoute, List<List<CharacterCG>>> cgLookup = new();

		void OnEnable()
		{
			cgLookup.Clear();
			BuildLookup();
		}

		public CharacterCG GetCG(CharacterRoute route, int num, int stage = 0)
		{
			if (!cgLookup.TryGetValue(route, out List<List<CharacterCG>> characterCGs)) return null;
			if (num <= 0 || num > characterCGs.Count) return null;

			List<CharacterCG> cgVariations = characterCGs[num - 1];
			if (stage < 0 || stage >= cgVariations.Count) return null;

			return cgVariations[stage];
		}

		public List<CharacterCG> GetBaseCGs(CharacterRoute route)
		{
			if (!cgLookup.TryGetValue(route, out List<List<CharacterCG>> characterCGs)) return new();

			List<CharacterCG> baseCGs = new();
			foreach (List<CharacterCG> cgVariations in characterCGs)
			{
				baseCGs.Add(cgVariations[0]);
			}

			return baseCGs;
		}

		void BuildLookup()
		{
			foreach (CharacterCG cg in cgs)
			{
				if (!cgLookup.TryGetValue(cg.route, out List<List<CharacterCG>> characterCGs))
				{
					cgLookup.Add(cg.route, new List<List<CharacterCG>>());
					characterCGs = cgLookup[cg.route];
				}

				while (characterCGs.Count < cg.num)
					characterCGs.Add(new List<CharacterCG>());

				List<CharacterCG> cgVariations = characterCGs[cg.num - 1];
				while (cgVariations.Count <= cg.stage)
					cgVariations.Add(null);

				cgVariations[cg.stage] = cg;
			}
		}
	}
}
