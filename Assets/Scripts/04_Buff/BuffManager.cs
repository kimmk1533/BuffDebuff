using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using SpreadSheet;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using BuffDictionary = DoubleKeyDictionary<int, string, BuffData>;
using BuffUIDictionary = DoubleKeyDictionary<int, string, BuffUIData>;

public sealed class BuffManager : Singleton<BuffManager>
{
	// < 코드, 명칭, 버프 데이터 >
	private Dictionary<E_BuffType, BuffDictionary> m_BuffDictionary;
	// < 코드, 명칭, 버프UI 데이터 >
	private Dictionary<E_BuffType, BuffUIDictionary> m_BuffUIDictionary;

	private void Awake()
	{
		Initialize();

		// 엑셀에서 읽어온 데이터로 딕셔너리에 버프 생성
		LoadAllBuff();
	}

	private void Initialize()
	{
		m_BuffDictionary = new Dictionary<E_BuffType, BuffDictionary>();

		for (E_BuffType i = 0; i != E_BuffType.Max; ++i)
		{
			m_BuffDictionary[i] = new BuffDictionary();
		}
	}

	[ContextMenu("LoadAllBuff")]
	public void LoadAllBuff()
	{
		if (Application.isPlaying == false)
			Initialize();

		DataSet dataSet = new DataSet();
		SpreadSheetManager.Instance.LoadJsonData(dataSet);

		for (E_BuffType buffType = E_BuffType.Buff; buffType != E_BuffType.Max - 1; ++buffType)
		{
			LoadBuff(dataSet, buffType);
		}
	}
	public void LoadBuff(DataSet dataSet, E_BuffType buffType)
	{
		string sheetName = string.Concat(BuffEnumUtil.EnumToKorString(buffType), " 목록");

		DataRow[] rows = dataSet.Tables[sheetName].Select();

		foreach (var row in rows)
		{
			#region 코드
			// 코드 불러오기
			string codeStr = row[0] as string;

			// 자료형 파싱
			if (int.TryParse(codeStr, out int code) == false)
			{
				Debug.LogError("버프 코드 불러오기 오류! | 코드: " + codeStr);
				return;
			}
			#endregion
			#region 명칭
			// 명칭 불러오기
			string title = row[1] as string;
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
					Debug.LogError("버프 효과 종류 불러오기 오류! | 버프 효과 종류: " + effectTypeStr);
					return;
			}

			// 자료형 파싱
			if (System.Enum.TryParse(effectTypeStr, out E_BuffEffectType effectType) == false)
			{
				Debug.LogError("버프 효과 종류 전환 오류! | 버프 효과 종류: " + effectTypeStr);
				return;
			}
			#endregion
			#region 등급
			// 등급 불러오기
			string gradeStr = row[3] as string;

			// 자료형 파싱
			if (System.Enum.TryParse(gradeStr, out E_BuffGrade grade) == false)
			{
				Debug.LogError("버프 등급 전환 오류! | 버프 등급: " + gradeStr);
				return;
			}
			#endregion
			#region 최대 스택
			// 최대 스택 불러오기
			string maxStackStr = row[4] as string;

			// 자료형 파싱
			if (int.TryParse(maxStackStr, out int maxStack) == false)
			{
				Debug.LogError("버프 최대 스택 전환 오류! | 버프 최대 스택: " + maxStackStr);
				return;
			}
			#endregion
			#region 적용 무기 타입
			// 적용되는 무기 타입 불러오기
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
					Debug.LogError("버프 적용 무기 타입 전환 오류! | 버프 적용 무기: " + weaponStr);
					return;
			}

			// 자료형 파싱
			if (System.Enum.TryParse(weaponStr, out E_BuffWeapon weapon) == false)
			{
				Debug.LogError("버프 적용 무기 타입 전환 오류! | 버프 적용 무기: " + weaponStr);
				return;
			}
			#endregion
			#region 설명
			string description = row[7] as string;
			#endregion

			// 파일 읽어 오는 방식으로 수정 필요
			BuffData buffData = new BuffData(title, code, buffType, effectType, grade, maxStack, weapon, description);

			m_BuffDictionary[buffType].Add((code, title), buffData);
		}
	}
#if UNITY_EDITOR
	public void CreateAllBuff(bool load, bool script, bool asset, bool uiAsset, bool switchCase)
	{
		if (Application.isEditor == false ||
			Application.isPlaying == true)
			return;

		if (script == false &&
			asset == false &&
			uiAsset == false &&
			switchCase == false)
			return;

		if (load == true ||
			m_BuffDictionary == null ||
			m_BuffDictionary.Count == 0)
		{
			LoadAllBuff();
		}

		StringBuilder sb = null;

		if (switchCase)
		{
			sb = new StringBuilder();

			sb.AppendLine("\t\tswitch (buffData.title)");
			sb.AppendLine("\t\t{");
		}

		for (E_BuffType buffType = E_BuffType.Buff; buffType != E_BuffType.Max - 1; ++buffType)
		{
			if (switchCase)
			{
				sb.Append("#region ");
				sb.AppendLine(BuffEnumUtil.EnumToKorString<E_BuffType>(buffType));
			}

			foreach (var item in m_BuffDictionary[buffType])
			{
				BuffData buffData = item.Value;

				if (buffData == null)
				{
					Debug.LogError("버프 데이터 없음");
					return;
				}

				if (script)
					CreateBuffScript(buffData);
				if (asset)
					CreateBuffScriptableObject(buffData);
				if (uiAsset)
					CreateBuffUIScriptableObject(buffData);
				if (switchCase)
					AppendBuffCase(sb, buffData.title);
			}

			if (switchCase)
				sb.AppendLine("#endregion");
		}

		if (switchCase)
		{
			sb.AppendLine("\t\t}");

			CreateBuffCase(sb);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}
	public void CreateAllBuffScript()
	{
		if (Application.isPlaying == true)
			return;

		if (m_BuffDictionary == null ||
			m_BuffDictionary.Count == 0)
		{
			LoadAllBuff();
		}

		for (E_BuffType buffType = E_BuffType.Buff; buffType != E_BuffType.Max - 1; ++buffType)
		{
			foreach (var item in m_BuffDictionary[buffType])
			{
				BuffData buffData = item.Value;

				if (buffData == null)
				{
					Debug.LogError("버프 데이터 없음");
					return;
				}

				CreateBuffScript(buffData);
			}
		}

		AssetDatabase.SaveAssets();
		Debug.Log("버프 Script 생성 완료");
		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}
	public void CreateAllBuffScriptableObject()
	{
		if (Application.isPlaying == true)
			return;

		if (m_BuffDictionary == null ||
			m_BuffDictionary.Count == 0)
		{
			LoadAllBuff();
		}

		for (E_BuffType buffType = E_BuffType.Buff; buffType != E_BuffType.Max - 1; ++buffType)
		{
			foreach (var item in m_BuffDictionary[buffType])
			{
				BuffData buffData = item.Value;

				if (buffData == null)
				{
					Debug.LogError("버프 데이터 없음");
					return;
				}

				CreateBuffScriptableObject(buffData);
				CreateBuffUIScriptableObject(buffData);
			}
		}

		AssetDatabase.SaveAssets();
		Debug.Log("버프 Scriptable Object 생성 완료");
		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}

	// 스크립트 생성
	public void CreateBuffScript(BuffData buffData)
	{
		string path = Path.Combine(Application.dataPath, "DataBase", "Script", buffData.buffType.ToString());

		if (Directory.Exists(path) == false)
			Directory.CreateDirectory(path);

		string file = Path.Combine(path, buffData.title + ".cs");
		string template = Path.Combine(Application.dataPath, "DataBase", "Script", "Template", "BuffScriptTemplate.txt");
		string className = buffData.title.Replace(' ', '_');

		StringBuilder sb = new StringBuilder(File.ReadAllText(template));

		sb.Replace("$Buff", className);
		sb.Replace("$Description", buffData.description);

		if (File.Exists(file) == true)
		{
			const string start = "\t{";
			const string end = "\t}";

			string templateCode = sb.ToString();
			sb.Clear();

			string[] newFileLines = templateCode.Split("\r\n");
			string[] oldFileLines = File.ReadAllLines(file);

			#region 템플릿 파일 함수 저장
			List<string> newFileFuncOrderList = new List<string>();
			Dictionary<string, string> newFileFuncMap = new Dictionary<string, string>();
			for (int i = 0; i < newFileLines.Length; ++i)
			{
				if (newFileLines[i] != start)
					continue;

				string funcName = newFileLines[i - 1];

				sb.AppendLine(newFileLines[i - 1]);
				for (int j = i; j < newFileLines.Length; ++j)
				{
					sb.AppendLine(newFileLines[j]);

					if (newFileLines[j] == end)
					{
						newFileFuncOrderList.Add(funcName);
						newFileFuncMap.Add(funcName, sb.ToString());
						sb.Clear();
						break;
					}
				}
			}
			#endregion

			#region 기존 파일 함수 저장
			Dictionary<string, string> oldFileFuncMap = new Dictionary<string, string>();
			for (int i = 0; i < oldFileLines.Length; ++i)
			{
				if (oldFileLines[i] != start)
					continue;

				string funcName = oldFileLines[i - 1];

				sb.AppendLine(oldFileLines[i - 1]);
				for (int j = i; j < oldFileLines.Length; ++j)
				{
					sb.AppendLine(oldFileLines[j]);

					if (oldFileLines[j] == end)
					{
						oldFileFuncMap.Add(funcName, sb.ToString());
						sb.Clear();
						break;
					}
				}
			}
			#endregion

			sb.Clear();

			foreach (var item in oldFileFuncMap)
			{
				if (newFileFuncMap.ContainsKey(item.Key))
				{
					newFileFuncMap[item.Key] = item.Value;
				}
			}

			List<string> deletedFuncNameList = new List<string>();
			for (int i = 0; i < oldFileLines.Length; ++i)
			{
				string oldfuncName = oldFileLines[i];

				if (deletedFuncNameList.Contains(oldfuncName))
				{
					int index = oldFileFuncMap[oldfuncName].Split("\r\n").Length - 1;
					i += index - 1;
					continue;
				}

				if (newFileFuncMap.ContainsKey(oldfuncName))
				{
					int index = newFileFuncOrderList.IndexOf(oldfuncName);

					for (int j = 0; j < index; ++j)
					{
						string newFuncName = newFileFuncOrderList[0];

						string[] funcLines = newFileFuncMap[newFuncName].Split("\r\n");

						for (int k = 0; k < funcLines.Length - 1; ++k)
						{
							sb.AppendLine(funcLines[k]);
						}

						newFileFuncOrderList.RemoveAt(0);
						newFileFuncMap.Remove(newFuncName);
						deletedFuncNameList.Add(newFuncName);
					}

					newFileFuncMap.Remove(oldfuncName);
					newFileFuncOrderList.Remove(oldfuncName);
				}

				if (i == oldFileLines.Length - 1)
					sb.Append(oldFileLines[i]);
				else
					sb.AppendLine(oldFileLines[i]);
			}
		}

		File.WriteAllText(file, sb.ToString());
	}
	// 스크립터블 오브젝트 생성
	public void CreateBuffScriptableObject(BuffData buffData)
	{
		string path = Path.Combine(Application.dataPath, "DataBase", "Scriptable Object", buffData.buffType.ToString());

		if (Directory.Exists(path) == false)
			Directory.CreateDirectory(path);

		string file = Path.Combine("Assets", "DataBase", "Scriptable Object", buffData.buffType.ToString(), buffData.title + ".asset");

		AssetDatabase.CreateAsset(buffData, file);
	}
	public void CreateBuffUIScriptableObject(BuffData buffData)
	{
		string path = Path.Combine(Application.dataPath, "DataBase", "Scriptable Object", buffData.buffType.ToString());

		if (Directory.Exists(path) == false)
			Directory.CreateDirectory(path);

		string file = Path.Combine("Assets", "DataBase", "Scriptable Object", buffData.buffType.ToString(), buffData.title + "_UI.asset");

		AssetDatabase.CreateAsset(new BuffUIData(buffData), file);
	}

	private void CreateBuffCase(StringBuilder sb, [System.Runtime.CompilerServices.CallerFilePath] string path = "")
	{
		string code = File.ReadAllText(path);

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

	public BuffData GetBuffData(int code)
	{
		BuffData buff = null;

		for (E_BuffType i = 0; i < E_BuffType.Max; ++i)
		{
			if (TryGetBuffData(i, code, out buff) == true)
				break;
		}

		return buff;
	}
	public bool TryGetBuffData(E_BuffType buffType, int code, out BuffData buff)
	{
		BuffDictionary buffDictionary = m_BuffDictionary[buffType];

		if (buffDictionary == null)
		{
			buff = null;

			Debug.LogError("버프 목록 생성 안됨");
			return false;
		}

		if (buffDictionary.TryGetValue(code, out buff))
		{
			buff = null;

			Debug.LogError("버프 가져오기 실패");
			return false;
		}

		return true;
	}
	public BuffData GetBuffData(string title)
	{
		BuffData buff = null;

		for (E_BuffType i = 0; i < E_BuffType.Max; ++i)
		{
			if (TryGetBuffData(i, title, out buff) == true)
				break;
		}

		return buff;
	}
	public bool TryGetBuffData(E_BuffType buffType, string title, out BuffData buff)
	{
		BuffDictionary buffDictionary = m_BuffDictionary[buffType];

		if (buffDictionary == null)
		{
			buff = null;

			Debug.LogError("버프 목록 생성 안됨");
			return false;
		}

		if (buffDictionary.TryGetValue(title, out buff) == false)
		{
			buff = null;

			Debug.LogError("버프 가져오기 실패");
			return false;
		}

		return true;
	}

	public AbstractBuff CreateBuff(int code)
	{
		BuffData buffData = GetBuffData(code);

		return CreateBuff(buffData);
	}
	public AbstractBuff CreateBuff(string title)
	{
		BuffData buffData = GetBuffData(title);

		return CreateBuff(buffData);
	}
	public AbstractBuff CreateBuff(BuffData buffData)
	{
		if (buffData == null)
			return null;

		// $BuffFunc
		switch (buffData.title)
		{
			#region 버프
			case "체력 증가":
				return new 체력_증가(buffData);
			case "재생":
				return new 재생(buffData);
			case "빠른 재생":
				return new 빠른_재생(buffData);
			case "회복량 증가":
				return new 회복량_증가(buffData);
			case "방어력 증가":
				return new 방어력_증가(buffData);
			case "회피율 증가":
				return new 회피율_증가(buffData);
			case "공격력 증가":
				return new 공격력_증가(buffData);
			case "공격 속도 증가":
				return new 공격_속도_증가(buffData);
			case "이동 속도 증가":
				return new 이동_속도_증가(buffData);
			case "공격 범위 증가":
				return new 공격_범위_증가(buffData);
			case "사거리 증가":
				return new 사거리_증가(buffData);
			case "투사체 속도 증가":
				return new 투사체_속도_증가(buffData);
			case "공격 횟수 증가":
				return new 공격_횟수_증가(buffData);
			case "치명타 확률 증가":
				return new 치명타_확률_증가(buffData);
			case "치명타 대미지 증가":
				return new 치명타_대미지_증가(buffData);
			case "점프 추가":
				return new 점프_추가(buffData);
			case "전방향 대쉬":
				return new 전방향_대쉬(buffData);
			case "대쉬 횟수 증가":
				return new 대쉬_횟수_증가(buffData);
			case "대쉬 충전 속도 증가":
				return new 대쉬_충전_속도_증가(buffData);
			case "대쉬 무적":
				return new 대쉬_무적(buffData);
			case "무적시간 증가":
				return new 무적시간_증가(buffData);
			#endregion
			#region 디버프
			case "체력 감소":
				return new 체력_감소(buffData);
			case "느린 재생":
				return new 느린_재생(buffData);
			case "회복량 감소":
				return new 회복량_감소(buffData);
			case "방어력 감소":
				return new 방어력_감소(buffData);
			case "회피율 감소":
				return new 회피율_감소(buffData);
			case "공격력 감소":
				return new 공격력_감소(buffData);
			case "공격 속도 감소":
				return new 공격_속도_감소(buffData);
			case "이동 속도 감소":
				return new 이동_속도_감소(buffData);
			case "공격 범위 감소":
				return new 공격_범위_감소(buffData);
			case "사거리 감소":
				return new 사거리_감소(buffData);
			case "투사체 속도 감소":
				return new 투사체_속도_감소(buffData);
			case "패링 확률 감소":
				return new 패링_확률_감소(buffData);
				#endregion
		}
		// $BuffFunc

		return null;
	}
}