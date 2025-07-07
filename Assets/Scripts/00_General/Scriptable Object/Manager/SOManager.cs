using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BuffDebuff
{
	public abstract class SOManager<T> : SerializedSingleton<T> where T : SOManager<T>
	{
		protected static string dataFolder;
		protected static readonly string defaultResourcesPath = Path.Combine("Data Files", "Scriptable Objects");
		protected static readonly string defaultDataPath = Path.Combine(Application.dataPath, "Resources", defaultResourcesPath);
		protected static readonly string defaultSavePath = Path.Combine("Assets", "Resources", defaultResourcesPath);

		public static string resourcesPath => Path.Combine(defaultResourcesPath, dataFolder);
		public static string dataPath => Path.Combine(defaultDataPath, dataFolder);
		public static string savePath => Path.Combine(defaultSavePath, dataFolder);

		// 스크립터블 오브젝트 생성
		protected virtual void CreateScriptableObject(SOData data, string fileName, bool deleteExistingFile = false)
		{
			// Data 폴더 생성
			if (Directory.Exists(dataPath) == false)
				Directory.CreateDirectory(dataPath);

			string filePath = Path.Combine(SOManager<T>.savePath, fileName + ".asset");

			if (File.Exists(filePath) == true &&
				deleteExistingFile == true)
				File.Delete(filePath);

			AssetDatabase.CreateAsset(data, filePath);
		}

		/// <summary>
		/// [ContextMenu("Create SO")] 사용 필수
		/// </summary>
		public abstract void CreateSO();
		/// <summary>
		/// [ContextMenu("Create SO")] 사용 필수
		/// </summary>
		public abstract void DeleteSO();
	}
}