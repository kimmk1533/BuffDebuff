using UnityEngine;
using System.Collections;
using GoogleSheetsToUnity;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using GoogleSheetsToUnity.ThirdPary;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class TestData : ScriptableObject
{
	public string associatedSheet = "";
	public string associatedWorksheet = "";

	internal void UpdateStats(List<GSTU_Cell> list)
	{
		Debug.Log($"명칭:{list[0].value}, 코드:{list[1].value} 등급:{list[2].value} 설명:{list[4].value}");
	}

}

[CustomEditor(typeof(TestData))]
public class DataEditor : Editor
{
	TestData data;

	void OnEnable()
	{
		data = (TestData)target;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUILayout.Label("Read Data Examples");

		if (GUILayout.Button("Pull Data Method One"))
		{
			UpdateStats(UpdateMethodOne);
		}
	}

	void UpdateStats(UnityAction<GstuSpreadSheet> callback, bool mergedCells = false)
	{
		SpreadsheetManager.Read(new GSTU_Search(data.associatedSheet, data.associatedWorksheet, "C1", "G44", "C", 1), callback, mergedCells);
	}

	void UpdateMethodOne(GstuSpreadSheet ss)
	{
		//data.UpdateStats(ss.rows["Jim"]);
		//foreach (string dataName in data.Names)
		//	data.UpdateStats(ss.rows[dataName], dataName);
		foreach (var item in ss.rows.primaryDictionary)
		{
			data.UpdateStats(item.Value);
		}
		EditorUtility.SetDirty(target);
	}

}