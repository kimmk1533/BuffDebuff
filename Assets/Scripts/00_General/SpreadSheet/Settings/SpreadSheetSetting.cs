using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpreadSheet
{
	[CreateAssetMenu(fileName = "new Setting", menuName = "Google Spread Sheets/Setting", order = int.MinValue)]
	public class SpreadSheetSetting : ScriptableObject
	{
		// OAuth2 클라이언트 이름
		public string clientName;
		// OAuth2 클라이언트 ID
		public string clientId;
		// OAuth2 클라이언트 보안 비밀번호
		public string clientSecret;

		// 스프레드시트 키(url)
		public string spreadSheetId;

		[RuntimeReadOnly(true)]
		public List<WorkSheetData> workSheetDataList;

		//// enter Auth 2.0 Refresh Token and AccessToken after succesfully authorizing with Access Code
		//public string refreshToken = "";
		//public string accessToken = "";

//#if UNITY_EDITOR
//		/// <summary>
//		/// Select currently exist account setting asset file.
//		/// </summary>
//		[MenuItem("Google Spread Sheet/Select Google Spread Sheet Setting")]
//		public static void Edit()
//		{
//			if (Instance == null)
//			{
//				Debug.LogError("No Google Spread Sheet Setting.asset file is found. Create setting file first.");
//				return;
//			}

//			Selection.activeObject = Instance;
//		}
//#endif
	}
}