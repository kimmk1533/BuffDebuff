using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Enum;
using AYellowpaper.SerializedCollections;
using BuffTypeGrade = System.Tuple<Enum.E_BuffType, Enum.E_BuffGrade>;
using Mono.Cecil.Cil;

public sealed class BuffManager : Singleton<BuffManager>
{
	#region 변수
	[Space(10)]
	[SerializeField]
	private BuffGradeInfo m_BuffGradeInfo = null;

	// < 타입, 등급, 코드, 명칭, 버프 >
	private DoubleKeyDictionary<int, string, BuffCounter> m_BuffMap = null;
	private Dictionary<BuffTypeGrade, List<int>> m_BuffTypeGradeMap = null;
	#endregion

	#region 프로퍼티

	#endregion

	#region 이벤트
	public event System.Func<BuffData, bool> onBuffAdded = null;
	public event System.Func<BuffData, bool> onBuffRemoved = null;
	#endregion

	#region 매니저
	private static PlayerManager M_Player => PlayerManager.Instance;
	#endregion

	public void Initialize()
	{
		// m_BuffGradeInfo
		if (m_BuffGradeInfo == null)
			m_BuffGradeInfo = new BuffGradeInfo();

		// m_BuffMap
		if (m_BuffMap == null)
			m_BuffMap = new DoubleKeyDictionary<int, string, BuffCounter>();

		// m_BuffTypeGradeMap
		if (m_BuffTypeGradeMap == null)
			m_BuffTypeGradeMap = new Dictionary<BuffTypeGrade, List<int>>();
	}
	public void Finallize()
	{
		// m_BuffMap
		if (m_BuffMap != null)
		{
			foreach (var buffTypeMap in m_BuffMap)
			{
				buffTypeMap.Value.Clear();
			}
		}
	}

	public void InitializeGame()
	{
		m_BuffGradeInfo.UpdateBuffGradeCurve();

		LoadAllBuff();
	}
	public void FinallizeGame()
	{

	}

	public void LoadAllBuff()
	{
		// 임시로 양면 버프 제외
		for (E_BuffType buffType = E_BuffType.Buff; buffType < E_BuffType.Max - 1; ++buffType)
		{
			LoadBuff(buffType);
		}
	}
	public void LoadBuff(E_BuffType buffType)
	{
		string path = Path.Combine("BuffData", buffType.ToString());
		BuffData[] buffDatas = Resources.LoadAll<BuffData>(path);

		for (int i = 0; i < buffDatas.Length; ++i)
		{
			BuffData buffData = buffDatas[i];

			if (m_BuffMap.ContainsKey1(buffData.code) == true)
				continue;

			BuffCounter buffCounter = new BuffCounter(buffData);
			m_BuffMap.Add(buffData.code, buffData.title, buffCounter);

			BuffTypeGrade buffTypeGrade = new BuffTypeGrade(buffData.buffType, buffData.buffGrade);

			if (m_BuffTypeGradeMap.ContainsKey(buffTypeGrade) == false)
				m_BuffTypeGradeMap.Add(buffTypeGrade, new List<int>());
			else if (m_BuffTypeGradeMap[buffTypeGrade].Contains(buffData.code))
				continue;

			m_BuffTypeGradeMap[buffTypeGrade].Add(buffData.code);
		}

		#region 이전 코드
		//string sheetName = string.Concat(BuffEnumUtil.ToKorString(buffType), " 목록");

		//DataRow[] rows = dataSet.Tables[sheetName].Select();

		//foreach (var row in rows)
		//{
		//	#region 코드
		//	// 코드 불러오기
		//	string codeStr = row[0] as string;

		//	// 자료형 파싱
		//	if (int.TryParse(codeStr, out int code) == false)
		//	{
		//		Debug.LogError("버프 코드 불러오기 오류! | 코드: " + codeStr);
		//		return;
		//	}
		//	#endregion
		//	#region 명칭
		//	// 명칭 불러오기
		//	string title = row[1] as string;
		//	#endregion
		//	#region 등급
		//	// 등급 불러오기
		//	string gradeStr = row[3] as string;

		//	// 자료형 파싱
		//	if (System.Enum.TryParse(gradeStr, out E_BuffGrade grade) == false)
		//	{
		//		Debug.LogError("버프 등급 전환 오류! | 버프 등급: " + gradeStr);
		//		return;
		//	}
		//	#endregion

		//	if (m_BuffMap.ContainsKey(buffType) == true &&
		//		m_BuffMap[buffType].ContainsKey(grade) == true &&
		//		m_BuffMap[buffType][grade].ContainsPrimaryKey(code) == true)
		//		continue;

		//	#region 효과 종류
		//	// 효과 종류 불러오기
		//	string effectTypeStr = row[2] as string;

		//	// 한글 -> 영어 전환
		//	switch (effectTypeStr)
		//	{
		//		case "스탯형":
		//			effectTypeStr = "Stat";
		//			break;
		//		case "무기형":
		//			effectTypeStr = "Weapon";
		//			break;
		//		case "전투형":
		//			effectTypeStr = "Combat";
		//			break;
		//		default:
		//			Debug.LogError("버프 효과 종류 불러오기 오류! | 버프 효과 종류: " + effectTypeStr);
		//			return;
		//	}

		//	// 자료형 파싱
		//	if (System.Enum.TryParse(effectTypeStr, out E_BuffEffectType effectType) == false)
		//	{
		//		Debug.LogError("버프 효과 종류 전환 오류! | 버프 효과 종류: " + effectTypeStr);
		//		return;
		//	}
		//	#endregion
		//	#region 최대 스택
		//	// 최대 스택 불러오기
		//	string maxStackStr = row[4] as string;

		//	// 자료형 파싱
		//	if (int.TryParse(maxStackStr, out int maxStack) == false)
		//	{
		//		Debug.LogError("버프 최대 스택 전환 오류! | 버프 최대 스택: " + maxStackStr);
		//		return;
		//	}
		//	#endregion
		//	#region 적용 무기
		//	// 적용되는 무기 불러오기
		//	string weaponStr = row[5] as string;

		//	// 한글 -> 영어 전환
		//	switch (weaponStr)
		//	{
		//		case "공통":
		//			weaponStr = "All";
		//			break;
		//		case "근거리 무기":
		//			weaponStr = "Melee";
		//			break;
		//		case "원거리 무기":
		//			weaponStr = "Ranged";
		//			break;
		//		default:
		//			Debug.LogError("버프 적용 무기 타입 전환 오류! | 버프 적용 무기: " + weaponStr);
		//			return;
		//	}

		//	// 자료형 파싱
		//	if (System.Enum.TryParse(weaponStr, out E_BuffWeapon weapon) == false)
		//	{
		//		Debug.LogError("버프 적용 무기 전환 오류! | 버프 적용 무기: " + weaponStr);
		//		return;
		//	}
		//	#endregion
		//	#region 발동 조건
		//	// 발동 조건 불러오기
		//	string conditionStr = row[6] as string;

		//	// 한글 -> 영어 전환
		//	switch (conditionStr)
		//	{
		//		case "버프를 얻을 때":
		//			conditionStr = "Added";
		//			break;
		//		case "버프를 잃을 때":
		//			conditionStr = "Removed";
		//			break;
		//		case "매 프레임마다":
		//			conditionStr = "Update";
		//			break;
		//		case "일정 시간마다":
		//			conditionStr = "Timer";
		//			break;
		//		case "점프 시":
		//			conditionStr = "Jump";
		//			break;
		//		case "대쉬 시":
		//			conditionStr = "Dash";
		//			break;
		//		case "타격 시":
		//			conditionStr = "GiveDamage";
		//			break;
		//		case "피격 시":
		//			conditionStr = "TakeDamage";
		//			break;
		//		case "공격 시작 시":
		//			conditionStr = "AttackStart";
		//			break;
		//		case "공격 시":
		//			conditionStr = "Attack";
		//			break;
		//		case "공격 종료 시":
		//			conditionStr = "AttackEnd";
		//			break;
		//		case "적 처치 시":
		//			conditionStr = "KillEnemy";
		//			break;
		//		case "사망 시":
		//			conditionStr = "Death";
		//			break;
		//		case "스테이지를 넘어갈 시":
		//			conditionStr = "NextStage";
		//			break;
		//		default:
		//			Debug.LogError("버프 발동 조건 불러오기 오류! | 발동 조건 종류: " + conditionStr);
		//			return;
		//	}

		//	// 자료형 파싱
		//	if (System.Enum.TryParse(conditionStr, out E_BuffInvokeCondition condition) == false)
		//	{
		//		Debug.LogError("버프 발동 조건 전환 오류! | 버프 등급: " + conditionStr);
		//		return;
		//	}
		//	#endregion
		//	#region 버프 값
		//	// 버프 값 불러오기
		//	string buffValueStr = row[7] as string;

		//	// 자료형 파싱
		//	if (float.TryParse(buffValueStr, out float buffValue) == false &&
		//		buffValueStr != "-")
		//	{
		//		Debug.LogError("버프 값 전환 오류! | 버프 값: " + buffValueStr);
		//		return;
		//	}
		//	#endregion
		//	#region 버프 시간
		//	// 버프 시간 불러오기
		//	string buffTimeStr = row[8] as string;

		//	// 자료형 파싱
		//	if (float.TryParse(buffTimeStr, out float buffTime) == false &&
		//		buffTimeStr != "-")
		//	{
		//		Debug.LogError("버프 시간 전환 오류! | 버프 시간: " + buffTimeStr);
		//		return;
		//	}
		//	#endregion
		//	#region 설명
		//	string description = row[9] as string;
		//	#endregion
		//	#region 이미지

		//	#endregion

		//	BuffData buffData = ScriptableObject.CreateInstance<BuffData>();
		//	buffData.Initialize(title, code, buffType, effectType, grade, maxStack, weapon, condition, buffValue, buffTime, description, null);

		//	m_BuffMap[buffType][grade].Add((code, title), buffData);

		//	BuffCounter buffCounter = new BuffCounter(code, title, maxStack);
		//	buffCounter.count = 0;

		//	m_BuffCounterMap.Add(code, buffCounter);
		//}
		#endregion
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

	public bool AddBuffCount(BuffData buffData)
	{
		if (m_BuffMap.ContainsKey1(buffData.code) == false)
			throw new System.Exception("버프 카운터 없음");

		BuffCounter buffCounter = m_BuffMap[buffData.code];
		if (buffCounter.count >= buffCounter.maxStack)
			return false;

		if (onBuffAdded == null)
			return false;

		bool result = true;
		foreach (System.Func<BuffData, bool> item in onBuffAdded.GetInvocationList())
		{
			result &= item(buffData);
		}

		if (result == true)
			++buffCounter.count;

		return result;
	}
	public bool RemoveBuffCount(BuffData buffData)
	{
		if (m_BuffMap.ContainsKey1(buffData.code) == false)
			throw new System.Exception("버프 카운터 없음");

		BuffCounter buffCounter = m_BuffMap[buffData.code];
		if (buffCounter.count <= 0)
			return false;

		if (onBuffRemoved == null)
			return false;

		bool result = true;
		foreach (System.Func<BuffData, bool> item in onBuffRemoved.GetInvocationList())
		{
			result &= item(buffData);
		}

		if (result == true)
			--buffCounter.count;

		return result;
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

		if (AddBuffCount(buffData) == false)
		{
			Debug.Log("New Buff is Max Stack. buff = " + buffData.title);
			return false;
		}

		RemoveBuffCount(first);
		RemoveBuffCount(second);

		return true;
	}

	public BuffData GetBuffData(int code)
	{
		if (m_BuffMap.TryGetValue(code, out BuffCounter buffCounter) == false)
			return null;

		return buffCounter.buffData;
	}
	public BuffData GetBuffData(string title)
	{
		if (m_BuffMap.TryGetValue(title, out BuffCounter buffCounter) == false)
			return null;

		return buffCounter.buffData;
	}

	public BuffData GetRandomBuffData()
	{
		E_BuffType buffType = (E_BuffType)Random.Range(0, (int)E_BuffType.Max);
		E_BuffGrade grade = m_BuffGradeInfo.GetRandomGrade(M_Player.currentLevel);

		return GetRandomBuffData(buffType, grade);
	}
	public BuffData GetRandomBuffData(E_BuffType buffType)
	{
		E_BuffGrade grade = m_BuffGradeInfo.GetRandomGrade(M_Player.currentLevel);

		return GetRandomBuffData(buffType, grade);
	}
	public BuffData GetRandomBuffData(E_BuffType buffType, E_BuffGrade grade)
	{
		if (buffType == E_BuffType.Max)
			throw new System.Exception("buffType 이 E_BuffType.Max 였습니다.");
		if (grade == E_BuffGrade.Max)
			throw new System.Exception("buffGrade 가 E_BuffGrade.Max 였습니다.");

		BuffTypeGrade buffTypeGrade = new BuffTypeGrade(buffType, grade);

		if (m_BuffTypeGradeMap.TryGetValue(buffTypeGrade, out List<int> codeList) == false)
			throw new System.Exception("m_BuffTypeGradeMap 안에 값이 없습니다.");

		if (codeList == null ||
			codeList.Count == 0)
		{
			Debug.Log("조건에 해당하는 버프가 없음.");
			return null;
		}

		int randomIndex = Random.Range(0, codeList.Count);
		int code = codeList[randomIndex];

		return m_BuffMap[code].buffData;
	}
	public List<BuffData> GetRandomBuffData(E_BuffType buffType, E_BuffGrade grade, int count)
	{
		if (buffType == E_BuffType.Max)
			throw new System.Exception("buffType 이 E_BuffType.Max 였습니다.");
		if (grade == E_BuffGrade.Max)
			throw new System.Exception("buffGrade 가 E_BuffGrade.Max 였습니다.");

		BuffTypeGrade buffTypeGrade = new BuffTypeGrade(buffType, grade);

		if (m_BuffTypeGradeMap.TryGetValue(buffTypeGrade, out List<int> codeList) == false)
			throw new System.Exception("m_BuffTypeGradeMap 안에 값이 없습니다.");

		if (codeList == null ||
			codeList.Count == 0)
		{
			Debug.Log("조건에 해당하는 버프가 없음.");
			return null;
		}

		count = Mathf.Clamp(count, 0, codeList.Count);

		// 피셔 예이츠 셔플
		for (int i = 0; i < count; ++i)
		{
			int randomIndex = Random.Range(0, codeList.Count);

			codeList.Swap(i, randomIndex);
		}

		List<BuffData> result = new List<BuffData>();
		for (int i = 0; i < count; ++i)
		{
			result.Add(m_BuffMap[codeList[i]].buffData);
		}

		return result;
	}

#if UNITY_EDITOR
	public void OnValidate()
	{
		if (Application.isPlaying == true)
			return;

		if (m_BuffGradeInfo != null)
			m_BuffGradeInfo.OnValidate();
	}
#endif

	[System.Serializable]
	private class BuffCounter
	{
		#region 변수
		[SerializeField]
		private BuffData m_BuffData;

		[Space(5)]
		[SerializeField]
		private int m_Count;
		#endregion

		#region 프로퍼티
		public BuffData buffData => m_BuffData;
		public int maxStack => m_BuffData.maxStack;
		public int count
		{
			get => m_Count;
			set => m_Count = value;
		}
		#endregion

		public BuffCounter(BuffData buffData)
		{
			m_BuffData = buffData;
			m_Count = 0;
		}

		public void Clear()
		{
			m_Count = 0;
		}
	}
	[System.Serializable]
	private class BuffGradeInfo
	{
		[SerializeField, Min(1f)]
		private float m_Width;
		[SerializeField, Min(0.01f)]
		private float m_Height;
		[SerializeField]
		private float m_Offset;

		[Space(5)]
		[SerializeField]
		[SerializedDictionary("버프 등급", "버프 정보")]
		private SerializedDictionary<E_BuffGrade, BuffGradeInfoItem> m_BuffGradeInfoMap;

		public E_BuffGrade GetRandomGrade(int level)
		{
			float max = 0f;
			float[] rates = new float[(int)E_BuffGrade.Max];

			int index;
			for (E_BuffGrade grade = 0; grade < E_BuffGrade.Max; ++grade)
			{
				index = (int)grade;

				rates[index] = m_BuffGradeInfoMap[grade].curve.keys[level].value;

				max += rates[index];
			}

			int randInt = Random.Range(0, 100);
			float rand = randInt * 0.01f;

			float sum = 0f;
			E_BuffGrade result = E_BuffGrade.Max;
			for (E_BuffGrade grade = 0; grade < E_BuffGrade.Max; ++grade)
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

		public void OnValidate()
		{
			int grade_index;

			UpdateBuffGradeCurve();

			float[] leftRates = new float[(int)E_BuffGrade.Max];
			float[] rightRates = new float[(int)E_BuffGrade.Max];
			float[] currentRates = new float[(int)E_BuffGrade.Max];

			float leftMax = 0f;
			float rightMax = 0f;
			float currentMax = 0f;

			for (E_BuffGrade grade = 0; grade < E_BuffGrade.Max; ++grade)
			{
				grade_index = (int)grade;

				leftRates[grade_index] = m_BuffGradeInfoMap[grade].curve.keys[0].value;
				rightRates[grade_index] = m_BuffGradeInfoMap[grade].curve.keys[M_Player.maxLevel].value;
				currentRates[grade_index] = m_BuffGradeInfoMap[grade].curve.keys[M_Player.currentLevel].value;

				leftMax += leftRates[grade_index];
				rightMax += rightRates[grade_index];
				currentMax += currentRates[grade_index];
			}

			for (E_BuffGrade grade = 0; grade < E_BuffGrade.Max; ++grade)
			{
				grade_index = (int)grade;

				leftRates[grade_index] /= leftMax;
				rightRates[grade_index] /= rightMax;
				currentRates[grade_index] /= currentMax;

				m_BuffGradeInfoMap[grade].minLevelPercent = leftRates[grade_index] * 100f;
				m_BuffGradeInfoMap[grade].maxLevelPercent = rightRates[grade_index] * 100f;
				m_BuffGradeInfoMap[grade].currLevelPercent = currentRates[grade_index] * 100f;
			}
		}
		public void UpdateBuffGradeCurve()
		{
			foreach (var item in m_BuffGradeInfoMap)
			{
				int length = item.Value.curve.length;
				for (int i = 0; i < length; ++i)
				{
					item.Value.curve.RemoveKey(0);
				}
			}

			int maxLevel = M_Player.maxLevel;
			float levelPercent, rate;

			for (int level = 0; level <= maxLevel; ++level)
			{
				levelPercent = (float)level / maxLevel;

				for (E_BuffGrade grade = 0; grade < E_BuffGrade.Max; ++grade)
				{
					rate = BuffGradeRateFormula(levelPercent, grade);

					m_BuffGradeInfoMap[grade].curve.AddKey(new Keyframe(levelPercent, rate)
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
		private float BuffGradeRateFormula(float levelPercent, E_BuffGrade grade)
		{
			const int n = (int)E_BuffGrade.Max;

			const float PI2 = 2 * Mathf.PI;
			float width = m_Width + m_BuffGradeInfoMap[grade].width;
			float height = m_Height + m_BuffGradeInfoMap[grade].height;
			float offset = m_Offset + m_BuffGradeInfoMap[grade].offset;

			// 대충 직접 만든 공식
			// 그래프 그려볼 수 있는 사이트: https://www.desmos.com/calculator/kihjluohq9?lang=ko
			float value = height * Mathf.Cos(PI2 * (levelPercent - offset - (float)grade / n) / width);

			return Mathf.Clamp01(value);
		}

		[System.Serializable]
		private class BuffGradeInfoItem
		{
			#region 변수
			[SerializeField]
			private float m_Width;
			[SerializeField]
			private float m_Height;
			[SerializeField]
			private float m_Offset;

			[Space(5)]
			[SerializeField, ReadOnly(true)]
			private AnimationCurve m_Curve;
			[SerializeField, ReadOnly]
			private float m_MinLevelPercent;
			[SerializeField, ReadOnly]
			private float m_MaxLevelPercent;
			[SerializeField, ReadOnly]
			private float m_CurrLevelPercent;
			#endregion

			#region 프로퍼티
			public float width => m_Width;
			public float height => m_Height;
			public float offset => m_Offset;
			public AnimationCurve curve => m_Curve;
			public float minLevelPercent
			{
				get => m_MinLevelPercent;
				set => m_MinLevelPercent = value;
			}
			public float maxLevelPercent
			{
				get => m_MaxLevelPercent;
				set => m_MaxLevelPercent = value;
			}
			public float currLevelPercent
			{
				get => m_CurrLevelPercent;
				set => m_CurrLevelPercent = value;
			}
			#endregion
		}
	}
}