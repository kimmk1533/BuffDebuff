using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEditor;

namespace BuffDebuff
{
	[CustomEditor(typeof(BuffGradeInfo))]
	public class BuffGradeInfoEditor : OdinEditor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			//GUIHelper.RequestRepaint();
			EditorUtility.SetDirty(target);
		}
	}
}