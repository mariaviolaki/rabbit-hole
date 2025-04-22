using System.Collections;
using System.Threading.Tasks;
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

		protected override async Task Init()
		{
			await base.Init();

			// Load this character's model into the scene
			GameObject modelPrefab = await manager.FileManager.LoadModel3DPrefab(data.CastName);
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

			ChangeExpression(expressionName);
		}

		public Coroutine SetExpression(string expressionName, float speed = 0)
		{
			if (!IsValidExpression(expressionName)) return null;

			manager.StopProcess(ref expressionCoroutine);

			speed = speed <= 0 ? manager.GameOptions.Model3D.ExpressionTransitionSpeed : speed;
			expressionCoroutine = manager.StartCoroutine(TransitionExpression(expressionName, speed));
			return expressionCoroutine;
		}

		public override void FlipInstant()
		{
			modelContainer.localEulerAngles = new Vector3(0, -modelContainer.localEulerAngles.y, 0);
			isFacingRight = !isFacingRight;
		}
		protected override IEnumerator FlipDirection(float speed)
		{
			yield return FadeImage(canvasGroup, false, speed);

			modelContainer.localEulerAngles = new Vector3(0, -modelContainer.localEulerAngles.y, 0);

			yield return FadeImage(canvasGroup, true, speed);

			isFacingRight = !isFacingRight;
			directionCoroutine = null;
		}

		public override void ChangeBrightnessInstant(bool isHighlighted)
		{
			rawImage.color = isHighlighted ? LightColor : DarkColor;
			this.isHighlighted = isHighlighted;
		}
		protected override IEnumerator ChangeBrightness(bool isHighlighted, float speed)
		{
			yield return SetImageBrightness(rawImage, isHighlighted, speed);

			brightnessCoroutine = null;
		}

		public override void ChangeColorInstant(Color color)
		{
			rawImage.color = color;
			LightColor = color;
		}
		protected override IEnumerator ChangeColor(Color color, float speed)
		{
			yield return ColorImage(rawImage, color, speed);

			colorCoroutine = null;
		}

		protected override IEnumerator FadeImage(CanvasGroup canvasGroup, bool isFadeIn, float speed)
		{
			yield return base.FadeImage(canvasGroup, isFadeIn, speed);
			isVisible = isFadeIn;
		}

		protected override IEnumerator ColorImage(Graphic image, Color color, float speed)
		{
			yield return base.ColorImage(image, color, speed);
			LightColor = color;
		}

		protected override IEnumerator SetImageBrightness(Graphic image, bool isHighlighted, float speed)
		{
			yield return base.SetImageBrightness(image, isHighlighted, speed);
			this.isHighlighted = isHighlighted;
		}

		void ChangeExpression(string expressionName)
		{
			// Clear the old expression
			if (currentExpression != string.Empty)
				ChangeSubExpressions(currentExpression);

			// If a new expression was specified, change into this - and cache the new expression
			if (expressionName == string.Empty)
			{
				currentExpression = string.Empty;
			}
			else
			{
				ChangeSubExpressions(expressionName);
				currentExpression = expressionName;
			}
		}

		IEnumerator TransitionExpression(string expressionName, float speed)
		{
			float transitionDuration = (1 / speed) * expressionTransitionMultiplier;

			// Fade off the old expression
			if (currentExpression != string.Empty)
			{
				float fadeOffDuration = expressionName == null ? transitionDuration : (transitionDuration / 2);
				yield return TransitionSubExpressions(currentExpression, fadeOffDuration);
			}

			// If a new expression was specified, transition into this - and cache the new expression
			if (expressionName == string.Empty)
			{
				currentExpression = string.Empty;
			}
			else
			{
				yield return TransitionSubExpressions(expressionName, transitionDuration);
				currentExpression = expressionName;
			}

			expressionCoroutine = null;
		}

		void ChangeSubExpressions(string expressionName)
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

		IEnumerator TransitionSubExpressions(string expressionName, float transitionDuration)
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
