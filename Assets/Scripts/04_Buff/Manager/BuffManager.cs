using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BuffDebuff.Enum;
using AYellowpaper.SerializedCollections;
using BuffTypeGrade = System.Tuple<BuffDebuff.Enum.E_BuffType, BuffDebuff.Enum.E_BuffGrade>;

namespace BuffDebuff
{
	public sealed class BuffManager : Singleton<BuffManager>
	{
		#region 변수
		[Space(10)]
		[SerializeField]
		private BuffGradeInfo m_BuffGradeInfo = null;

		// < 코드, 명칭, 버프 >
		private DoubleKeyDictionary<int, string, BuffData> m_BuffDataMap = null;
		// < 타입, 등급, 코드 >
		private Dictionary<BuffTypeGrade, List<int>> m_BuffTypeGradeMap = null;
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
			if (m_BuffDataMap == null)
				m_BuffDataMap = new DoubleKeyDictionary<int, string, BuffData>();

			// m_BuffTypeGradeMap
			if (m_BuffTypeGradeMap == null)
				m_BuffTypeGradeMap = new Dictionary<BuffTypeGrade, List<int>>();

			LoadAllBuffData();
		}
		public void Finallize()
		{

		}

		public void InitializeGame()
		{
			m_BuffGradeInfo.UpdateBuffGradeCurve();

			foreach (var item in m_BuffDataMap)
			{
				BuffData buffData = item.Value;
				BuffTypeGrade buffTypeGrade = new BuffTypeGrade(buffData.buffType, buffData.buffGrade);

				if (m_BuffTypeGradeMap.ContainsKey(buffTypeGrade) == false)
					m_BuffTypeGradeMap.Add(buffTypeGrade, new List<int>());
				else if (m_BuffTypeGradeMap[buffTypeGrade].Contains(buffData.code))
					continue;

				m_BuffTypeGradeMap[buffTypeGrade].Add(buffData.code);
			}
		}
		public void FinallizeGame()
		{

		}

		private void LoadAllBuffData()
		{
			// 임시로 양면 버프 제외
			for (E_BuffType buffType = E_BuffType.Buff; buffType <= E_BuffType.Debuff; ++buffType)
			{
				LoadBuffData(buffType);
			}
		}
		private void LoadBuffData(E_BuffType buffType)
		{
			string path = Path.Combine("Scriptable Object", "BuffData", buffType.ToString());
			BuffData[] buffDatas = Resources.LoadAll<BuffData>(path);

			for (int i = 0; i < buffDatas.Length; ++i)
			{
				BuffData buffData = buffDatas[i];

				if (m_BuffDataMap.ContainsKey1(buffData.code) == true)
					continue;

				m_BuffDataMap.Add(buffData.code, buffData.title, buffData);
			}
		}

		//public bool AddBuffCount(BuffData buffData)
		//{
		//	if (m_BuffMap.ContainsKey1(buffData.code) == false)
		//		throw new System.Exception("버프 카운터 없음");

		//	Buff buff = m_BuffMap[buffData.code];
		//	if (buff.count >= buff.maxStack)
		//		return false;

		//	if (onBuffAdded == null)
		//		return false;

		//	bool result = true;
		//	foreach (System.Func<BuffData, bool> item in onBuffAdded.GetInvocationList())
		//	{
		//		result &= item(buffData);
		//	}

		//	if (result == true)
		//		++buff.count;

		//	return result;
		//}
		//public bool RemoveBuffCount(BuffData buffData)
		//{
		//	if (m_BuffMap.ContainsKey1(buffData.code) == false)
		//		throw new System.Exception("버프 카운터 없음");

		//	Buff buff = m_BuffMap[buffData.code];
		//	if (buff.count <= 0)
		//		return false;

		//	if (onBuffRemoved == null)
		//		return false;

		//	bool result = true;
		//	foreach (System.Func<BuffData, bool> item in onBuffRemoved.GetInvocationList())
		//	{
		//		result &= item(buffData);
		//	}

		//	if (result == true)
		//		--buff.count;

		//	return result;
		//}

		public BuffData GetBuffData(int code)
		{
			if (m_BuffDataMap.TryGetValue(code, out BuffData buffData) == false)
				return null;

			return buffData;
		}
		public BuffData GetBuffData(string title)
		{
			if (m_BuffDataMap.TryGetValue(title, out BuffData buffData) == false)
				return null;

			return buffData;
		}

		public BuffData GetRandomBuffData()
		{
			E_BuffType buffType = (E_BuffType)Random.Range(0, (int)E_BuffType.Max);
			E_BuffGrade grade = m_BuffGradeInfo.GetRandomGrade(M_Player.currentLevel + 2);

			return GetRandomBuffData(buffType, grade);
		}
		public BuffData GetRandomBuffData(E_BuffType buffType)
		{
			E_BuffGrade grade = m_BuffGradeInfo.GetRandomGrade(M_Player.currentLevel + 2);

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
				throw new System.Exception("m_BuffTypeGradeMap 안에 값이 없습니다." + buffTypeGrade.ToString());

			if (codeList == null ||
				codeList.Count == 0)
			{
				Debug.Log("조건에 해당하는 버프가 없음.");
				return null;
			}

			int randomIndex = Random.Range(0, codeList.Count);
			int code = codeList[randomIndex];

			return m_BuffDataMap[code];
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
				result.Add(m_BuffDataMap[codeList[i]]);
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

					leftRates[grade_index] = m_BuffGradeInfoMap[grade].curve.keys[2].value;
					rightRates[grade_index] = m_BuffGradeInfoMap[grade].curve.keys[M_Player.maxLevel + 2].value;
					currentRates[grade_index] = m_BuffGradeInfoMap[grade].curve.keys[M_Player.currentLevel + 2].value;

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

				for (E_BuffGrade grade = 0; grade < E_BuffGrade.Max; ++grade)
				{
					m_BuffGradeInfoMap[grade].curve.AddKey(new Keyframe(-0.002f, 1f)
					{
						weightedMode = WeightedMode.Both,
						inTangent = 0f,
						outTangent = 0f,
						inWeight = 0f,
						outWeight = 0f,
					});
					m_BuffGradeInfoMap[grade].curve.AddKey(new Keyframe(-0.001f, 0f)
					{
						weightedMode = WeightedMode.Both,
						inTangent = 0f,
						outTangent = 0f,
						inWeight = 0f,
						outWeight = 0f,
					});

					for (int level = 0; level <= maxLevel; ++level)
					{
						levelPercent = (float)level / maxLevel;

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
}