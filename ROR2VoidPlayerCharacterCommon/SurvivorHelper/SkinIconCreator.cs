using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Xan.ROR2VoidPlayerCharacterCommon.SurvivorHelper {
	/// <summary>
	/// An improvement of R2API's IconTexJob that properly handles the line color.
	/// </summary>
	internal struct IconTexJobImproved : IJobParallelFor {
		[ReadOnly]
		public Color32 top;

		[ReadOnly]
		public Color32 right;

		[ReadOnly]
		public Color32 bottom;

		[ReadOnly]
		public Color32 left;

		private static readonly Color32 _lineFade = new Color32(127, 127, 127, 255);

		public NativeArray<Color32> texOutput;

		public void Execute(int index) {
			int num = index % 128 - 64;
			int num2 = index / 128 - 64;
			if (num2 > num && num2 > -num) {
				texOutput[index] = top;
			} else if (num2 < num && num2 < -num) {
				texOutput[index] = bottom;
			} else if (num2 > num && num2 < -num) {
				texOutput[index] = left;
			} else if (num2 < num && num2 > -num) {
				texOutput[index] = right;
			}
			if (Math.Abs(Math.Abs(num2) - Math.Abs(num)) <= 6) {
				texOutput[index] = Color32.Lerp(texOutput[index], _lineFade, 0.25f);
			}
		}
	}

	/// <summary>
	/// This class mimics the R2API technique of creating default skin icons.
	/// </summary>
	internal static class SkinIconCreator {
		
		/// <summary>
		/// Programmatically create a four-color skin icon for a survivor.;
		/// </summary>
		/// <param name="top"></param>
		/// <param name="right"></param>
		/// <param name="bottom"></param>
		/// <param name="left"></param>
		/// <returns></returns>
		public static Sprite CreateSkinIcon(Color32 top, Color32 right, Color32 bottom, Color32 left) {
			Texture2D texture2D = new Texture2D(128, 128, TextureFormat.RGBA32, mipChain: false);
			IconTexJobImproved jobData = default;
			jobData.top = top;
			jobData.bottom = bottom;
			jobData.right = right;
			jobData.left = left;
			jobData.texOutput = texture2D.GetRawTextureData<Color32>();
			jobData.Schedule(16384, 1).Complete();
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.Apply();
			return Sprite.Create(texture2D, new Rect(0f, 0f, 128f, 128f), new Vector2(0.5f, 0.5f));
		}

	}

}
