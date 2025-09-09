using System.Collections;
using UnityEngine;

namespace Characters
{
	public abstract class Character : MonoBehaviour
	{
		[SerializeField] protected GameOptionsSO gameOptions;
		protected CharacterData data;
		protected RectTransform root;
		protected CharacterManager manager;

		public CharacterManager Manager => manager;
		public RectTransform Root => root;
		public CharacterData Data => data;
		public GameOptionsSO GameOptions => gameOptions;

		virtual protected void Update()
		{
		}

		public void InitializeBase(CharacterManager manager, CharacterData data)
		{
			this.manager = manager;
			this.data = data;
			gameObject.name = data.Name;
		}

		virtual public IEnumerator Initialize(CharacterManager manager, CharacterData data)
		{
			InitializeBase(manager, data);
			yield break;
		}

		public void ResetData()
		{
			data = manager.Bank.GetCharacterData(Data.Name, Data.CastName, manager.GameOptions);
		}

		public void SetName(string name)
		{
			if (string.IsNullOrEmpty(name)) return;

			data.Name = name;
		}
	}
}
