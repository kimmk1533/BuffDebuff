using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using UnityEngine;

namespace SpreadSheet
{
	public static class DataUtil
	{
		public static readonly string jsonFolder = Path.Combine("Data Files", "Json");
		public static readonly string jsonPath = Path.Combine(Application.dataPath, "Resources", jsonFolder);

		public static void LoadJsonData(DataSet dataset)
		{
			DirectoryInfo info = new DirectoryInfo(jsonPath);
			foreach (FileInfo file in info.GetFiles())
			{
				if (file.Name.EndsWith(".json") == false)
					continue;

				//로컬 경로에서 json 가져와서 DataTable으로 변환
				DataTable table = DataUtil.GetDataTable(file);
				dataset.Tables.Add(table);
			}
		}
		public static DataSet LoadJsonData()
		{
			DataSet dataSet = new DataSet();

			LoadJsonData(dataSet);

			return dataSet;
		}
		public static DataTable GetDataTable(string fileName, string tableName)
		{
			string filePath = Path.Combine(jsonFolder, fileName);
			TextAsset text = Resources.Load<TextAsset>(filePath);

			DataTable data = JsonConvert.DeserializeObject<DataTable>(text.ToString());
			data.TableName = tableName;

			return data;
		}
		public static DataTable GetDataTable(FileInfo info)
		{
			string fileName = Path.GetFileNameWithoutExtension(info.Name);
			string filePath = Path.Combine(jsonFolder, fileName);
			TextAsset text = Resources.Load<TextAsset>(filePath);

			DataTable data = JsonConvert.DeserializeObject<DataTable>(text.ToString());
			data.TableName = fileName;

			return data;
		}
		public static string GetDataValue(DataSet dataSet, string tableName, string primary, string value, string column)
		{
			DataRow[] rows = dataSet.Tables[tableName].Select(string.Concat(primary, " = '", value, "'"));

			return rows[0][column].ToString();
		}
		public static string GetDataValue(DataTable dataTable, string primary, string value, string column)
		{
			DataRow[] rows = dataTable.Select(string.Concat(primary, " = '", value, "'"));

			return rows[0][column].ToString();
		}
		public static void SetObjectFile<T>(string key, T data)
		{
			if (Directory.Exists(jsonPath) == false)
				Directory.CreateDirectory(jsonPath);

			string filePath = Path.Combine(jsonPath, key + ".json");
			string value = JsonConvert.SerializeObject(data, Formatting.Indented);

			File.WriteAllText(filePath, value);
		}
	}
}