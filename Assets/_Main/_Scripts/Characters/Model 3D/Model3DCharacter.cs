using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class Model3DCharacter : GraphicsCharacter
	{
		const int ModelHeightOffset = 15;
		const float ExpressionSpeedMultiplier = 40f;

		readonly Dictionary<string, string> motionNames = new(StringComparer.OrdinalIgnoreCase);

		Transform modelRoot;
		Model3DExpressionBank expressionBank;
		Camera modelCamera;
		Transform modelContainer;
		Animator modelAnimator;
		SkinnedMeshRenderer skinnedMeshRenderer;

		RawImage primaryRawImage;
		RawImage secondaryRawImage;

		string expression;
		float expressionSpeed;
		TransitionStatus expressionStatus = TransitionStatus.Completed;

		public string Expression => expression;

		public bool IsTransitioningExpression() => expressionStatus != TransitionStatus.Completed;

		protected override void Update()
		{
			base.Update();

			TransitionColor();
			TransitionExpression();
		}

		override public IEnumerator Initialize(CharacterManager manager, CharacterData data)
		{
			yield return base.Initialize(manager, data);

			// Load this character's model into the scene
			yield return manager.Assets.LoadModel3DPrefab(data.CastName);
			GameObject modelPrefab = manager.Assets.GetModel3DPrefab(data.CastName);
			if (modelPrefab == null) yield break;

			GameObject modelRootGameObject = Instantiate(modelPrefab, manager.Model3DContainer);
			int model3DCount = manager.GetCharacterCount(CharacterType.Model3D);
			modelRoot = modelRootGameObject.GetComponent<Transform>();
			modelRoot.localPosition = new Vector3(0, model3DCount * ModelHeightOffset, 0);
			modelRoot.name = data.Name;

			// Create a primary image to swap with the secondary for smooth transitions
			RenderTexture renderTexture = new RenderTexture(manager.Options.Model3D.RenderTexture3D);
			primaryRawImage = animator.transform.GetChild(0).GetComponentInChildren<RawImage>();
			primaryRawImage.texture = renderTexture;
			secondaryRawImage = animator.transform.GetChild(1).GetComponentInChildren<RawImage>();
			secondaryRawImage.texture = renderTexture;

			modelCamera = modelRoot.GetComponentInChildren<Camera>();
			modelContainer = modelCamera.transform.GetChild(0);
			modelAnimator = modelContainer.GetComponentInChildren<Animator>();
			skinnedMeshRenderer = modelAnimator.GetComponentInChildren<SkinnedMeshRenderer>();
			expressionBank = skinnedMeshRenderer.GetComponent<Model3DExpressionBank>();
			modelCamera.targetTexture = renderTexture;
			modelContainer.localEulerAngles = new Vector3(0, manager.Options.Model3D.DefaultAngle, 0);

			expression = string.Empty;
			CacheMotionNames();
			SetColorImmediate();
		}

		public void SetMotion(string motionName)
		{
			if (!motionNames.TryGetValue(motionName, out string cachedName))
			{
				Debug.LogWarning($"Animation '{motionName}' not found for 3D character '{data.Name}'.");
				return;
			}

			modelAnimator.SetTrigger(cachedName);
		}

		public void SetExpression(string expression, bool isImmediate = false, float speed = 0f)
		{
			if (expression == this.expression && expressionStatus == TransitionStatus.Completed) return;

			// Allow null or empty strings to clear the current expression
			bool isClearingExpression = string.IsNullOrWhiteSpace(expression);
			if (!isClearingExpression && !IsValidExpression(expression)) return;

			this.expression = isClearingExpression ? string.Empty : expression;
			if (isImmediate)
			{
				SetExpressionImmediate();
				expressionStatus = TransitionStatus.Completed;
			}
			else
			{
				float defaultSpeed = vnOptions.Characters.TransitionSpeed;
				bool isSkipped = expressionStatus != TransitionStatus.Completed;
				expressionSpeed = GetTransitionSpeed(speed, defaultSpeed, ExpressionSpeedMultiplier, isSkipped);
				expressionStatus = expressionStatus == TransitionStatus.Completed ? TransitionStatus.Started : TransitionStatus.Skipped;
			}
		}

		public void SkipExpressionTransition()
		{
			if (expressionStatus == TransitionStatus.Completed) return;

			float defaultSpeed = vnOptions.Characters.TransitionSpeed;
			expressionSpeed = GetTransitionSpeed(defaultSpeed, defaultSpeed, ExpressionSpeedMultiplier, true);
			expressionStatus = TransitionStatus.Skipped;
		}

		protected override void SetColorImmediate()
		{
			Color displayColor = GetDisplayColor();
			primaryRawImage.color = displayColor;
			secondaryRawImage.color = displayColor;
		}

		void SetExpressionImmediate()
		{
			expressionBank.Expressions.TryGetValue(expression, out List<SubExpression> subExpressions);
			subExpressions ??= new List<SubExpression>();

			int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
			for (int i = 0; i < blendShapeCount; i++)
			{
				float weight = 0f;

				foreach (SubExpression subExpression in subExpressions)
				{
					if (i == subExpression.Index)
					{
						weight = subExpression.Weight;
						break;
					}
				}

				skinnedMeshRenderer.SetBlendShapeWeight(i, weight);
			}
		}

		protected override void TransitionColor()
		{
			if (colorStatus == TransitionStatus.Completed) return;

			Color displayColor = GetDisplayColor();
			float speed = colorSpeed * Time.deltaTime;
			primaryRawImage.color = Vector4.MoveTowards(primaryRawImage.color, displayColor, speed);
			secondaryRawImage.color = Vector4.MoveTowards(secondaryRawImage.color, displayColor, speed);

			if (Utilities.AreApproximatelyEqual(primaryRawImage.color, displayColor))
			{
				primaryRawImage.color = displayColor;
				secondaryRawImage.color = displayColor;
				colorStatus = TransitionStatus.Completed;
			}
		}

		void TransitionExpression()
		{
			if (expressionStatus == TransitionStatus.Completed) return;

			float speed = expressionSpeed * Time.deltaTime;

			expressionBank.Expressions.TryGetValue(expression, out List<SubExpression> subExpressions);
			subExpressions ??= new List<SubExpression>();

			bool isExpressionSet = true;

			int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
			for (int i = 0; i < blendShapeCount; i++)
			{
				float targetWeight = 0f;

				foreach (SubExpression subExpression in subExpressions)
				{
					if (i == subExpression.Index)
					{
						targetWeight = subExpression.Weight;
						break;
					}
				}

				float currentWeight = skinnedMeshRenderer.GetBlendShapeWeight(i);
				if (Utilities.AreApproximatelyEqual(currentWeight, targetWeight)) continue;

				float newWeight = Mathf.MoveTowards(currentWeight, targetWeight, speed);
				if (Utilities.AreApproximatelyEqual(targetWeight, newWeight))
				{
					skinnedMeshRenderer.SetBlendShapeWeight(i, targetWeight);
				}
				else
				{
					skinnedMeshRenderer.SetBlendShapeWeight(i, newWeight);
					isExpressionSet = false;
				}
			}

			if (isExpressionSet)
				expressionStatus = TransitionStatus.Completed;
		}

		bool IsValidExpression(string expressionName)
		{
			if (expressionName == null || !expressionBank.Expressions.ContainsKey(expressionName))
			{
				Debug.LogWarning($"No expression '{expressionName}' found for 3D Model: '{data.CastName}'");
				return false;
			}

			return true;
		}

		void CacheMotionNames()
		{
			foreach (AnimatorControllerParameter parameter in modelAnimator.parameters)
			{
				motionNames[parameter.name] = parameter.name;
			}
		}
	}
}
