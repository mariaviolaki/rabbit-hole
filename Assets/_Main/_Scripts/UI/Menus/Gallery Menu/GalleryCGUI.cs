using IO;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Visuals;
using VN;

namespace UI
{
	public class GalleryCGUI : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] Image primaryImage;
		[SerializeField] CanvasGroup primaryCanvasGroup;
		[SerializeField] Image secondaryImage;
		[SerializeField] CanvasGroup secondaryCanvasGroup;
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] AssetManagerSO assetManager;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] CGBankSO cgBank;
		[SerializeField] AssetLabelReference cgLabel;

		UITransitionHandler transitionHandler;
		CharacterCG characterCG;
		CharacterCG nextCharacterCG;
		Sprite nextSprite;
		bool isTransitioning;
		bool isLoadingNextCG;

		void Awake()
		{
			transitionHandler = new(vnOptions);
			isTransitioning = false;
		}

		public void SetCG(CharacterCG characterCG)
		{
			if (isTransitioning) return;

			OpenCGContainer();
			StartCoroutine(ShowFirstCGProcess(characterCG));
		}

		void SetNextCG()
		{
			if (isTransitioning) return;

			StartCoroutine(ShowNextCGProcess());
		}

		void HideCG()
		{
			if (isTransitioning) return;

			StartCoroutine(HideAndGoBackProcess());
		}

		IEnumerator ShowFirstCGProcess(CharacterCG characterCG)
		{
			if (isTransitioning) yield break;
			isTransitioning = true;

			// Get the next CG stage to transition
			yield return assetManager.LoadImage(characterCG.ImageName, cgLabel);
			Sprite cgSprite = assetManager.GetImage(characterCG.ImageName, cgLabel);
			if (cgSprite == null)
			{
				// Clear and exit if the next CG is invalid
				yield return HideCGProcess();
				CloseCGContainer();
				yield break;
			}

			yield return FadeInCGProcess(characterCG, cgSprite);
			StartCoroutine(PreloadNextCGProcess());

			isTransitioning = false;
		}

		IEnumerator ShowNextCGProcess()
		{
			if (isTransitioning) yield break;

			isTransitioning = true;
			while (isLoadingNextCG) yield return null;

			if (nextCharacterCG == null || nextSprite == null)
			{
				yield return HideCGProcess();
				yield break;
			}

			yield return ReplaceCGProcess();
			StartCoroutine(PreloadNextCGProcess());

			isTransitioning = false;
		}

		IEnumerator HideAndGoBackProcess()
		{
			if (isTransitioning) yield break;
			isTransitioning = true;

			yield return HideCGProcess();
		}

		IEnumerator PreloadNextCGProcess()
		{
			if (characterCG.stage + 1 >= characterCG.stageCount) yield break;
			isLoadingNextCG = true;

			CharacterCG nextCharacterCG = cgBank.GetCG(characterCG.route, characterCG.num, characterCG.stage + 1);
			if (nextCharacterCG == null)
			{
				isLoadingNextCG = false;
				yield break;
			}

			// Get the next CG stage to transition
			yield return assetManager.LoadImage(nextCharacterCG.ImageName, cgLabel);
			nextSprite = assetManager.GetImage(nextCharacterCG.ImageName, cgLabel);
			if (nextSprite == null)
			{
				// Clear and exit if the next CG is invalid
				assetManager.UnloadImage(nextCharacterCG.ImageName, cgLabel);
				isLoadingNextCG = false;
				yield break;
			}

			this.nextCharacterCG = nextCharacterCG;
			isLoadingNextCG = false;
		}

		IEnumerator FadeInCGProcess(CharacterCG characterCG, Sprite cgSprite)
		{
			primaryCanvasGroup.alpha = 0f;
			primaryImage.sprite = cgSprite;
			yield return transitionHandler.SetVisibility(primaryCanvasGroup, true, vnOptions.General.TransitionSpeed);
			this.characterCG = characterCG;
		}

		IEnumerator ReplaceCGProcess()
		{
			secondaryCanvasGroup.alpha = 0f;
			secondaryImage.sprite = nextSprite;

			yield return transitionHandler.SetVisibility(secondaryCanvasGroup, true, vnOptions.General.TransitionSpeed);

			//yield return transitionHandler.Replace(primaryCanvasGroup, secondaryCanvasGroup, vnOptions.General.TransitionSpeed);
			SwapContainers();

			assetManager.UnloadImage(characterCG.ImageName, cgLabel);
			characterCG = nextCharacterCG;
			nextSprite = null;
			nextCharacterCG = null;
		}

		IEnumerator HideCGProcess()
		{
			yield return transitionHandler.SetVisibility(primaryCanvasGroup, false, vnOptions.General.TransitionSpeed);
			HideContainers();
			UnloadCGs();
			CloseCGContainer();
		}

		void OpenCGContainer()
		{
			HideContainers();
			gameObject.SetActive(true);
			inputManager.IsGalleryCGOpen = true;
			inputManager.OnCloseCG += HideCG;
		}

		void CloseCGContainer()
		{
			inputManager.OnCloseCG -= HideCG;
			inputManager.IsGalleryCGOpen = false;
			isTransitioning = false;
			gameObject.SetActive(false);
		}

		void UnloadCGs()
		{
			nextSprite = null;

			if (characterCG != null)
			{
				assetManager.UnloadImage(characterCG.ImageName, cgLabel);
				characterCG = null;
			}

			if (nextCharacterCG != null)
			{
				assetManager.UnloadImage(nextCharacterCG.ImageName, cgLabel);
				nextCharacterCG = null;
			}
		}

		void HideContainers()
		{
			primaryCanvasGroup.alpha = 0f;
			secondaryCanvasGroup.alpha = 0f;
			primaryImage.sprite = null;
			secondaryImage.sprite = null;
		}

		void SwapContainers()
		{
			primaryImage.sprite = secondaryImage.sprite;
			primaryCanvasGroup.alpha = 1f;
			secondaryCanvasGroup.alpha = 0f;
			secondaryImage.sprite = null;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			SetNextCG();
		}
	}
}
