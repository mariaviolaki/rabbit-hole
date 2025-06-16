using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class ChoiceButtonUI : FadeableUI
{
	[SerializeField] Button button;
	[SerializeField] TextMeshProUGUI buttonText;

	ObjectPool<ChoiceButtonUI> buttonPool;
	int index = -1;
	string text = "";

	public event Action<int, string> OnSelect;

	public void Initialize(ObjectPool<ChoiceButtonUI> buttonPool)
	{
		this.buttonPool = buttonPool;
	}

	public void ClearData()
	{
		index = -1;
		text = "";
		buttonText.text = text;
	}

	public void UpdateData(int index, string text)
	{
		this.index = index;
		this.text = text;

		buttonText.text = text;
		button.onClick.AddListener(SelectChoice);
	}

	public void RemoveListeners()
	{
		button.onClick.RemoveAllListeners();
	}

	public void Release()
	{
		buttonPool.Release(this);
	}

	void SelectChoice()
	{
		OnSelect?.Invoke(index, text);	
	}
}
