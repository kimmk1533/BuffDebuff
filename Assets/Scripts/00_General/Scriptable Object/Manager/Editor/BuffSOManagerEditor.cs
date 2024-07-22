using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuffDebuff
{
	[CustomEditor(typeof(BuffSOManager))]
	public class BuffSOManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			BuffSOManager buffSOManager = target as BuffSOManager;

			if (GUILayout.Button("Create Buff\n" +
				"Scriptable Object"))
			{
				buffSOManager.CreateSO();
			}
		}
	}
}