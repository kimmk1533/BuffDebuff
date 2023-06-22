using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuffManager))]
public class BuffManagerEditor : Editor
{
	static bool m_LoadBuff;
	static bool m_CreateScript;
	static bool m_CreateAsset;
	static bool m_UpdateBuffManager;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		BuffManager buffManager = target as BuffManager;

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Options");

		m_LoadBuff = GUILayout.Toggle(m_LoadBuff, " Load Buff From Spread Sheet");
		//GUILayout.Space(5.0f);
		m_CreateScript = GUILayout.Toggle(m_CreateScript, " Create Script");
		//GUILayout.Space(5.0f);
		m_CreateAsset = GUILayout.Toggle(m_CreateAsset, " Create ScriptableObject");
		m_UpdateBuffManager = GUILayout.Toggle(m_UpdateBuffManager, " Update Buff Manager(switch case)");

		EditorGUILayout.Space();

		if (GUILayout.Button("Create Buff"))
		{
			buffManager.CreateAllBuff(m_LoadBuff, m_CreateScript, m_CreateAsset, m_UpdateBuffManager);
		}
	}
}