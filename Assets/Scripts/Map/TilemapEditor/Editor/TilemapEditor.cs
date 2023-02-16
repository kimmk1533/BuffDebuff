using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tilemap))]
public class TilemapEditor : Editor
{
	private Tilemap m_Tilemap;

	private void OnEnable()
	{
		m_Tilemap = target as Tilemap;

		m_Tilemap.Init();

		SceneView.duringSceneGui -= ResetPos;
		SceneView.duringSceneGui += ResetPos;
	}
	private void OnDisable()
	{
		SceneView.duringSceneGui -= ResetPos;
	}

	private void ResetPos(SceneView sceneView)
	{
		m_Tilemap.transform.position = Vector3.zero;
	}
}