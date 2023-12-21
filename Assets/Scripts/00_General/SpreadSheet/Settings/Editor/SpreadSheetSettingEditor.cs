using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpreadSheet
{
	[CustomEditor(typeof(SpreadSheetSetting))]
	public class SpreadSheetSettingEditor : Editor
	{
		SpreadSheetSetting setting => SpreadSheetSetting.Instance;

		public override void OnInspectorGUI()
		{
			GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
			headerStyle.fontSize = 12;
			headerStyle.fontStyle = FontStyle.Bold;

			GUILayout.Label("Google Spread Sheet Settings", headerStyle);

			EditorGUILayout.Separator();

			//const int LabelWidth = 90;

			//using (new GUILayout.HorizontalScope())
			//{
			//	//setting.useOAuth2JsonFile = GUILayout.Toggle(setting.useOAuth2JsonFile, " I have OAuth2 JSON file");

			//	// reset client_id and client_secret and empty its textfields.
			//	if (GUILayout.Button("Reset", GUILayout.Width(60)))
			//	{
			//		setting.clientName = string.Empty;
			//		setting.clientId = string.Empty;
			//		setting.clientSecret = string.Empty;

			//		setting.workSheetDataList.Clear();

			//		//// retrieves from google developer center.
			//		//setting.refreshToken = string.Empty;
			//		//setting.accessToken = string.Empty;
			//	}
			//}

			EditorGUILayout.Separator();

			//if (setting.clientName == null)
			//	setting.clientName = string.Empty;
			//if (setting.clientId == null)
			//	setting.clientId = string.Empty;
			//if (setting.clientSecret == null)
			//	setting.clientSecret = string.Empty;

			var clientName = serializedObject.FindProperty("clientName");
			var clientId = serializedObject.FindProperty("clientId");
			var clientSecret = serializedObject.FindProperty("clientSecret");
			var spreadSheetId = serializedObject.FindProperty(propertyPath: "spreadSheetId");
			var workSheetDataList = serializedObject.FindProperty("workSheetDataList");

			// OAuth2 clientName
			EditorGUILayout.PropertyField(clientName);

			// OAuth2 clientId
			EditorGUILayout.PropertyField(clientId);

			// OAuth2 clientSecret
			EditorGUILayout.PropertyField(clientSecret);

			EditorGUILayout.Separator();

			// SpreadSheetId
			EditorGUILayout.PropertyField(spreadSheetId);

			EditorGUILayout.Separator();

			EditorGUILayout.PropertyField(workSheetDataList);

			serializedObject.ApplyModifiedProperties();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(setting);
			}
		}
	}
}