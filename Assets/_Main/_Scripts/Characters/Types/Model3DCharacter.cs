using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
	public class Model3DCharacter : GraphicsCharacter
	{
		const int ModelHeightOffset = 15;

		Transform modelRoot;
		Camera modelCamera;
		Transform modelContainer;
		Animator modelAnimator;
		RawImage rawImage;

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
	}
}
