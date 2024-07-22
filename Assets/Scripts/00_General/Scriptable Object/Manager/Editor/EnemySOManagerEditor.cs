using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuffDebuff
{
	[CustomEditor(typeof(EnemySOManager))]
	public class EnemySOManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EnemySOManager enemySOManager = target as EnemySOManager;

			if (GUILayout.Button("Create Enemy\n" +
				"Scriptable Object"))
			{
				enemySOManager.CreateSO();
			}
		}
	}
}