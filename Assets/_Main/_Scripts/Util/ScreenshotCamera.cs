using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IO
{
	public class ScreenshotCamera : MonoBehaviour
	{
		[SerializeField] Camera screenshotCamera;

		const int slotWidth = 512;
		const int slotHeight = 288;

		public Texture2D CaptureForSlot() => Capture(slotWidth, slotHeight);

		Texture2D Capture(int width, int height)
		{
			RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24);

			screenshotCamera.targetTexture = renderTexture;
			RenderTexture.active = renderTexture;
			screenshotCamera.Render();

			Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
			screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			screenshot.Apply();

			RenderTexture.active = null;
			screenshotCamera.targetTexture = null;
			RenderTexture.ReleaseTemporary(renderTexture);

			return screenshot;
		}
	}
}
