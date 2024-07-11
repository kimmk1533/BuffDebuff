using System.Collections;
using System.Collections.Generic;
using BuffDebuff.Enum;
using SpreadSheet;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Data;

namespace BuffDebuff
{
	public class BuffFileManager : Singleton<BuffFileManager>
	{
		#region 매니저
		private static BuffSheetManager M_BuffSheet => BuffSheetManager.Instance;
		#endregion

		public void Initialize()
		{

		}
		public void Finallize()
		{

		}

#if UNITY_EDITOR

		public void CreateAllBuff(bool load, /*bool script, */bool asset, bool switchCase)
		{
			if (Application.isEditor == false ||
				Application.isPlaying == true)
				return;

			if (load == false &&
				//script == false &&
				asset == false &&
				switchCase == false)
				return;

			if (load)
			{
				M_BuffSheet.Initialize();
			}

			DataSet dataSet = new DataSet();
			M_BuffSheet.LoadJsonData(dataSet);

			StringBuilder sb = null;

			if (switchCase)
			{
				sb = new StringBuilder();

				sb.AppendLine("\t\tswitch (buffData.title)");
				sb.AppendLine("\t\t{");
			}

			for (E_BuffType buffType = E_BuffType.Buff; buffType < E_BuffType.Max; ++buffType)
			{
				if (switchCase)
				{
					sb.Append("\t\t\t#region ");
					sb.AppendLine(BuffEnumUtil.ToKorString<E_BuffType>(buffType));
				}

				string sheetName = string.Concat(buffType.ToString(), "Data");
				DataTable dataTable = dataSet.Tables[sheetName];

				if (dataTable == null)
					continue;

				DataRow[] rows = dataTable.Select();

				for (int i = 0; i < rows.Length; ++i)
				{
					DataRow row = rows[i];

					#region 명칭
					// 명칭 불러오기
					string title = row[1] as string;
					#endregion
					#region 코드
					// 코드 불러오기
					string codeStr = row[0] as string;

					// 자료형 파싱
					if (int.TryParse(codeStr, out int code) == false)
					{
						Debug.LogError(title + " 버프 코드 불러오기 오류! | 코드: " + codeStr);
						return;
					}
					#endregion
					#region 효과 종류
					// 효과 종류 불러오기
					string effectTypeStr = row[2] as string;

					// 한글 -> 영어 전환
					switch (effectTypeStr)
					{
						case "스탯형":
							effectTypeStr = "Stat";
							break;
						case "무기형":
							effectTypeStr = "Weapon";
							break;
						case "전투형":
							effectTypeStr = "Combat";
							break;
						default:
							Debug.LogError(title + " 버프 효과 종류 불러오기 오류! | 버프 효과 종류: " + effectTypeStr);
							return;
					}

					// 자료형 파싱
					if (System.Enum.TryParse(effectTypeStr, out E_BuffEffectType effectType) == false)
					{
						Debug.LogError(title + " 버프 효과 종류 전환 오류! | 버프 효과 종류: " + effectTypeStr);
						return;
					}
					#endregion
					#region 등급
					// 등급 불러오기
					string gradeStr = row[3] as string;

					// 자료형 파싱
					if (System.Enum.TryParse(gradeStr, out E_BuffGrade grade) == false)
					{
						Debug.LogError(title + " 버프 등급 전환 오류! | 버프 등급: " + gradeStr);
						return;
					}
					#endregion
					#region 최대 스택
					// 최대 스택 불러오기
					string maxStackStr = row[4] as string;

					// 자료형 파싱
					if (int.TryParse(maxStackStr, out int maxStack) == false)
					{
						Debug.LogError(title + " 버프 최대 스택 전환 오류! | 버프 최대 스택: " + maxStackStr);
						return;
					}
					#endregion
					#region 적용 무기
					// 적용되는 무기 불러오기
					string weaponStr = row[5] as string;

					// 한글 -> 영어 전환
					switch (weaponStr)
					{
						case "공통":
							weaponStr = "All";
							break;
						case "근거리 무기":
							weaponStr = "Melee";
							break;
						case "원거리 무기":
							weaponStr = "Ranged";
							break;
						default:
							Debug.LogError(title + " 버프 적용 무기 타입 전환 오류! | 버프 적용 무기: " + weaponStr);
							return;
					}

					// 자료형 파싱
					if (System.Enum.TryParse(weaponStr, out E_BuffWeapon weapon) == false)
					{
						Debug.LogError(title + " 버프 적용 무기 전환 오류! | 버프 적용 무기: " + weaponStr);
						return;
					}
					#endregion
					#region 발동 조건
					// 발동 조건 불러오기
					string conditionStr = row[6] as string;

					// 한글 -> 영어 전환
					switch (conditionStr)
					{
						case "버프를 얻을 때":
							conditionStr = "Added";
							break;
						case "버프를 잃을 때":
							conditionStr = "Removed";
							break;
						case "매 프레임마다":
							conditionStr = "Update";
							break;
						case "일정 시간마다":
							conditionStr = "Timer";
							break;
						case "점프 시":
							conditionStr = "Jump";
							break;
						case "대쉬 시":
							conditionStr = "Dash";
							break;
						case "타격 시":
							conditionStr = "GiveDamage";
							break;
						case "피격 시":
							conditionStr = "TakeDamage";
							break;
						case "공격 시작 시":
							conditionStr = "AttackStart";
							break;
						case "공격 시":
							conditionStr = "Attack";
							break;
						case "공격 종료 시":
							conditionStr = "AttackEnd";
							break;
						case "적 처치 시":
							conditionStr = "KillEnemy";
							break;
						case "사망 시":
							conditionStr = "Death";
							break;
						case "스테이지를 넘어갈 시":
							conditionStr = "NextStage";
							break;
						default:
							Debug.LogError(title + " 버프 발동 조건 불러오기 오류! | 발동 조건 종류: " + conditionStr);
							return;
					}

					// 자료형 파싱
					if (System.Enum.TryParse(conditionStr, out E_BuffInvokeCondition condition) == false)
					{
						Debug.LogError(title + " 버프 발동 조건 전환 오류! | 버프 등급: " + conditionStr);
						return;
					}
					#endregion
					#region 버프 값
					// 버프 값 불러오기
					string buffValueStr = row[7] as string;

					// 자료형 파싱
					if (float.TryParse(buffValueStr, out float buffValue) == false &&
						buffValueStr != "-")
					{
						Debug.LogError(title + " 버프 값 전환 오류! | 버프 값: " + buffValueStr);
						return;
					}
					#endregion
					#region 버프 시간
					// 버프 시간 불러오기
					string buffTimeStr = row[8] as string;

					// 자료형 파싱
					if (float.TryParse(buffTimeStr, out float buffTime) == false &&
						buffTimeStr != "-")
					{
						Debug.LogError(title + " 버프 시간 전환 오류! | 버프 시간: " + buffTimeStr);
						return;
					}
					#endregion
					#region 설명
					string description = row[9] as string;
					#endregion
					#region 이미지

					#endregion

					BuffData buffData = ScriptableObject.CreateInstance<BuffData>();
					buffData.Initialize(title, code, buffType, effectType, grade, maxStack, weapon, condition, buffValue, buffTime, description, null);

					if (asset)
						CreateBuffScriptableObject(buffData);
					if (switchCase)
						AppendBuffCase(sb, buffData.title);
				}

				if (switchCase)
				{
					sb.AppendLine("\t\t\t#endregion");
				}
			}

			if (switchCase)
			{
				sb.AppendLine("\t\t}");

				CreateBuffCase(sb);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			Debug.Log("Buff Data Generation Completed!");
		}

		// 스크립트 생성
		//private void CreateBuffScript(BuffData buffData)
		//{
		//	string path = Path.Combine(Application.dataPath, "DataBase", "Buff Script", buffData.buffType.ToString());

		//	if (Directory.Exists(path) == false)
		//		Directory.CreateDirectory(path);

		//	string file = Path.Combine(path, buffData.title + ".cs");
		//	string template = Path.Combine(Application.dataPath, "DataBase", "Buff Script", "Template", "BuffScriptTemplate.txt");
		//	string className = buffData.title.Replace(' ', '_');
		//	string conditionInterface = "IOnBuff" + buffData.buffInvokeCondition.ToString();
		//	string conditionFormat = @"
		//public void OnBuff{0}<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
		//{

		//}";
		//	string condition = conditionFormat.Replace("{0}", buffData.buffInvokeCondition.ToString());

		//	switch (buffData.buffInvokeCondition)
		//	{
		//		case E_BuffInvokeCondition.Added:
		//			conditionInterface += ", IOnBuffRemoved";
		//			condition += conditionFormat.Replace("{0}", "Removed");
		//			break;
		//		case E_BuffInvokeCondition.Removed:
		//			conditionInterface = "IOnBuffAdded, " + conditionInterface;
		//			condition = conditionFormat.Replace("{0}", "Added") + condition;
		//			break;

		//		case E_BuffInvokeCondition.GiveDamage:
		//			conditionInterface += ", IOnBuffTakeDamage";
		//			condition += conditionFormat.Replace("{0}", "TakeDamage");
		//			break;
		//		case E_BuffInvokeCondition.TakeDamage:
		//			conditionInterface = "IOnBuffGiveDamage, " + conditionInterface;
		//			condition = conditionFormat.Replace("{0}", "GiveDamage") + condition;
		//			break;

		//		case E_BuffInvokeCondition.AttackStart:
		//			conditionInterface += ", IOnBuffAttack";
		//			conditionInterface += ", IOnBuffAttackEnd";
		//			condition += conditionFormat.Replace("{0}", "Attack");
		//			condition += conditionFormat.Replace("{0}", "AttackEnd");
		//			break;
		//		case E_BuffInvokeCondition.Attack:
		//			conditionInterface = "IOnBuffAttackStart, " + conditionInterface;
		//			conditionInterface += ", IOnBuffAttackEnd";
		//			condition = conditionFormat.Replace("{0}", "AttackStart") + condition;
		//			condition += conditionFormat.Replace("{0}", "AttackEnd");
		//			break;
		//		case E_BuffInvokeCondition.AttackEnd:
		//			conditionInterface = "IOnBuffAttackStart, IOnBuffAttack, " + conditionInterface;
		//			condition = conditionFormat.Replace("{0}", "AttackStart") + conditionFormat.Replace("{0}", "Attack") + condition;
		//			break;
		//	}

		//	StringBuilder sb = new StringBuilder(File.ReadAllText(template));

		//	sb.Replace("$Title", className);
		//	sb.Replace("$ConditionInterface", conditionInterface);
		//	sb.Replace("$Condition", condition);
		//	sb.Replace("$Description", buffData.description);

		//	if (File.Exists(file) == true)
		//	{
		//		const string start = "\t{";
		//		const string end = "\t}";

		//		string templateCode = sb.ToString();
		//		sb.Clear();

		//		string[] newFileLines = templateCode.Split("\r\n");
		//		string[] oldFileLines = File.ReadAllLines(file);

		//		#region 템플릿 파일 함수 저장
		//		List<string> newFileFuncOrderList = new List<string>();
		//		Dictionary<string, string> newFileFuncMap = new Dictionary<string, string>();
		//		for (int i = 0; i < newFileLines.Length; ++i)
		//		{
		//			if (newFileLines[i] != start)
		//				continue;

		//			string funcName = newFileLines[i - 1];

		//			sb.AppendLine(newFileLines[i - 1]);
		//			for (int j = i; j < newFileLines.Length; ++j)
		//			{
		//				sb.AppendLine(newFileLines[j]);

		//				if (newFileLines[j] == end)
		//				{
		//					newFileFuncOrderList.Add(funcName);
		//					newFileFuncMap.Add(funcName, sb.ToString());
		//					sb.Clear();
		//					break;
		//				}
		//			}
		//		}
		//		#endregion

		//		#region 기존 파일 함수 저장
		//		Dictionary<string, string> oldFileFuncMap = new Dictionary<string, string>();
		//		for (int i = 0; i < oldFileLines.Length; ++i)
		//		{
		//			if (oldFileLines[i] != start)
		//				continue;

		//			string funcName = oldFileLines[i - 1];

		//			sb.AppendLine(oldFileLines[i - 1]);
		//			for (int j = i; j < oldFileLines.Length; ++j)
		//			{
		//				sb.AppendLine(oldFileLines[j]);

		//				if (oldFileLines[j] == end)
		//				{
		//					oldFileFuncMap.Add(funcName, sb.ToString());
		//					sb.Clear();
		//					break;
		//				}
		//			}
		//		}
		//		#endregion

		//		sb.Clear();

		//		foreach (var item in oldFileFuncMap)
		//		{
		//			if (newFileFuncMap.ContainsKey(item.Key))
		//			{
		//				newFileFuncMap[item.Key] = item.Value;
		//			}
		//		}

		//		List<string> deletedFuncNameList = new List<string>();
		//		for (int i = 0; i < oldFileLines.Length; ++i)
		//		{
		//			string oldfuncName = oldFileLines[i];

		//			if (deletedFuncNameList.Contains(oldfuncName))
		//			{
		//				int index = oldFileFuncMap[oldfuncName].Split("\r\n").Length - 1;
		//				i += index - 1;
		//				continue;
		//			}

		//			if (newFileFuncMap.ContainsKey(oldfuncName))
		//			{
		//				int index = newFileFuncOrderList.IndexOf(oldfuncName);

		//				for (int j = 0; j < index; ++j)
		//				{
		//					string newFuncName = newFileFuncOrderList[0];

		//					string[] funcLines = newFileFuncMap[newFuncName].Split("\r\n");

		//					for (int k = 0; k < funcLines.Length - 1; ++k)
		//					{
		//						sb.AppendLine(funcLines[k]);
		//					}

		//					newFileFuncOrderList.RemoveAt(0);
		//					newFileFuncMap.Remove(newFuncName);
		//					deletedFuncNameList.Add(newFuncName);
		//				}

		//				newFileFuncMap.Remove(oldfuncName);
		//				newFileFuncOrderList.Remove(oldfuncName);
		//			}

		//			if (i == oldFileLines.Length - 1)
		//				sb.Append(oldFileLines[i]);
		//			else
		//				sb.AppendLine(oldFileLines[i]);
		//		}
		//	}

		//	File.WriteAllText(file, sb.ToString());
		//}

		// 스크립터블 오브젝트 생성
		private void CreateBuffScriptableObject(BuffData buffData)
		{
			// BuffData 폴더 생성
			string buffDataPath = Path.Combine(Application.dataPath, "Resources", "BuffData");
			if (Directory.Exists(buffDataPath) == false)
				Directory.CreateDirectory(buffDataPath);

			// Buff/Debuff 폴더 생성
			string path = Path.Combine(buffDataPath, buffData.buffType.ToString());
			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);

			string file = Path.Combine("Assets", "Resources", "BuffData", buffData.buffType.ToString(), buffData.title + ".asset");

			AssetDatabase.CreateAsset(buffData, file);
		}
		private void CreateBuffCase(StringBuilder sb, [System.Runtime.CompilerServices.CallerFilePath] string path = "")
		{
			string code = File.ReadAllText(path.Replace("BuffFileManager", "BuffManager"));

			var str = code.Split("\t\t// $BuffFunc");

			string allCase = sb.ToString();
			sb.Clear();

			sb.Append(str[0]);
			sb.AppendLine("\t\t// $BuffFunc");
			sb.Append(allCase);
			sb.Append("\t\t// $BuffFunc");
			sb.Append(str[2]);

			File.WriteAllText(path, sb.ToString());
		}
		private void AppendBuffCase(StringBuilder sb, string title)
		{
			string className = title.Replace(' ', '_');

			sb.Append("\t\t\tcase \"");
			sb.Append(title);
			sb.AppendLine("\":");
			sb.Append("\t\t\t\treturn new ");
			sb.Append(className);
			sb.AppendLine("(buffData);");
		}

#endif
	}
}