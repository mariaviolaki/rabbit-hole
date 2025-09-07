using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Variables;

namespace UI
{
	public class TraitsMenuUI : FadeableUI
	{
		[SerializeField] Slider honestySlider;
		[SerializeField] Slider kindnessSlider;
		[SerializeField] Slider curiositySlider;
		[SerializeField] Button backButton;
		[SerializeField] MenusUI menus;
		[SerializeField] GameManager gameManager;

		bool isTransitioning = false;

		public event Action OnClose;

		public bool IsTransitioning => isTransitioning;

		protected override void Awake()
		{
			base.Awake();

			honestySlider.minValue = DefaultVariables.MinHonesty;
			honestySlider.maxValue = DefaultVariables.MaxHonesty;
			kindnessSlider.minValue = DefaultVariables.MinKindness;
			kindnessSlider.maxValue = DefaultVariables.MaxKindness;
			curiositySlider.minValue = DefaultVariables.MinCuriosity;
			curiositySlider.maxValue = DefaultVariables.MaxCuriosity;
		}

		override protected void OnEnable()
		{
			base.OnEnable();
			SubscribeListeners();
		}

		override protected void OnDisable()
		{
			base.OnDisable();
			UnsubscribeListeners();
		}

		public IEnumerator Open(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible || isTransitioning) yield break;
			isTransitioning = true;

			base.fadeSpeed = fadeSpeed;
			isImmediateTransition = isImmediate;

			PrepareOpen();

			yield return SetVisible(isImmediate, fadeSpeed);

			isTransitioning = false;
		}

		public IEnumerator Close(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsHidden || isTransitioning) yield break;
			isTransitioning = true;

			fadeSpeed = fadeSpeed <= 0 ? gameOptions.General.TransitionSpeed : fadeSpeed;
			yield return SetHidden(isImmediate, fadeSpeed);

			isTransitioning = false;
			OnClose?.Invoke();
		}

		void PrepareOpen()
		{
			if (int.TryParse(gameManager.Variables.Get(DefaultVariables.HonestyVariable).ToString(), out int honestyValue))
				honestySlider.value = Mathf.Clamp(honestyValue, DefaultVariables.MinHonesty, DefaultVariables.MaxHonesty);
			if (int.TryParse(gameManager.Variables.Get(DefaultVariables.KindnessVariable).ToString(), out int kindnessValue))
				kindnessSlider.value = Mathf.Clamp(kindnessValue, DefaultVariables.MinKindness, DefaultVariables.MaxKindness);
			if (int.TryParse(gameManager.Variables.Get(DefaultVariables.CuriosityVariable).ToString(), out int curiosityValue))
				curiositySlider.value = Mathf.Clamp(curiosityValue, DefaultVariables.MinCuriosity, DefaultVariables.MaxCuriosity);
		}

		void SubscribeListeners()
		{
			backButton.onClick.AddListener(menus.CloseMenu);
		}

		void UnsubscribeListeners()
		{
			backButton.onClick.RemoveListener(menus.CloseMenu);
		}
	}
}
