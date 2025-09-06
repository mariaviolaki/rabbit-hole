using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class CheckboxUI : MonoBehaviour
	{
		[SerializeField] Image checkedImage;

		bool isChecked;

		public bool IsChecked => isChecked;

		void Awake()
		{
			SetChecked(false);
		}

		public void SetChecked(bool isChecked)
		{
			this.isChecked = isChecked;
			checkedImage.enabled = isChecked;
		}
	}
}
