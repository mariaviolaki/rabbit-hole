using Gameplay;
using IO;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Visuals;

namespace Preprocessing
{
	public static class CGBankBuilder
	{
		[MenuItem("Tools/Build CG Bank")]
		public static void BuildCGBank()
		{
			ConvertAllToSprite(FilePaths.CGRoot);
			ConvertAllToSprite(FilePaths.CGThumbnailsRoot);

			// Find the CG Bank asset
			string[] assetGuids = AssetDatabase.FindAssets($"t:{FilePaths.CGBankName}");
			if (assetGuids.Length == 0)
			{
				Debug.LogWarning("No CG Bank asset found in project.");
				return;
			}

			string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
			CGBankSO cgBank = AssetDatabase.LoadAssetAtPath<CGBankSO>(assetPath);
			if (cgBank == null)
			{
				Debug.LogWarning("Failed to load CG Bank asset.");
				return;
			}

			// Clear the old character CGs
			Undo.RecordObject(cgBank, "Rebuild CG Bank");
			cgBank.cgs.Clear();

			// Look for all CGs recursively under the root folder
			foreach (string imagePath in Directory.GetFiles(FilePaths.CGRoot, $"*{FilePaths.CGFileExtension}", SearchOption.AllDirectories))
			{
				string imageName = Path.GetFileNameWithoutExtension(imagePath);
				string[] imageParts = imageName.Split(FilePaths.CGSeparator);

				// Expecting something like "Void.1.0" -> [ "Void", "1", "0" ]
				if (imageParts.Length < 3 ||
					!Enum.TryParse(imageParts[0], out CharacterRoute route) ||
					!int.TryParse(imageParts[1], out int num) ||
					!int.TryParse(imageParts[2], out int stage))
				{
					Debug.LogWarning($"Skipping CG '{imageName}', invalid format.");
					continue;
				}

				string routeName = route.ToString();
				string thumbnailPath = Path.Combine(FilePaths.CGThumbnailsRoot, routeName, $"{imageName}{FilePaths.CGFileExtension}");
				if (!File.Exists(thumbnailPath))
				{
					Debug.LogWarning($"Skipping CG '{imageName}' because of missing thumbnail in route '{routeName}'. Expected path: {thumbnailPath}");
					continue;
				}

				CharacterCG cg = new(route, num, stage);
				cgBank.cgs.Add(cg);
			}

			// Order all the CGs based on their route, number, and stage
			cgBank.cgs = cgBank.cgs.OrderBy(cg => cg.route).ThenBy(cg => cg.num).ThenBy(cg => cg.stage).ToList();

			// for each CG in a group (route + num) save how many stages there are
			var groupedCGs = cgBank.cgs.GroupBy(cg => (cg.route, cg.num));
			foreach (var group in groupedCGs)
			{
				int stageCount = group.Count();
				foreach (CharacterCG cg in group)
					cg.stageCount = stageCount;
			}

			EditorUtility.SetDirty(cgBank);
			AssetDatabase.SaveAssets();

			Debug.Log("<color=#32CD32>Successfully populated CG Bank!</color>");
		}

		static void ConvertAllToSprite(string folderPath)
		{
			foreach (string imagePath in Directory.GetFiles(folderPath, $"*{FilePaths.CGFileExtension}", SearchOption.AllDirectories))
			{
				string assetPath = imagePath.Replace(Application.dataPath, "Assets");
				TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
				if (importer != null && importer.textureType != TextureImporterType.Sprite)
				{
					importer.textureType = TextureImporterType.Sprite;
					importer.SaveAndReimport();
				}
			}
		}
	}
}
