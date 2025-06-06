using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class Model3DCharacter : GraphicsCharacter
	{
		const int ModelHeightOffset = 15;
		const float expressionTransitionMultiplier = 5f;

		Transform modelRoot;
		Camera modelCamera;
		Transform modelContainer;
		Animator modelAnimator;
		RawImage rawImage;
		SkinnedMeshRenderer skinnedMeshRenderer;
		Model3DExpressionDirectory expressionDirectory;

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
			RenderTexture renderTexture = new RenderTexture(manager.GameOptions.Model3D.RenderTexture3D);
			int model3DCount = manager.GetCharacterCount(CharacterType.Model3D);

			modelRoot = modelRootGameObject.GetComponent<Transform>();
			modelCamera = modelRoot.GetComponentInChildren<Camera>();
			modelContainer = modelCamera.transform.GetChild(0);
			modelAnimator = modelContainer.GetComponentInChildren<Animator>();
			skinnedMeshRenderer = modelAnimator.GetComponentInChildren<SkinnedMeshRenderer>();
			expressionDirectory = skinnedMeshRenderer.GetComponent<Model3DExpressionDirectory>();
			rawImage = animator.GetComponentInChildren<RawImage>();

			modelRoot.localPosition = new Vector3(0, model3DCount * ModelHeightOffset, 0);
			modelContainer.localEulerAngles = new Vector3(0, manager.GameOptions.Model3D.DefaultAngle, 0);
			rawImage.texture = renderTexture;
			modelCamera.targetTexture = renderTexture;
			modelRoot.name = data.Name;
		}

		public void SetMotion(string motionName)
		{
			modelAnimator.SetTrigger(motionName);
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

			modelContainer.localEulerAngles = new Vector3(0, -modelContainer.localEulerAngles.y, 0);
			isFacingRight = !isFacingRight;
		}
		protected override IEnumerator FlipDirection(float speed, bool isSkipped)
		{
			yield return FadeImage(canvasGroup, false, speed, isSkipped);

			modelContainer.localEulerAngles = new Vector3(0, -modelContainer.localEulerAngles.y, 0);

			yield return FadeImage(canvasGroup, true, speed, isSkipped);

			isFacingRight = !isFacingRight;
			directionCoroutine = null;
		}

		public override void ChangeBrightnessInstant(bool isHighlighted)
		{
			manager.StopProcess(ref brightnessCoroutine);

			rawImage.color = isHighlighted ? LightColor : DarkColor;
			this.isHighlighted = isHighlighted;
		}
		protected override IEnumerator ChangeBrightness(bool isHighlighted, float speed, bool isSkipped)
		{
			yield return SetImageBrightness(rawImage, isHighlighted, speed, isSkipped);

			brightnessCoroutine = null;
		}

		public override void ChangeColorInstant(Color color)
		{
			manager.StopProcess(ref colorCoroutine);

			rawImage.color = color;
			LightColor = color;
		}
		protected override IEnumerator ChangeColor(Color color, float speed, bool isSkipped)
		{
			yield return ColorImage(rawImage, color, speed, isSkipped);

			colorCoroutine = null;
		}

		protected override IEnumerator FadeImage(CanvasGroup canvasGroup, bool isFadeIn, float speed, bool isSkipped)
		{
			yield return base.FadeImage(canvasGroup, isFadeIn, speed, isSkipped);
			isVisible = isFadeIn;
		}

		protected override IEnumerator ColorImage(Graphic image, Color color, float speed, bool isSkipped)
		{
			yield return base.ColorImage(image, color, speed, isSkipped);
			LightColor = color;
		}

		protected override IEnumerator SetImageBrightness(Graphic image, bool isHighlighted, float speed, bool isSkipped)
		{
			yield return base.SetImageBrightness(image, isHighlighted, speed, isSkipped);
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
				skinnedMeshRenderer.SetBlendShapeWeight(subExpression.Index, expressionWeight);
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

					skinnedMeshRenderer.SetBlendShapeWeight(subExpression.Index, expressionWeight);
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
	}
}
