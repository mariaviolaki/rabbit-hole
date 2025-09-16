using Dialogue;
using TMPro;
using UnityEngine;

namespace UI
{
	public class DialogueContinuePromptUI : MonoBehaviour
	{
		[SerializeField] Canvas dialogueCanvas;
		[SerializeField] TextMeshProUGUI dialogueText;
		[SerializeField] Animator animator;
		[SerializeField] VNOptionsSO vnOptions;

		RectTransform rectTransform;

		public bool IsVisible => animator.gameObject.activeSelf;

		void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			Hide();
		}

		public void Show()
		{
			if (vnOptions.Dialogue.PromptPos == PromptPosition.TextboxBottomRight)
				rectTransform.localPosition = GetContainerPosition();
			else
				rectTransform.localPosition = GetTextPosition();

			animator.gameObject.SetActive(true);
		}

		public void Hide()
		{
			animator.gameObject.SetActive(false);
		}

		Vector2 GetTextPosition()
		{
			// Get the most up-to-date text data
			dialogueText.ForceMeshUpdate();

			TMP_TextInfo textInfo = dialogueText.textInfo;

			int lastVisibleIndex = -1;
			for (int i = textInfo.characterCount - 1; i >= 0; i--)
			{
				if (textInfo.characterInfo[i].isVisible)
				{
					lastVisibleIndex = i;
					break;
				}
			}

			// Don't position image relative to text if no visible text is found
			if (lastVisibleIndex == -1)
				return GetContainerPosition();

			RectTransform parentRect = rectTransform.parent as RectTransform;
			Camera canvasCamera = dialogueCanvas.worldCamera;

			// Calculate the correct position inside the parent to position the image
			Vector3 worldPos = dialogueText.transform.TransformPoint(textInfo.characterInfo[lastVisibleIndex].bottomRight);
			Vector3 screenPos = canvasCamera.WorldToScreenPoint(worldPos);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, canvasCamera, out Vector2 localPos);

			float xOffset = dialogueText.fontSize * 0.8f;
			float yOffset = dialogueText.fontSize * 0.4f;

			return localPos + new Vector2(xOffset, yOffset);
		}

		Vector2 GetContainerPosition()
		{
			Vector3[] dialogueWorldCorners = new Vector3[4];
			dialogueText.rectTransform.GetWorldCorners(dialogueWorldCorners);

			RectTransform parentRect = rectTransform.parent as RectTransform;
			Camera canvasCamera = dialogueCanvas.worldCamera;

			// Calculate the screen position of the bottom right corner of the textbox
			Vector2 worldPos = dialogueWorldCorners[3]; // bottom right
			Vector2 screenPos = canvasCamera.WorldToScreenPoint(worldPos);

			RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, canvasCamera, out Vector2 localPos);

			float dialogueHeight = dialogueText.rectTransform.rect.size.y;
			float xOffset = -dialogueHeight * 0.1f;
			float yOffset = dialogueHeight * 0.2f;

			return localPos + new Vector2(xOffset, yOffset);
		}
	}
}
