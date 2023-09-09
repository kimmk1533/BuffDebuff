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
// < 코드, 명칭, 버프 데이터 >
using BuffDictionary = DoubleKeyDictionary<int, string, BuffData>;

public sealed class BuffManager : Singleton<BuffManager>
{
	private Dictionary<int, BuffCounter> m_BuffCounterMap;
	// < 타입, 등급, 버프 >
	private Dictionary<E_BuffType, Dictionary<E_BuffGrade, BuffDictionary>> m_BuffMap;

	[SerializeField, Min(0.01f)]
	private float m_A;
	[SerializeField, Min(1f)]
	private float m_B;
	[SerializeField]
	private float m_C;

	[SerializeField]
	private List<BuffGrade> m_BuffGradeInfoList;

	private System.Func<BuffData, bool> m_OnBuffAdded;
	private System.Func<BuffData, bool> m_OnBuffRemoved;

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

	private PlayerManager M_Player => PlayerManager.Instance;

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

		UpdateBuffGradeCurve();

		LoadAllBuff();
	}
	public void InitializeEvent()
	{
		m_OnBuffAdded = null;
		m_OnBuffRemoved = null;
	}

	public void LoadAllBuff()
	{
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
					Debug.LogError("버프 적용 무기 타입 전환 오류! | 버프 적용 무기: " + weaponStr);
					return;
			}

			// 자료형 파싱
			if (System.Enum.TryParse(weaponStr, out E_BuffWeapon weapon) == false)
			{
				Debug.LogError("버프 적용 무기 전환 오류! | 버프 적용 무기: " + weaponStr);
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
					Debug.LogError("버프 발동 조건 불러오기 오류! | 발동 조건 종류: " + conditionStr);
					return;
			}

			// 자료형 파싱
			if (System.Enum.TryParse(conditionStr, out E_BuffInvokeCondition condition) == false)
			{
				Debug.LogError("버프 발동 조건 전환 오류! | 버프 등급: " + conditionStr);
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
				Debug.LogError("버프 값 전환 오류! | 버프 값: " + buffValueStr);
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
				Debug.LogError("버프 시간 전환 오류! | 버프 시간: " + buffTimeStr);
				return;
			}
			#endregion
			#region 설명
			string description = row[9] as string;
			#endregion
			#region 이미지

			#endregion

			BuffData buffData = new BuffData(title, code, buffType, effectType, grade, maxStack, weapon, condition, buffValue, buffTime, description, null);

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

		if (load == false &&
			script == false &&
			asset == false &&
			switchCase == false)
			return;

		if (load)
		{
			SpreadSheetManager.Instance.Initialize();
		}

		Initialize();

		StringBuilder sb = null;

		if (switchCase)
		{
			sb = new StringBuilder();

			sb.AppendLine("\t\tswitch (buffData.title)");
			sb.AppendLine("\t\t{");
		}

		for (E_BuffType buffType = E_BuffType.Buff; buffType < E_BuffType.Max - 1; ++buffType)
		{
			if (switchCase)
			{
				sb.Append("#region ");
				sb.AppendLine(BuffEnumUtil.EnumToKorString<E_BuffType>(buffType));
			}

			for (E_BuffGrade grade = E_BuffGrade.Normal; grade < E_BuffGrade.Max; ++grade)
			{
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
			}

			if (switchCase)
			{
				sb.AppendLine("#endregion");
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
		string conditionInterface = "IOnBuff" + buffData.buffInvokeCondition.ToString();
		string conditionFormat = @"
	public void OnBuff{0}<T>(Character<T> character) where T : CharacterStat, new()
	{

	}";
		string condition = conditionFormat.Replace("{0}", buffData.buffInvokeCondition.ToString());

		switch (buffData.buffInvokeCondition)
		{
			case E_BuffInvokeCondition.Added:
				conditionInterface += ", IOnBuffRemoved";
				condition += conditionFormat.Replace("{0}", "Removed");
				break;
			case E_BuffInvokeCondition.Removed:
				conditionInterface = "IOnBuffAdded, " + conditionInterface;
				condition = conditionFormat.Replace("{0}", "Added") + condition;
				break;

			case E_BuffInvokeCondition.GiveDamage:
				conditionInterface += ", IOnBuffTakeDamage";
				condition += conditionFormat.Replace("{0}", "TakeDamage");
				break;
			case E_BuffInvokeCondition.TakeDamage:
				conditionInterface = "IOnBuffGiveDamage, " + conditionInterface;
				condition = conditionFormat.Replace("{0}", "GiveDamage") + condition;
				break;

			case E_BuffInvokeCondition.AttackStart:
				conditionInterface += ", IOnBuffAttack";
				conditionInterface += ", IOnBuffAttackEnd";
				condition += conditionFormat.Replace("{0}", "Attack");
				condition += conditionFormat.Replace("{0}", "AttackEnd");
				break;
			case E_BuffInvokeCondition.Attack:
				conditionInterface = "IOnBuffAttackStart, " + conditionInterface;
				conditionInterface += ", IOnBuffAttackEnd";
				condition = conditionFormat.Replace("{0}", "AttackStart") + condition;
				condition += conditionFormat.Replace("{0}", "AttackEnd");
				break;
			case E_BuffInvokeCondition.AttackEnd:
				conditionInterface = "IOnBuffAttackStart, IOnBuffAttack, " + conditionInterface;
				condition = conditionFormat.Replace("{0}", "AttackStart") + conditionFormat.Replace("{0}", "Attack") + condition;
				break;
		}

		StringBuilder sb = new StringBuilder(File.ReadAllText(template));

		sb.Replace("$Title", className);
		sb.Replace("$ConditionInterface", conditionInterface);
		sb.Replace("$Condition", condition);
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
			case "공격력 증가":
				return new 공격력_증가(buffData);
			case "공격 속도 증가":
				return new 공격_속도_증가(buffData);
			case "방어력 증가":
				return new 방어력_증가(buffData);
			case "회피율 증가":
				return new 회피율_증가(buffData);
			case "이동 속도 증가":
				return new 이동_속도_증가(buffData);
			case "재생":
				return new 재생(buffData);
			case "빠른 재생":
				return new 빠른_재생(buffData);
			case "회복량 증가":
				return new 회복량_증가(buffData);
			case "공격 범위 증가":
				return new 공격_범위_증가(buffData);
			case "사거리 증가":
				return new 사거리_증가(buffData);
			case "투사체 속도 증가":
				return new 투사체_속도_증가(buffData);
			case "치명타 확률 증가":
				return new 치명타_확률_증가(buffData);
			case "치명타 대미지 증가":
				return new 치명타_대미지_증가(buffData);
			case "추가 점프":
				return new 추가_점프(buffData);
			case "공격 횟수 증가":
				return new 공격_횟수_증가(buffData);
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
			case "느린 재생":
				return new 느린_재생(buffData);
			case "회복량 감소":
				return new 회복량_감소(buffData);
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

		E_BuffGrade firstBuffGrade = first.buffGrade;
		E_BuffGrade secondBuffGrade = second.buffGrade;

		if (firstBuffGrade != secondBuffGrade)
		{
			Debug.Log("Combine Buff`s buffGrade should be same. first = " + first.buffGrade.ToString() + ", second = " + second.buffGrade.ToString());
			return false;
		}

		E_BuffGrade grade = firstBuffGrade + 1;

		if (grade == E_BuffGrade.Max)
			grade = E_BuffGrade.Max - 1;

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
		if (grade == E_BuffGrade.Max)
		{
			Debug.LogError("Random Grade should be not E_BuffGrade.Max!");
			buffData = null;
			return false;
		}

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
		if (grade == E_BuffGrade.Max)
		{
			Debug.LogError("Random Grade should be not E_BuffGrade.Max!");
			return null;
		}

		TryGetBuffData(buffType, grade, title, out BuffData buffData);

		return buffData;
	}
	public bool TryGetBuffData(E_BuffType buffType, E_BuffGrade grade, string title, out BuffData buffData)
	{
		if (grade == E_BuffGrade.Max)
		{
			Debug.LogError("Random Grade should be not E_BuffGrade.Max!");
			buffData = null;
			return false;
		}

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
		if (grade == E_BuffGrade.Max)
		{
			Debug.LogError("Random Grade should be not E_BuffGrade.Max!");
			return null;
		}

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
		if (grade == E_BuffGrade.Max)
		{
			Debug.LogError("Random Grade should be not E_BuffGrade.Max!");
			return null;
		}

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

	public E_BuffGrade GetRandomGrade(int level)
	{
		const E_BuffGrade MaxBuffGrade = E_BuffGrade.Max - 1;

		float max = 0f;
		float[] rates = new float[(int)MaxBuffGrade];

		int index;
		for (E_BuffGrade grade = 0; grade < MaxBuffGrade; ++grade)
		{
			index = (int)grade;

			rates[index] = m_BuffGradeInfoList[index].curve.keys[level].value;

			max += rates[index];
		}

		int randInt = Random.Range(0, 100);
		float rand = randInt * 0.01f;

		float sum = 0f;
		E_BuffGrade result = E_BuffGrade.Max;
		for (E_BuffGrade grade = 0; grade < MaxBuffGrade; ++grade)
		{
			index = (int)grade;

			sum += rates[index] / max;

			if (rand < sum)
			{
				result = grade;
				break;
			}
		}

		return result;
	}

	private void UpdateBuffGradeCurve()
	{
		foreach (var item in m_BuffGradeInfoList)
		{
			int length = item.curve.length;
			for (int i = 0; i < length; ++i)
			{
				item.curve.RemoveKey(0);
			}
		}

		int maxLevel = M_Player.maxLevel + 1;
		float levelPercent, rate;

		for (int level = 0; level < maxLevel; ++level)
		{
			levelPercent = (float)level / maxLevel;

			for (E_BuffGrade grade = 0; grade < E_BuffGrade.GOD; ++grade)
			{
				rate = BuffGradeRateFormula(levelPercent, grade);

				m_BuffGradeInfoList[(int)grade].curve.AddKey(new Keyframe(levelPercent, rate)
				{
					weightedMode = WeightedMode.Both,
					inTangent = 0f,
					outTangent = 0f,
					inWeight = 0f,
					outWeight = 0f,
				});
			}
		}
	}
	private float BuffGradeRateFormula(float levelPercent, E_BuffGrade buffGrade)
	{
		const int n = (int)E_BuffGrade.GOD - 1;

		int grade = (int)buffGrade;

		const float PI2 = 2 * Mathf.PI;
		float a = m_A + m_BuffGradeInfoList[grade].a;
		float b = m_B + m_BuffGradeInfoList[grade].b;
		float c = m_C + m_BuffGradeInfoList[grade].c;

		// 대충 직접 만든 공식
		// 그래프 그려볼 수 있는 사이트: https://www.desmos.com/calculator/kihjluohq9?lang=ko
		float value = a * Mathf.Cos(PI2 * (levelPercent - c - (float)grade / n) / b);

		return Mathf.Clamp01(value);
	}

	public void OnValidate()
	{
		const E_BuffGrade MaxBuffGrade = E_BuffGrade.Max - 1;
		int grade_index;

		UpdateBuffGradeCurve();

		float[] leftRates = new float[(int)MaxBuffGrade];
		float[] rightRates = new float[(int)MaxBuffGrade];
		float[] currentRates = new float[(int)MaxBuffGrade];

		float leftMax = 0f;
		float rightMax = 0f;
		float currentMax = 0f;

		for (E_BuffGrade grade = 0; grade < MaxBuffGrade; ++grade)
		{
			grade_index = (int)grade;

			leftRates[grade_index] = m_BuffGradeInfoList[grade_index].curve.keys[0].value;
			rightRates[grade_index] = m_BuffGradeInfoList[grade_index].curve.keys[M_Player.maxLevel].value;
			currentRates[grade_index] = m_BuffGradeInfoList[grade_index].curve.keys[M_Player.currentLevel].value;

			leftMax += leftRates[grade_index];
			rightMax += rightRates[grade_index];
			currentMax += currentRates[grade_index];
		}

		for (E_BuffGrade grade = 0; grade < MaxBuffGrade; ++grade)
		{
			grade_index = (int)grade;

			leftRates[grade_index] /= leftMax;
			rightRates[grade_index] /= rightMax;
			currentRates[grade_index] /= currentMax;

			m_BuffGradeInfoList[grade_index].leftPercent = Mathf.RoundToInt(leftRates[grade_index] * 100f);
			m_BuffGradeInfoList[grade_index].rightPercent = Mathf.RoundToInt(rightRates[grade_index] * 100f);
			m_BuffGradeInfoList[grade_index].currentPercent = Mathf.RoundToInt(currentRates[grade_index] * 100f);
		}
	}

	private class BuffCounter
	{
		public int code { get; set; }
		public int count { get; set; }
		public int maxStack { get; set; }
	}
	[System.Serializable]
	private class BuffGrade
	{
		[SerializeField, ReadOnly]
		private E_BuffGrade m_Grade;
		[SerializeField]
		private float m_A;
		[SerializeField]
		private float m_B;
		[SerializeField]
		private float m_C;
		[SerializeField]
		private AnimationCurve m_Curve;

		[SerializeField, ReadOnly]
		private int m_LeftPercent;
		[SerializeField, ReadOnly]
		private int m_RightPercent;
		[SerializeField, ReadOnly]
		private int m_CurrentPercent;

		public E_BuffGrade buffGrade
		{
			get => m_Grade;
			set => m_Grade = value;
		}
		public float a => m_A;
		public float b => m_B;
		public float c => m_C;
		public AnimationCurve curve => m_Curve;
		public int leftPercent
		{
			get => m_LeftPercent;
			set => m_LeftPercent = value;
		}
		public int rightPercent
		{
			get => m_RightPercent;
			set => m_RightPercent = value;
		}
		public int currentPercent
		{
			get => m_CurrentPercent;
			set => m_CurrentPercent = value;
		}
	}
}