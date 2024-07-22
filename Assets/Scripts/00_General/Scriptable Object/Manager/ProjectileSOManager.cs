using SpreadSheet;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BuffDebuff
{
	public class ProjectileSOManager : SOManager<ProjectileSOManager>
	{
		static ProjectileSOManager()
		{
			dataFolder = "ProjectileData";
			dataPath = Path.Combine(Application.dataPath, "Resources", "Scriptable Object", dataFolder);
			savePath = Path.Combine("Assets", "Resources", "Scriptable Object", dataFolder);
		}

		[ContextMenu("Create SO")]
		public override void CreateSO()
		{
			if (Application.isEditor == false ||
				Application.isPlaying == true)
				return;

			DataTable dataTable = DataUtil.GetDataTable("ProjectileData", "투사체");

			if (dataTable == null)
			{
				Debug.LogError("올바른 Projectile 데이터 테이블이 존재하지 않습니다.");
				return;
			}

			DataRow[] rows = dataTable.Select();

			if (rows.Length < 1)
			{
				Debug.LogWarning("Projectile 테이블은 존재하나 Projectile 데이터 없음");
				return;
			}

			for (int i = 0; i < rows.Length; ++i)
			{
				DataRow row = rows[i];

				#region 에셋 경로
				string assetPathStr = row[0] as string;
				#endregion
				#region 이름
				string titleStr = row[2] as string;
				#endregion
				#region 코드
				string codeStr = row[1] as string;

				if (int.TryParse(codeStr, out int code) == false)
				{
					Debug.LogError(titleStr + "투사체 데이터 [코드] 전환 오류!\n" +
						"코드: " + codeStr);
					return;
				}
				#endregion
				#region 이동 속도
				string moveSpeedStr = row[3] as string;

				if (float.TryParse(moveSpeedStr, out float moveSpeed) == false)
				{
					Debug.LogError(titleStr + "투사체 데이터 [이동 속도] 전환 오류!\n" +
						"이동 속도: " + moveSpeedStr);
					return;
				}
				#endregion
				#region 생존 시간
				string lifeTimeStr = row[4] as string;

				if (float.TryParse(lifeTimeStr, out float lifeTime) == false)
				{
					Debug.LogError(titleStr + "투사체 데이터 [생존 시간] 전환 오류!\n" +
						"생존 시간: " + lifeTimeStr);
					return;
				}
				#endregion

				ProjectileData data = ScriptableObject.CreateInstance<ProjectileData>();
				data.Initialize(assetPathStr, code, titleStr, moveSpeed, lifeTime);

				CreateScriptableObject(data, titleStr);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			Debug.Log("Projectile 스크립터블 오브젝트 생성 완료");
		}
		[ContextMenu("Delete SO")]
		public override void DeleteSO()
		{
			throw new System.NotImplementedException();
		}
	}
}