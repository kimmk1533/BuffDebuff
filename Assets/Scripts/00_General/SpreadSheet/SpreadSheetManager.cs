using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using UnityEngine;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using System.Threading;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

// https://velog.io/@eqeq109/%EA%B5%AC%EA%B8%80-%EC%8A%A4%ED%94%84%EB%A0%88%EB%93%9C-%EC%8B%9C%ED%8A%B8-API%EB%A5%BC-%EC%9D%B4%EC%9A%A9%ED%95%B4-%EC%9C%A0%EB%8B%88%ED%8B%B0-%EB%8D%B0%EC%9D%B4%ED%84%B0-%ED%85%8C%EC%9D%B4%EB%B8%94-%EA%B4%80%EB%A6%AC-%EB%A7%A4%EB%8B%88%EC%A0%80-%EB%A7%8C%EB%93%A4%EA%B8%B0-2-%EA%B5%AC%ED%98%84%ED%8E%B8
namespace SpreadSheet
{
	public class SpreadSheetManager<T> : SerializedSingleton<T> where T : SpreadSheetManager<T>
	{
		[InlineEditor]
		[SerializeField, RuntimeReadOnly(true)]
		protected SpreadSheetSetting m_Setting;

		protected DataSet m_DataBase;

		[Button("Load Sheet")]
		public virtual void LoadSpreadSheetData()
		{
			m_DataBase = new DataSet("DataBase");

			MakeSheetDataset(m_DataBase);
		}

		protected void MakeSheetDataset(DataSet dataset)
		{
			ClientSecrets pass = new ClientSecrets();
			pass.ClientId = m_Setting.clientId;
			pass.ClientSecret = m_Setting.clientSecret;

			string[] scopes = new string[] { SheetsService.Scope.SpreadsheetsReadonly };
			UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(pass, scopes, m_Setting.clientName, CancellationToken.None).Result;

			SheetsService service = new SheetsService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
			});

			Spreadsheet request = service.Spreadsheets.Get(m_Setting.spreadSheetId).Execute();

			for (int i = 0; i < m_Setting.workSheetDataList.Count; ++i)
			{
				WorkSheetData item = m_Setting.workSheetDataList[i];

				if (item.enabled == false)
					continue;

				if (string.IsNullOrEmpty(item.fileName) == true)
					item.fileName = item.sheetName;

				#region 시트 이름 확인
				WorkSheetData sheetData = null;

				for (int j = 0; j < request.Sheets.Count; ++j)
				{
					Sheet sheet = request.Sheets[j];

					if (sheet.Properties.Title.Equals(item.sheetName))
					{
						sheetData = item;
						break;
					}
				}

				if (sheetData == null)
					continue;
				#endregion

				DataTable table = SendRequest(service, sheetData);
				dataset.Tables.Add(table);
			}
		}

		protected DataTable SendRequest(SheetsService service, WorkSheetData sheetData)
		{
			DataTable result = null;
			bool success = true;
			string fileName = sheetData.fileName;
			string sheetName = sheetData.sheetName;

			try
			{
				var request = service.Spreadsheets.Values.Get(m_Setting.spreadSheetId, sheetData.ToRange());
				// API 호출로 받아온 IList 데이터
				var jsonObject = request.Execute().Values;
				// IList 데이터를 jsonConvert 하기위해 직렬화
				string jsonString = ParseSheetData(jsonObject, sheetData.offsetCell);

				// DataTable로 변환
				result = SpreadSheetToDataTable(jsonString);
			}
			catch (Exception e)
			{
				success = false;
				Debug.LogError(e);
				// 예외 발생시 로컬 경로에 있는 json 파일을 통해 데이터 가져옴
				result = DataUtil.GetDataTable(fileName, sheetName);
				Debug.Log("시트 로드 실패로 로컬 " + sheetName + " json 데이터 불러옴");
			}

			Debug.Log("\"" + sheetName + "\" 시트 로드 " + (success ? "성공" : "실패"));

			result.TableName = sheetName;

			if (result != null)
			{
				// 변환한 테이블을 json 파일로 저장
				SaveDataToFile(fileName, result);
			}

			return result;
		}
		protected DataTable SpreadSheetToDataTable(string json)
		{
			DataTable data = JsonConvert.DeserializeObject<DataTable>(json);
			return data;
		}
		protected string ParseSheetData(IList<IList<object>> value, WorkSheetData.Cell offset)
		{
			StringBuilder jsonBuilder = new StringBuilder();

			IList<object> columns = value[0];
			int offsetRow = offset.row;
			int offsetCol = char.IsWhiteSpace(offset.column) || offset.column.Equals('\0') ?
				0 :
				char.ToLower(offset.column) - 'a' + 1;

			jsonBuilder.Append("[");
			for (int row = offsetRow; row < value.Count; row++)
			{
				var data = value[row];

				jsonBuilder.Append("{");
				for (int col = offsetCol; col < data.Count; col++)
				{
					jsonBuilder.Append("\"" + columns[col].ToString() + "\"" + ":");
					jsonBuilder.Append("\"" + data[col].ToString() + "\"");
					jsonBuilder.Append(",");
				}
				jsonBuilder.Append("}");

				if (row != value.Count - 1)
					jsonBuilder.Append(",");
			}
			jsonBuilder.Append("]");

			return jsonBuilder.ToString();
		}
		protected void SaveDataToFile(string fileName, DataTable newTable)
		{
			string path = DataUtil.jsonPath;

			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);

			string file = fileName + ".json";

			// 로컬경로
			string JsonPath = Path.Combine(path, file);

			FileInfo info = new FileInfo(JsonPath);

			// 동일 파일 유무 체크
			if (info.Exists)
			{
				DataTable checkTable = DataUtil.GetDataTable(info);

				// 파일 내용 체크
				if (BinaryCheck<DataTable>(newTable, checkTable))
					return;
			}

			// json파일 저장
			DataUtil.SetObjectFile(fileName, newTable);
		}
		protected bool BinaryCheck<TData>(TData src, TData target)
		{
			// 두 대상을 바이너리로 변환해서 비교, 다르면 false 반환
			BinaryFormatter formatter1 = new BinaryFormatter();
			MemoryStream stream1 = new MemoryStream();
			formatter1.Serialize(stream1, src);

			BinaryFormatter formatter2 = new BinaryFormatter();
			MemoryStream stream2 = new MemoryStream();
			formatter2.Serialize(stream2, target);

			byte[] srcByte = stream1.ToArray();
			byte[] tarByte = stream2.ToArray();

			if (srcByte.Length != tarByte.Length)
			{
				return false;
			}
			for (int i = 0; i < srcByte.Length; i++)
			{
				if (srcByte[i] != tarByte[i])
					return false;
			}
			return true;
		}
	}
}