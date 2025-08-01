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
	public class BuffSOManager : SOManager<BuffSOManager>
	{
		[SerializeField]
		private bool m_SwitchCase;

		static BuffSOManager()
		{
			dataFolder = "BuffData";
		}

		[ContextMenu("Create SO")]
		public override void CreateSO()
		{
			DataSet dataSet = DataUtil.LoadJsonData();

			StringBuilder sb = null;

			if (m_SwitchCase)
			{
				sb = new StringBuilder();

				sb.AppendLine("\t\tswitch (buffData.title)");
				sb.AppendLine("\t\t{");
			}

			for (E_BuffType buffType = E_BuffType.Buff; buffType < E_BuffType.Max; ++buffType)
			{
				if (m_SwitchCase)
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

					#region 사용 여부
					string isUseStr = row[11] as string;
					if (bool.TryParse(isUseStr, out bool isUse) == false)
					{
						Debug.LogError(title + " 사용 여부 전환 오류! | 사용 여부: " + isUseStr);
						return;
					}

					if (isUse == false)
						continue;
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
					string effectTypeKorStr = row[2] as string;

					// 자료형 파싱
					if (BuffEnumUtil.TryParseKorStr(effectTypeKorStr, out E_BuffEffectType effectType) == false)
					{
						Debug.LogError(title + " 버프 효과 종류 전환 오류! | 버프 효과 종류: " + effectTypeKorStr);
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
					string weaponKorStr = row[5] as string;

					// 자료형 파싱
					if (BuffEnumUtil.TryParseKorStr(weaponKorStr, out E_BuffWeaponType weapon) == false)
					{
						Debug.LogError(title + " 버프 적용 무기 전환 오류! | 버프 적용 무기: " + weaponKorStr);
						return;
					}
					#endregion
					#region 발동 조건
					// 발동 조건 불러오기
					string conditionKorStr = row[6] as string;

					// 자료형 파싱
					if (BuffEnumUtil.TryParseKorStr(conditionKorStr, out E_BuffInvokeCondition condition) == false)
					{
						Debug.LogError(title + " 버프 발동 조건 전환 오류! | 버프 등급: " + conditionKorStr);
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
					#region 버프 값 적용 방식
					// 버프 값 적용 방식 불러오기
					string buffValueTypeKorStr = row[8] as string;

					// 자료형 파싱
					if (BuffEnumUtil.TryParseKorStr(buffValueTypeKorStr, out E_BuffValueType buffValueType) == false)
					{
						Debug.LogError(title + " 버프 값 적용 방식 전환 오류! | 버프 값 적용 방식: " + buffValueTypeKorStr);
						return;
					}
					#endregion
					#region 설명
					string description = row[9] as string;
					#endregion
					#region 이미지

					#endregion

					BuffData buffData = ScriptableObject.CreateInstance<BuffData>();
					buffData.Initialize(title, code, buffType, effectType, grade, maxStack, weapon, condition, buffValue, buffValueType, description, null);

					CreateScriptableObject(buffData, title);

					if (m_SwitchCase)
						AppendBuffCase(sb, buffData.title);
				}

				if (m_SwitchCase)
				{
					sb.AppendLine("\t\t\t#endregion");
				}
			}

			if (m_SwitchCase)
			{
				sb.AppendLine("\t\t}");

				CreateBuffCase(sb);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			Debug.Log("Buff Data Generation Completed!");
		}
		[ContextMenu("Delete SO")]
		public override void DeleteSO()
		{

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
		protected override void CreateScriptableObject(SOData data, string fileName, bool deleteExistingFile = false)
		{
			BuffData buffData = data as BuffData;

			// Data 폴더 생성
			if (Directory.Exists(dataPath) == false)
				Directory.CreateDirectory(dataPath);

			// Buff/Debuff 폴더 생성
			string directoryPath = Path.Combine(dataPath, buffData.buffType.ToString());
			if (Directory.Exists(directoryPath) == false)
				Directory.CreateDirectory(directoryPath);

			string filePath = Path.Combine(savePath, buffData.buffType.ToString(), fileName + ".asset");

			if (File.Exists(filePath) == true &&
				deleteExistingFile == true)
				File.Delete(filePath);

			AssetDatabase.CreateAsset(data, filePath);
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
	}
}