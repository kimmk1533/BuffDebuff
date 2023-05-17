using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(BuffManager))]
public class BuffManagerEditor : Editor
{
	bool m_LoadBuff;
	bool m_CreateScript;
	bool m_CreateScriptableObject;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		BuffManager buffManager = target as BuffManager;

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Options");
		m_LoadBuff = GUILayout.Toggle(m_LoadBuff, "Load Buff From Spread Sheet\t");
		//GUILayout.Space(5.0f);
		m_CreateScript = GUILayout.Toggle(m_CreateScript, "Create Script\t");
		//GUILayout.Space(5.0f);
		m_CreateScriptableObject = GUILayout.Toggle(m_CreateScriptableObject, "Create Scriptable Object\t");

		if (GUILayout.Button("Create Buff"))
		{
			if (m_LoadBuff)
				buffManager.LoadAllBuff();
			if (m_CreateScript)
				buffManager.CreateAllBuffScript();
			if (m_CreateScriptableObject)
				buffManager.CreateAllBuffScriptableObject();
		}
	}
}