using System.Collections;
using System.Collections.Generic;
using SpreadSheet;
using System.Data;
using UnityEditor;
using UnityEngine;

namespace BuffDebuff
{
	public class LevelSOManager : SOManager<LevelSOManager>
	{
		static LevelSOManager()
		{
			dataFolder = "LevelData";
		}

		[ContextMenu("Create SO")]
		public override void CreateSO()
		{
			if (Application.isEditor == false ||
				Application.isPlaying == true)
				return;

			DataTable dataTable = DataUtil.GetDataTable("LevelData", "적");

			if (dataTable == null)
			{
				Debug.LogError("올바른 Level 데이터 테이블이 존재하지 않습니다.");
				return;
			}

			DataRow[] rows = dataTable.Select();

			if (rows.Length < 1)
			{
				Debug.LogWarning("Level 테이블은 존재하나 Level 데이터 없음");
				return;
			}

			for (int i = 0; i < rows.Length; ++i)
			{
				DataRow row = rows[i];

				#region 현재 레벨
				string currentLevelStr = row[0] as string;

				if (int.TryParse(currentLevelStr, out int currentLevel) == false)
				{
					Debug.LogError("레벨 데이터 [현재 레벨] 전환 오류!\n" +
						"현재 레벨: " + currentLevelStr);
					return;
				}
				#endregion
				#region 다음 레벨
				string nextLevelStr = row[1] as string;

				if (int.TryParse(nextLevelStr, out int nextLevel) == false)
				{
					Debug.LogError("레벨 데이터 [다음 레벨] 전환 오류!\n" +
						"다음 레벨: " + nextLevelStr);
					return;
				}
				#endregion
				#region 필요 경험치
				string requiredXpStr = row[2] as string;

				if (float.TryParse(requiredXpStr, out float requiredXp) == false)
				{
					Debug.LogError("레벨 데이터 [필요 경험치] 전환 오류!\n" +
						"필요 경험치: " + requiredXpStr);
					return;
				}
				#endregion

				LevelData data = ScriptableObject.CreateInstance<LevelData>();
				data.Initialize(currentLevel, nextLevel, requiredXp);

				CreateScriptableObject(data, "Level " + currentLevelStr, true);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			Debug.Log("Level 스크립터블 오브젝트 생성 완료");
		}
		[ContextMenu("Delete SO")]
		public override void DeleteSO()
		{
			throw new System.NotImplementedException();
		}
	}
}