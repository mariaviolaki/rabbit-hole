using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class Model3DCharacter : GraphicsCharacter
	{
		const int ModelHeightOffset = 15;
		const float ModelExpressionTransitionMultiplier = 5f;

		Transform modelRoot;
		Camera modelCamera;
		Transform modelContainer;
		Animator modelAnimator;
		RawImage rawImage;
		SkinnedMeshRenderer skinnedMeshRenderer;
		Model3DExpressionDirectory expressionDirectory;

		string currentExpression;
		Coroutine expressionCoroutine;

		protected override async Task Init()
		{
			await base.Init();

			// Load this character's model into the scene
			GameObject modelPrefab = await manager.FileManager.LoadModel3DPrefab(data.CastName);
			GameObject modelRootGameObject = Object.Instantiate(modelPrefab, manager.Model3DContainer);
			RenderTexture renderTexture = new RenderTexture(manager.GameOptions.RenderTexture3D);
			int model3DCount = manager.GetCharacterCount(CharacterType.Model3D);

			modelRoot = modelRootGameObject.GetComponent<Transform>();
			modelCamera = modelRoot.GetComponentInChildren<Camera>();
			modelContainer = modelRoot.GetChild(0);
			modelAnimator = modelContainer.GetComponentInChildren<Animator>();
			skinnedMeshRenderer = modelAnimator.GetComponentInChildren<SkinnedMeshRenderer>();
			expressionDirectory = skinnedMeshRenderer.GetComponent<Model3DExpressionDirectory>();
			rawImage = animator.GetComponentInChildren<RawImage>();

			modelRoot.localPosition = new Vector3(0, model3DCount * ModelHeightOffset, 0);
			rawImage.texture = renderTexture;
			modelCamera.targetTexture = renderTexture;
			modelRoot.name = data.Name;

			Debug.Log($"Created 3D Character: {Data.Name}");
		}

		public override Coroutine Darken(float speed = 0)
		{
			return null;
		}

		public override Coroutine FaceLeft(float speed = 0)
		{
			return null;
		}

		public override Coroutine FaceRight(float speed = 0)
		{
			return null;
		}

		public override Coroutine Flip(float speed = 0)
		{
			return null;
		}

		public override Coroutine Lighten(float speed = 0)
		{
			return null;
		}

		public void SetMotion(string motionName)
		{
			modelAnimator.SetTrigger(motionName);
		}

		public Coroutine SetExpression(string expressionName, float speed = -1)
		{
			if (expressionName == currentExpression) return null;

			if (expressionName != null && !expressionDirectory.Expressions.ContainsKey(expressionName))
			{
				Debug.LogError($"No expression '{expressionName}' found for 3D Model: '{data.CastName}'");
				return null;
			}

			manager.StopProcess(ref expressionCoroutine);

			speed = speed <= 0 ? manager.GameOptions.ModelExpressionSpeed : speed;
			expressionCoroutine = manager.StartCoroutine(TransitionExpression(expressionName, speed));
			return expressionCoroutine;
		}

		IEnumerator TransitionExpression(string expressionName, float speed)
		{
			float transitionDuration = (1 / speed) * ModelExpressionTransitionMultiplier;

			// Fade off the old expression
			if (currentExpression != null)
			{
				float fadeOffDuration = expressionName == null ? transitionDuration : (transitionDuration / 2);
				yield return TransitionSubExpressions(currentExpression, fadeOffDuration);
			}

			// If a new expression was specified, transition into this - and cache the new expression
			if (expressionName == null)
			{
				currentExpression = null;
			}
			else
			{
				yield return TransitionSubExpressions(expressionName, transitionDuration);
				currentExpression = expressionName;
			}

			expressionCoroutine = null;
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
	}
}
