using SpreadSheet;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BuffDebuff
{
	public class PlayerSOManager : SOManager<PlayerSOManager>
	{
		static PlayerSOManager()
		{
			dataFolder = "PlayerData";
			dataPath = Path.Combine(Application.dataPath, "Resources", "Scriptable Object", dataFolder);
			savePath = Path.Combine("Assets", "Resources", "Scriptable Object", dataFolder);
		}

		[ContextMenu("Create SO")]
		public override void CreateSO()
		{
			if (Application.isEditor == false ||
				Application.isPlaying == true)
				return;

			DataTable dataTable = DataUtil.GetDataTable("PlayerData", "플레이어");

			if (dataTable == null)
			{
				Debug.LogError("올바른 Player 데이터 테이블이 존재하지 않습니다.");
				return;
			}

			DataRow[] rows = dataTable.Select();

			if (rows.Length < 1)
			{
				Debug.LogWarning("Player 테이블은 존재하나 Player 데이터 없음");
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
					Debug.LogError(titleStr + " 플레이어 데이터 [코드] 전환 오류!\n" +
						"코드: " + codeStr);
					return;
				}
				#endregion
				#region 체력
				string hpStr = row[3] as string;

				if (float.TryParse(hpStr, out float hp) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [체력] 전환 오류!\n" +
						"체력: " + hpStr);
					return;
				}
				#endregion
				#region 체력 회복량
				string hpRegenStr = row[4] as string;

				if (float.TryParse(hpRegenStr, out float hpRegen) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [체력 회복량] 전환 오류!\n" +
						"체력 회복량: " + hpRegenStr);
					return;
				}
				#endregion
				#region 체력 재생 쿨타임
				string hpRegenTimeStr = row[5] as string;

				if (float.TryParse(hpRegenTimeStr, out float hpRegenTime) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [체력 재생 쿨타임] 전환 오류!\n" +
						"체력 재생 쿨타임: " + hpRegenTimeStr);
					return;
				}
				#endregion
				#region 힐 배율
				string healScaleStr = row[6] as string;

				if (float.TryParse(healScaleStr, out float healScale) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [힐 배율] 전환 오류!\n" +
						"힐 배율: " + healScaleStr);
					return;
				}
				#endregion
				#region 치유 감소 배율
				string antiHealScaleStr = row[7] as string;

				if (float.TryParse(antiHealScaleStr, out float antiHealScale) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [치유 감소 배율] 전환 오류!\n" +
						"치유 감소 배율: " + antiHealScaleStr);
					return;
				}
				#endregion
				#region 방어력
				string armorStr = row[8] as string;

				if (float.TryParse(armorStr, out float armor) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [방어력] 전환 오류!\n" +
						"방어력: " + armorStr);
					return;
				}
				#endregion
				#region 공격력
				string attackPowerStr = row[9] as string;

				if (float.TryParse(attackPowerStr, out float attackPower) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [공격력] 전환 오류!\n" +
						"공격력: " + attackPowerStr);
					return;
				}
				#endregion
				#region 공격 속도
				string attackSpeedStr = row[10] as string;

				if (float.TryParse(attackSpeedStr, out float attackSpeed) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [공격 속도] 전환 오류!\n" +
						"공격 속도: " + attackSpeedStr);
					return;
				}
				#endregion
				#region 공격 크기
				string attackSizeStr = row[11] as string;

				if (float.TryParse(attackSizeStr, out float attackSize) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [공격 크기] 전환 오류!\n" +
						"공격 크기: " + attackSizeStr);
					return;
				}
				#endregion
				#region 투사체 속도
				string shotSpeedStr = row[12] as string;

				if (float.TryParse(shotSpeedStr, out float shotSpeed) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [투사체 속도] 전환 오류!\n" +
						"투사체 속도: " + shotSpeedStr);
					return;
				}
				#endregion
				#region 투사체 생존 시간
				string attackRangeStr = row[13] as string;

				if (float.TryParse(attackRangeStr, out float attackRange) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [투사체 생존 시간] 전환 오류!\n" +
						"투사체 생존 시간: " + attackRangeStr);
					return;
				}
				#endregion
				#region 타격 수
				string multiHitCountStr = row[14] as string;

				if (int.TryParse(multiHitCountStr, out int multiHitCount) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [타격 수] 전환 오류!\n" +
						"타격 수: " + multiHitCountStr);
					return;
				}
				#endregion
				#region 치명타 확률
				string criticalRateStr = row[15] as string;

				if (float.TryParse(criticalRateStr, out float criticalRate) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [치명타 확률] 전환 오류!\n" +
						"치명타 확률: " + criticalRateStr);
					return;
				}
				#endregion
				#region 치명타 대미지 배율
				string criticalDamageScaleStr = row[16] as string;

				if (float.TryParse(criticalDamageScaleStr, out float criticalDamageScale) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [치명타 대미지 배율] 전환 오류!\n" +
						"치명타 대미지 배율: " + criticalDamageScaleStr);
					return;
				}
				#endregion
				#region 회피율
				string avoidabilityStr = row[17] as string;

				if (float.TryParse(avoidabilityStr, out float avoidability) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [회피율] 전환 오류!\n" +
						"회피율: " + avoidabilityStr);
					return;
				}
				#endregion
				#region 이동 속도
				string moveSpeedStr = row[18] as string;

				if (float.TryParse(moveSpeedStr, out float moveSpeed) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [이동 속도] 전환 오류!\n" +
						"이동 속도: " + moveSpeedStr);
					return;
				}
				#endregion
				#region 시야 거리
				string sightStr = row[19] as string;

				if (float.TryParse(sightStr, out float sight) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [시야 거리] 전환 오류!\n" +
						"시야 거리: " + sightStr);
					return;
				}
				#endregion
				#region 획득 경험치 배율
				string xpScaleStr = row[20] as string;

				if (float.TryParse(xpScaleStr, out float xpScale) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [획득 경험치 배율] 전환 오류!\n" +
						"획득 경험치 배율: " + xpScaleStr);
					return;
				}
				#endregion
				#region 대쉬 속도
				string dashSpeedStr = row[21] as string;

				if (float.TryParse(dashSpeedStr, out float dashSpeed) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [대쉬 속도] 전환 오류!\n" +
						"대쉬 속도: " + dashSpeedStr);
					return;
				}
				#endregion
				#region 대쉬 횟수
				string dashCountStr = row[22] as string;

				if (int.TryParse(dashCountStr, out int dashCount) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [대쉬 횟수] 전환 오류!\n" +
						"대쉬 횟수: " + dashCountStr);
					return;
				}
				#endregion
				#region 대쉬 충전 속도
				string dashRechargeTimeStr = row[23] as string;

				if (float.TryParse(dashRechargeTimeStr, out float dashRechargeTime) == false)
				{
					Debug.LogError(titleStr + " 플레이어 데이터 [대쉬 충전 속도] 전환 오류!\n" +
						"대쉬 충전 속도: " + dashRechargeTimeStr);
					return;
				}
				#endregion

				PlayerData data = ScriptableObject.CreateInstance<PlayerData>();
				data.Initialize(assetPathStr, code, titleStr, hp, hpRegen, hpRegenTime, healScale, antiHealScale, armor, attackPower, attackSpeed, attackSize, shotSpeed, attackRange, multiHitCount, criticalRate, criticalDamageScale, avoidability, moveSpeed, sight, xpScale, dashSpeed, dashCount, dashRechargeTime);

				CreateScriptableObject(data, titleStr);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			Debug.Log("Player 스크립터블 오브젝트 생성 완료");
		}
		[ContextMenu("Delete SO")]
		public override void DeleteSO()
		{
			throw new System.NotImplementedException();
		}
	}
}