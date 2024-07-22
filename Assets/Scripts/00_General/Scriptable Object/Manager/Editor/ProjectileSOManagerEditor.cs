using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuffDebuff
{
	[CustomEditor(typeof(ProjectileSOManager))]
	public class ProjectileSOManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			ProjectileSOManager projectileSOManager = target as ProjectileSOManager;

			if (GUILayout.Button("Create Projectile\n" +
				"Scriptable Object"))
			{
				projectileSOManager.CreateSO();
			}
		}
	}
}