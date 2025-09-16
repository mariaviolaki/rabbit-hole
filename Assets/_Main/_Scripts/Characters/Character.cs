using System.Collections;
using UnityEngine;

namespace Characters
{
	public abstract class Character : MonoBehaviour
	{
		[SerializeField] protected VNOptionsSO vnOptions;
		protected CharacterData data;
		protected RectTransform root;
		protected CharacterManager manager;

		public CharacterManager Manager => manager;
		public RectTransform Root => root;
		public CharacterData Data => data;
		public VNOptionsSO GameOptions => vnOptions;

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
			data = manager.Bank.GetCharacterData(Data.Name, Data.CastName, manager.Options);
		}

		public void SetName(string name)
		{
			if (string.IsNullOrEmpty(name)) return;

			data.Name = name;
		}
	}
}
