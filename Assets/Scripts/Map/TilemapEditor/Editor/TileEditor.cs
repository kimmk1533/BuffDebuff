using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tile))]
public class TileEditor : Editor
{
	private Tile tile;

	private void OnEnable()
	{
		tile = serializedObject.targetObject as Tile;
	}
	public override void OnInspectorGUI()
	{
		tile.m_Sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", tile.m_Sprite, typeof(Sprite), false);
	}
}