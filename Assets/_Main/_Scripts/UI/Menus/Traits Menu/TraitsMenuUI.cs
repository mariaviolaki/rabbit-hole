using Game;
using UnityEngine;
using UnityEngine.UI;
using Variables;

namespace UI
{
	public class TraitsMenuUI : MenuBaseUI
	{
		[SerializeField] Slider honestySlider;
		[SerializeField] Slider kindnessSlider;
		[SerializeField] Slider curiositySlider;
		[SerializeField] GameSceneManager sceneManager;
		[SerializeField] VariableManager variableManager;

		override protected void Awake()
		{
			base.Awake();

			honestySlider.minValue = DefaultVariables.MinHonesty;
			honestySlider.maxValue = DefaultVariables.MaxHonesty;
			kindnessSlider.minValue = DefaultVariables.MinKindness;
			kindnessSlider.maxValue = DefaultVariables.MaxKindness;
			curiositySlider.minValue = DefaultVariables.MinCuriosity;
			curiositySlider.maxValue = DefaultVariables.MaxCuriosity;
		}

		override protected bool PrepareOpen(MenuType menuType)
		{
			if (int.TryParse(variableManager.Get(DefaultVariables.HonestyVariable).ToString(), out int honestyValue))
				honestySlider.value = Mathf.Clamp(honestyValue, DefaultVariables.MinHonesty, DefaultVariables.MaxHonesty);
			if (int.TryParse(variableManager.Get(DefaultVariables.KindnessVariable).ToString(), out int kindnessValue))
				kindnessSlider.value = Mathf.Clamp(kindnessValue, DefaultVariables.MinKindness, DefaultVariables.MaxKindness);
			if (int.TryParse(variableManager.Get(DefaultVariables.CuriosityVariable).ToString(), out int curiosityValue))
				curiositySlider.value = Mathf.Clamp(curiosityValue, DefaultVariables.MinCuriosity, DefaultVariables.MaxCuriosity);

			return base.PrepareOpen(menuType);
		}
	}
}
