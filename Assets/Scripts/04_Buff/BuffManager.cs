using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using SpreadSheet;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
// < 코드, 명칭, 버프 데이터 >
using BuffDictionary = DoubleKeyDictionary<int, string, BuffData>;

public sealed class BuffManager : Singleton<BuffManager>
{
	private Dictionary<int, BuffCounter> m_BuffCounterMap;
	// < 타입, 등급, 버프 >
	private Dictionary<E_BuffType, Dictionary<E_BuffGrade, BuffDictionary>> m_BuffMap;

	private int m_RewardsCount = 3;

	private System.Func<BuffData, bool> m_OnBuffAdded;
	private System.Func<BuffData, bool> m_OnBuffRemoved;

	public int rewardsCount
	{
		get
		{
			return m_RewardsCount;
		}
		set
		{
			m_RewardsCount = value;
		}
	}

	public event System.Func<BuffData, bool> onBuffAdded
	{
		add
		{
			m_OnBuffAdded += value;
		}
		remove
		{
			m_OnBuffAdded -= value;
		}
	}
	public event System.Func<BuffData, bool> onBuffRemoved
	{
		add
		{
			m_OnBuffRemoved += value;
		}
		remove
		{
			m_OnBuffRemoved -= value;
		}
	}

	public void Initialize()
	{
		m_BuffCounterMap = new Dictionary<int, BuffCounter>();
		m_BuffMap = new Dictionary<E_BuffType, Dictionary<E_BuffGrade, BuffDictionary>>();
		for (E_BuffType i = E_BuffType.Buff; i < E_BuffType.Max; ++i)
		{
			m_BuffMap[i] = new Dictionary<E_BuffGrade, BuffDictionary>();
			for (E_BuffGrade j = E_BuffGrade.Normal; j < E_BuffGrade.Max; ++j)
			{
				m_BuffMap[i][j] = new BuffDictionary();
			}
		}
	}

	[ContextMenu("LoadAllBuff")]
	public void LoadAllBuff()
	{
		if (Application.isPlaying == false)
			Initialize();

		DataSet dataSet = new DataSet();
		SpreadSheetManager.Instance.LoadJsonData(dataSet);

		for (E_BuffType buffType = E_BuffType.Buff; buffType < E_BuffType.Max - 1; ++buffType)
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
			BuffData buffData = new BuffData(title, code, buffType, effectType, grade, maxStack, weapon, description, null);

			m_BuffMap[buffType][grade].Add((code, title), buffData);

			BuffCounter buffCounter = new BuffCounter();
			buffCounter.code = code;
			buffCounter.count = 0;
			buffCounter.maxStack = maxStack;

			m_BuffCounterMap.Add(code, buffCounter);
		}
	}

	#region Create File
#if UNITY_EDITOR

	public void CreateAllBuff(bool load, bool script, bool asset, bool switchCase)
	{
		if (Application.isEditor == false ||
			Application.isPlaying == true)
			return;

		if (script == false &&
			asset == false &&
			switchCase == false)
			return;

		if (load == true ||
			m_BuffMap == null ||
			m_BuffMap.Count == 0)
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

		for (E_BuffType buffType = E_BuffType.Buff; buffType < E_BuffType.Max - 1; ++buffType)
		{
			for (E_BuffGrade grade = E_BuffGrade.Normal; grade < E_BuffGrade.Max; ++grade)
			{
				if (switchCase)
				{
					sb.Append("#region ");
					sb.AppendLine(BuffEnumUtil.EnumToKorString<E_BuffType>(buffType));
				}

				foreach (var item in m_BuffMap[buffType][grade])
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
					if (switchCase)
						AppendBuffCase(sb, buffData.title);
				}

				if (switchCase)
				{
					sb.AppendLine("#endregion");
				}
			}
		}

		if (switchCase)
		{
			sb.AppendLine("\t\t}");

			CreateBuffCase(sb);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}

	// 스크립트 생성
	private void CreateBuffScript(BuffData buffData)
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
	private void CreateBuffScriptableObject(BuffData buffData)
	{
		string path = Path.Combine(Application.dataPath, "DataBase", "Scriptable Object", buffData.buffType.ToString());

		if (Directory.Exists(path) == false)
			Directory.CreateDirectory(path);

		string file = Path.Combine("Assets", "DataBase", "Scriptable Object", buffData.buffType.ToString(), buffData.title + ".asset");

		AssetDatabase.CreateAsset(buffData, file);
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
	#endregion

	public BuffData GetBuffData(int code)
	{
		string codeStr = code.ToString();

		string buffTypeStr = codeStr[0].ToString();
		E_BuffType buffType = (E_BuffType)(int.Parse(buffTypeStr) - 1);

		string gradeStr = codeStr[2].ToString();
		E_BuffGrade grade = (E_BuffGrade)(int.Parse(gradeStr) - 1);

		TryGetBuffData(buffType, grade, code, out BuffData buffData);

		return buffData;
	}
	public bool TryGetBuffData(E_BuffType buffType, E_BuffGrade grade, int code, out BuffData buffData)
	{
		BuffDictionary buffDictionary = m_BuffMap[buffType][grade];

		if (buffDictionary == null)
		{
			buffData = null;

			Debug.LogError("버프 목록 생성 안됨");
			return false;
		}

		if (buffDictionary.TryGetValue(code, out buffData) == false)
		{
			buffData = null;

			Debug.LogError("버프 가져오기 실패");
			return false;
		}

		return true;
	}
	public BuffData GetBuffData(string title)
	{
		BuffData buffData = null;

		for (E_BuffType i = E_BuffType.Buff; i < E_BuffType.Max; ++i)
		{
			for (E_BuffGrade j = E_BuffGrade.Normal; j < E_BuffGrade.Max; ++j)
			{
				buffData = GetBuffData(i, j, title);
				if (buffData != null)
					return buffData;
			}
		}

		return buffData;
	}
	private BuffData GetBuffData(E_BuffType buffType, E_BuffGrade grade, string title)
	{
		TryGetBuffData(buffType, grade, title, out BuffData buffData);

		return buffData;
	}
	public bool TryGetBuffData(E_BuffType buffType, E_BuffGrade grade, string title, out BuffData buffData)
	{
		BuffDictionary buffDictionary = m_BuffMap[buffType][grade];

		if (buffDictionary == null)
		{
			buffData = null;

			Debug.LogError("버프 목록 생성 안됨");
			return false;
		}

		if (buffDictionary.TryGetValue(title, out buffData) == false)
		{
			buffData = null;

			Debug.LogError("버프 가져오기 실패");
			return false;
		}

		return true;
	}
	public BuffData GetRandomBuffData()
	{
		E_BuffType buffType = (E_BuffType)Random.Range(0, (int)E_BuffType.Max);
		E_BuffGrade grade = (E_BuffGrade)Random.Range(0, (int)E_BuffGrade.Max);

		return GetRandomBuffData(buffType, grade);
	}
	public BuffData GetRandomBuffData(E_BuffType buffType, E_BuffGrade grade)
	{
		BuffDictionary buffDictionary = m_BuffMap[buffType][grade];

		List<BuffData> dataList = buffDictionary.Values.ToList();
		dataList.RemoveAll((BuffData buffData) =>
		{
			BuffCounter buffCounter = m_BuffCounterMap[buffData.code];
			if (buffCounter.count >= buffCounter.maxStack)
				return true;

			return false;
		});

		if (dataList.Count <= 0)
			return null;

		int index = Random.Range(0, dataList.Count);

		return dataList[index];
	}
	public List<BuffData> GetRandomBuffData(E_BuffType buffType, E_BuffGrade grade, int count)
	{
		BuffDictionary buffDictionary = m_BuffMap[buffType][grade];

		List<BuffData> dataList = buffDictionary.Values.ToList();
		dataList.RemoveAll((BuffData buffData) =>
		{
			BuffCounter buffCounter = m_BuffCounterMap[buffData.code];
			if (buffCounter.count >= buffCounter.maxStack)
				return true;

			return false;
		});

		if (dataList.Count <= 0)
			return null;

		count = Mathf.Clamp(count, 0, dataList.Count);

		// 피셔 예이츠 셔플
		for (int i = 0; i < count; ++i)
		{
			int index = Random.Range(i, dataList.Count);

			dataList.Swap(i, index);
		}

		return dataList.GetRange(0, count);
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

	public bool AddBuff(BuffData buffData)
	{
		BuffCounter buffCounter = m_BuffCounterMap[buffData.code];
		if (buffCounter.count >= buffCounter.maxStack)
			return false;

		if (m_OnBuffAdded.Invoke(buffData))
		{
			++buffCounter.count;
			return true;
		}

		return false;
	}
	public bool RemoveBuff(BuffData buffData)
	{
		BuffCounter buffCounter = m_BuffCounterMap[buffData.code];
		if (buffCounter.count <= 0)
			return false;

		if (m_OnBuffRemoved.Invoke(buffData))
		{
			--buffCounter.count;
			return true;
		}

		return false;
	}
	public bool CombineBuff(BuffData first, BuffData second)
	{
		if (first == null)
			return false;

		if (second == null)
			return false;

		E_BuffType firstBuffType = first.buffType;
		E_BuffType secondBuffType = second.buffType;

		if (firstBuffType == E_BuffType.Bothbuff)
			firstBuffType = secondBuffType;
		else if (secondBuffType == E_BuffType.Bothbuff)
			secondBuffType = firstBuffType;

		if (firstBuffType != secondBuffType)
		{
			Debug.Log("Combine Buff`s buffType should be same. first = " + first.buffType.ToString() + ", second = " + second.buffType.ToString());
			return false;
		}

		float firstGrade = (int)first.grade + 1.0f;
		float secondGrade = (int)second.grade + 1.0f;
		float gradeInt = Mathf.Max(firstGrade, secondGrade) + 1;

		firstGrade = firstGrade / gradeInt;
		secondGrade = secondGrade / gradeInt;

		E_BuffGrade grade = (E_BuffGrade)Mathf.RoundToInt(firstGrade * secondGrade);

		BuffData buffData = null;
		switch (firstBuffType)
		{
			case E_BuffType.Buff:
				buffData = GetRandomBuffData(E_BuffType.Debuff, grade);
				break;
			case E_BuffType.Debuff:
				buffData = GetRandomBuffData(E_BuffType.Buff, grade);
				break;
			case E_BuffType.Bothbuff:
				buffData = GetRandomBuffData();
				break;
		}

		if (buffData == null)
			return false;

		if (AddBuff(buffData) == false)
		{
			Debug.Log("New Buff is Max Stack. buff = " + buffData.title);
			return false;
		}

		RemoveBuff(first);
		RemoveBuff(second);

		return true;
	}

	private class BuffCounter
	{
		public int code { get; set; }
		public int count { get; set; }
		public int maxStack { get; set; }
	}
	private class BuffGrade
	{
		public E_BuffGrade key { get; set; }
		public float rate { get; set; }
	}
}