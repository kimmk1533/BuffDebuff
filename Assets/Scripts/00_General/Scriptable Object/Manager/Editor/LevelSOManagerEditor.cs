using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuffDebuff
{
	[CustomEditor(typeof(LevelSOManager))]
	public class LevelSOManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			LevelSOManager levelSOManager = target as LevelSOManager;

			if (GUILayout.Button("Create Level\n" +
				"Scriptable Object"))
			{
				levelSOManager.CreateSO();
			}
		}
	}
}