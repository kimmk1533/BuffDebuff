using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuffDebuff
{
	[CustomEditor(typeof(PlayerSOManager))]
	public class PlayerSOManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			PlayerSOManager playerSOManager = target as PlayerSOManager;

			if (GUILayout.Button("Create Player\n" +
				"Scriptable Object"))
			{
				playerSOManager.CreateSO();
			}
		}
	}
}