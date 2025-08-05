using Dialogue;
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
	DialogueChoice choice;

	public event Action<DialogueChoice> OnSelect;

	public void Initialize(ObjectPool<ChoiceButtonUI> buttonPool)
	{
		this.buttonPool = buttonPool;
	}

	public void ClearData()
	{
		choice = null;
		buttonText.text = "";
	}

	public void UpdateChoice(DialogueChoice choice)
	{
		this.choice = choice;
		buttonText.text = choice.Text;
	}

	public void EnableListeners()
	{
		button.onClick.AddListener(SelectChoice);
	}

	public void DisableListeners()
	{
		button.onClick.RemoveAllListeners();
	}

	public void Release()
	{
		buttonPool.Release(this);
	}

	void SelectChoice()
	{
		OnSelect?.Invoke(choice);	
	}
}
