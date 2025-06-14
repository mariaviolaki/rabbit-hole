using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class Model3DCharacter : GraphicsCharacter
	{
		const string primaryContainerName = "Primary";
		const string secondaryContainerName = "Secondary";
		const int ModelHeightOffset = 15;
		const float expressionTransitionMultiplier = 5f;

		Transform modelRoot;
		Model3DExpressionDirectory expressionDirectory;

		GameObject primaryImageContainer;
		RawImage primaryRawImage;
		CanvasGroup primaryCanvasGroup;
		Camera primaryModelCamera;
		Transform primaryModelContainer;
		Animator primaryModelAnimator;
		SkinnedMeshRenderer primaryMeshRenderer;
		
		GameObject secondaryImageContainer;
		RawImage secondaryRawImage;
		CanvasGroup secondaryCanvasGroup;
		Camera secondaryModelCamera;
		Transform secondaryModelContainer;
		Animator secondaryModelAnimator;
		SkinnedMeshRenderer secondaryMeshRenderer;

		string currentExpression = string.Empty;
		Coroutine expressionCoroutine;

		protected override IEnumerator Init()
		{
			yield return base.Init();

			// Load this character's model into the scene
			yield return manager.FileManager.LoadModel3DPrefab(data.CastName);
			GameObject modelPrefab = manager.FileManager.GetModel3DPrefab(data.CastName);
			if (modelPrefab == null) yield break;

			GameObject modelRootGameObject = Object.Instantiate(modelPrefab, manager.Model3DContainer);
			int model3DCount = manager.GetCharacterCount(CharacterType.Model3D);
			modelRoot = modelRootGameObject.GetComponent<Transform>();
			modelRoot.localPosition = new Vector3(0, model3DCount * ModelHeightOffset, 0);
			modelRoot.name = data.Name;

			// Create a primary image to swap with the secondary for smooth transitions
			RenderTexture renderTexture = new RenderTexture(manager.GameOptions.Model3D.RenderTexture3D);
			primaryImageContainer = animator.transform.GetChild(0).gameObject;
			primaryCanvasGroup = primaryImageContainer.GetComponent<CanvasGroup>();
			primaryRawImage = primaryImageContainer.GetComponent<RawImage>();
			primaryModelCamera = modelRoot.GetComponentInChildren<Camera>();
			primaryModelContainer = primaryModelCamera.transform.GetChild(0);
			primaryModelAnimator = primaryModelContainer.GetComponentInChildren<Animator>();
			primaryMeshRenderer = primaryModelAnimator.GetComponentInChildren<SkinnedMeshRenderer>();
			expressionDirectory = primaryMeshRenderer.GetComponent<Model3DExpressionDirectory>();

			// Setup how the 3D model is displayed in the UI
			primaryRawImage.texture = renderTexture;
			primaryImageContainer.name = primaryContainerName;
			primaryModelCamera.targetTexture = renderTexture;
			primaryModelContainer.localEulerAngles = new Vector3(0, manager.GameOptions.Model3D.DefaultAngle, 0);

			// Create a secondary image for smooth transitions
			RenderTexture secondaryRenderTexture = new RenderTexture(manager.GameOptions.Model3D.RenderTexture3D);
			secondaryImageContainer = Object.Instantiate(primaryImageContainer, primaryImageContainer.transform.parent);
			secondaryCanvasGroup = secondaryImageContainer.GetComponent<CanvasGroup>();
			secondaryRawImage = secondaryImageContainer.GetComponent<RawImage>();
			secondaryModelCamera = Object.Instantiate(primaryModelCamera, primaryModelCamera.transform.parent);
			secondaryModelContainer = secondaryModelCamera.transform.GetChild(0);
			secondaryModelAnimator = secondaryModelContainer.GetComponentInChildren<Animator>();
			secondaryMeshRenderer = secondaryModelAnimator.GetComponentInChildren<SkinnedMeshRenderer>();

			// Setup how the secondary 3D model is displayed in the UI (but hide it on startup)
			ToggleSecondaryImage(false);
			secondaryRawImage.texture = secondaryRenderTexture;
			secondaryImageContainer.name = secondaryContainerName;
			secondaryModelCamera.targetTexture = secondaryRenderTexture;
			secondaryModelContainer.localEulerAngles = new Vector3(0, manager.GameOptions.Model3D.DefaultAngle, 0);
		}

		public void SetMotion(string motionName)
		{
			primaryModelAnimator.SetTrigger(motionName);
			secondaryModelAnimator.SetTrigger(motionName);
		}

		public void SetExpressionInstant(string expressionName)
		{
			if (!IsValidExpression(expressionName)) return;

			manager.StopProcess(ref expressionCoroutine);
			ChangeExpressionInstant(expressionName);
		}

		public Coroutine SetExpression(string expressionName, float speed = 0)
		{
			if (!IsValidExpression(expressionName)) return null;

			bool isSkipped = manager.StopProcess(ref expressionCoroutine);

			expressionCoroutine = manager.StartCoroutine(ChangeExpression(expressionName, speed, isSkipped));
			return expressionCoroutine;
		}

		public override void FlipInstant()
		{
			manager.StopProcess(ref directionCoroutine);

			Vector3 currentLocalScale = primaryRawImage.transform.localScale;
			primaryRawImage.transform.localScale = new Vector3(-currentLocalScale.x, currentLocalScale.y, currentLocalScale.z);
			isFacingRight = !isFacingRight;
		}
		protected override IEnumerator FlipDirection(float speed, bool isSkipped)
		{
			ToggleSecondaryImage(true);

			Vector3 currentLocalScale = primaryRawImage.transform.localScale;
			secondaryCanvasGroup.alpha = 0f;
			secondaryCanvasGroup.transform.localScale = new Vector3(-currentLocalScale.x, currentLocalScale.y, currentLocalScale.z);

			speed = GetTransitionSpeed(speed, manager.GameOptions.Model3D.ExpressionTransitionSpeed, isSkipped);
			yield return TransitionHandler.Replace(primaryCanvasGroup, secondaryCanvasGroup, speed);

			ToggleSecondaryImage(false);
			isFacingRight = !isFacingRight;
			directionCoroutine = null;
		}

		public override void ChangeBrightnessInstant(bool isHighlighted)
		{
			manager.StopProcess(ref brightnessCoroutine);

			primaryRawImage.color = isHighlighted ? LightColor : DarkColor;
			this.isHighlighted = isHighlighted;
		}
		protected override IEnumerator ChangeBrightness(bool isHighlighted, float speed, bool isSkipped)
		{
			yield return SetImageBrightness(primaryRawImage, isHighlighted, speed, isSkipped);

			brightnessCoroutine = null;
		}

		public override void ChangeColorInstant(Color color)
		{
			manager.StopProcess(ref colorCoroutine);

			primaryRawImage.color = color;
			LightColor = color;
		}
		protected override IEnumerator ChangeColor(Color color, float speed, bool isSkipped)
		{
			yield return ColorImage(primaryRawImage, color, speed, isSkipped);

			colorCoroutine = null;
		}

		IEnumerator ColorImage(Graphic image, Color color, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.ColorTransitionSpeed, isSkipped);
			Color targetColor = isHighlighted ? color : GetDarkColor(color);

			yield return TransitionHandler.SetColor(image, targetColor, speed);

			LightColor = color;
		}

		IEnumerator SetImageBrightness(Graphic image, bool isHighlighted, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Characters.BrightnessTransitionSpeed, isSkipped);
			Color targetColor = isHighlighted ? LightColor : DarkColor;

			yield return TransitionHandler.SetColor(image, targetColor, speed);

			this.isHighlighted = isHighlighted;
		}

		void ChangeExpressionInstant(string expressionName)
		{
			// Clear the old expression
			if (currentExpression != string.Empty)
				ChangeSubExpressionsInstant(currentExpression);

			// If a new expression was specified, change into this - and cache the new expression
			if (expressionName == string.Empty)
			{
				currentExpression = string.Empty;
			}
			else
			{
				ChangeSubExpressionsInstant(expressionName);
				currentExpression = expressionName;
			}
		}

		IEnumerator ChangeExpression(string expressionName, float speed, bool isSkipped)
		{
			speed = GetTransitionSpeed(speed, manager.GameOptions.Model3D.ExpressionTransitionSpeed, isSkipped);
			float transitionDuration = (1 / speed) * expressionTransitionMultiplier;

			// Fade off the old expression
			if (currentExpression != string.Empty)
			{
				float fadeOffDuration = expressionName == null ? transitionDuration : (transitionDuration / 2);
				yield return ChangeSubExpressions(currentExpression, fadeOffDuration);
			}

			// If a new expression was specified, transition into this - and cache the new expression
			if (expressionName == string.Empty)
			{
				currentExpression = string.Empty;
			}
			else
			{
				yield return ChangeSubExpressions(expressionName, transitionDuration);
				currentExpression = expressionName;
			}

			expressionCoroutine = null;
		}

		void ChangeSubExpressionsInstant(string expressionName)
		{
			bool isNewExpression = expressionName != currentExpression;
			SubExpression[] currentSubExpressions = expressionDirectory.Expressions[expressionName];

			// Immediately set all the sub-expressions listed for the main expression
			foreach (SubExpression subExpression in currentSubExpressions)
			{
				float expressionWeight = isNewExpression ? subExpression.Weight : 0f;
				primaryMeshRenderer.SetBlendShapeWeight(subExpression.Index, expressionWeight);
				secondaryMeshRenderer.SetBlendShapeWeight(subExpression.Index, expressionWeight);
			}
		}

		IEnumerator ChangeSubExpressions(string expressionName, float transitionDuration)
		{
			bool isNewExpression = expressionName != currentExpression;
			SubExpression[] currentSubExpressions = expressionDirectory.Expressions[expressionName];

			float timeElapsed = 0;
			while (timeElapsed < transitionDuration)
			{
				timeElapsed += Time.deltaTime;
				float smoothProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timeElapsed / transitionDuration));

				// Smoothly transition all the sub-expressions listed for the main expression
				foreach (SubExpression subExpression in currentSubExpressions)
				{
					float startWeight = isNewExpression ? 0f : subExpression.Weight;
					float endWeight = isNewExpression ? subExpression.Weight : 0f;
					float expressionWeight = Mathf.Lerp(startWeight, endWeight, smoothProgress);

					primaryMeshRenderer.SetBlendShapeWeight(subExpression.Index, expressionWeight);
					secondaryMeshRenderer.SetBlendShapeWeight(subExpression.Index, expressionWeight);
				}

				yield return null;
			}
		}

		bool IsValidExpression(string expressionName)
		{
			if (expressionName == currentExpression) return false;
			if (expressionName != string.Empty && !expressionDirectory.Expressions.ContainsKey(expressionName))
			{
				Debug.LogError($"No expression '{expressionName}' found for 3D Model: '{data.CastName}'");
				return false;
			}

			return true;
		}

		void ToggleSecondaryImage(bool isActive)
		{
			if (!isActive)
			{
				// Swap the visual containers because the graphic is now in the second one
				SwapContainers();
				secondaryCanvasGroup.alpha = primaryCanvasGroup.alpha;
				secondaryRawImage.transform.localScale = primaryRawImage.transform.localScale;
				secondaryRawImage.color = primaryRawImage.color;	
			}

			secondaryModelCamera.gameObject.SetActive(isActive);
			secondaryImageContainer.SetActive(isActive);
		}

		void SwapContainers()
		{
			GameObject tempImageContainer = primaryImageContainer;
			RawImage tempRawImage = primaryRawImage;
			CanvasGroup tempCanvasGroup = primaryCanvasGroup;
			Camera tempCamera = primaryModelCamera;
			Transform tempModelContainer = primaryModelContainer;
			Animator tempModelAnimator = primaryModelAnimator;
			SkinnedMeshRenderer tempMeshRenderer = primaryMeshRenderer;
			
			primaryImageContainer = secondaryImageContainer;
			primaryRawImage = secondaryRawImage;
			primaryCanvasGroup = secondaryCanvasGroup;
			primaryModelCamera = secondaryModelCamera;
			primaryModelContainer = secondaryModelContainer;
			primaryModelAnimator = secondaryModelAnimator;
			primaryMeshRenderer = secondaryMeshRenderer;
			
			secondaryImageContainer = tempImageContainer;
			secondaryRawImage = tempRawImage;
			secondaryCanvasGroup = tempCanvasGroup;
			secondaryModelCamera = tempCamera;
			secondaryModelContainer = tempModelContainer;
			secondaryModelAnimator = tempModelAnimator;
			secondaryMeshRenderer = tempMeshRenderer;
		}
	}
}
