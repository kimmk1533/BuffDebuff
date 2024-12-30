using System.Collections;
using System.Collections.Generic;
using BuffDebuff.Enum;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace BuffDebuff
{
	[CreateAssetMenu(fileName = "BuffGradeInfo", menuName = "Scriptable Objects/BuffGradeInfo")]
	public class BuffGradeInfo : SerializedScriptableObject
	{
		#region 변수
		[SerializeField, MinValue(1f)]
		private float m_Width = 2f;
		[SerializeField, MinValue(1f)]
		private float m_Height = 1f;
		[SerializeField]
		private float m_Offset;
		[SerializeField, PropertyRange(1f, "maxLevel")]
		private int m_CurrentLevel;

		//[System.NonSerialized, OdinSerialize]
		[SerializeField]
		[PropertyOrder(1f)]
		[DictionaryDrawerSettings(KeyLabel = "버프 등급", ValueLabel = "버프 정보")]
		private Dictionary<E_BuffGrade, BuffGradeInfoItem> m_BuffGradeInfoMap = new Dictionary<E_BuffGrade, BuffGradeInfoItem>();
		#endregion

		#region 프로퍼티
		private int maxLevel => PlayerManager.playerMaxLevel;
		#endregion

		public E_BuffGrade GetRandomGrade(int level)
		{
			float max = 0f;
			float[] rates = new float[(int)E_BuffGrade.Max];

			int index;
			for (E_BuffGrade grade = 0; grade < E_BuffGrade.Max; ++grade)
			{
				index = (int)grade;

				rates[index] = m_BuffGradeInfoMap[grade].GetBuffProbability(level);

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

		private void OnValidate()
		{
			UpdateBuffGradeTexture();
			UpdateBuffGradePercent();
		}
		public void UpdateBuffGradeTexture()
		{
			for (E_BuffGrade buffGrade = E_BuffGrade.Normal; buffGrade < E_BuffGrade.Max; ++buffGrade)
			{
				if (m_BuffGradeInfoMap.ContainsKey(buffGrade) == false)
				{
					Debug.LogError("버프 확률 정보에 " + buffGrade.ToString() + " 없음");
					continue;
				}

				BuffGradeInfoItem buffGradeInfoItem = m_BuffGradeInfoMap[buffGrade];

				Texture2D texture2D = buffGradeInfoItem.CreateTexture();

				int prevLevel = 1;
				buffGradeInfoItem.SetBuffProbability(prevLevel, BuffGradeRateFormula(0f, buffGrade) * texture2D.height);

				for (int currLevel = prevLevel + 1; currLevel <= maxLevel; ++currLevel, ++prevLevel)
				{
					float prevLevelPercent = (float)prevLevel / maxLevel;
					float currLevelPercent = (float)currLevel / maxLevel;

					float prevProbability = BuffGradeRateFormula(prevLevelPercent, buffGrade) * texture2D.height;
					float currProbability = BuffGradeRateFormula(currLevelPercent, buffGrade) * texture2D.height;

					int x0 = Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(prevLevelPercent) * texture2D.width), 0, texture2D.width - 1);
					int x1 = Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(currLevelPercent) * texture2D.width), 0, texture2D.width - 1);
					int y0 = Mathf.Clamp(Mathf.RoundToInt(prevProbability), 0, texture2D.height - 1);
					int y1 = Mathf.Clamp(Mathf.RoundToInt(currProbability), 0, texture2D.height - 1);

					bresenham(texture2D, x0, y0, x1, y1, Color.green);
					bresenham(texture2D, x0, Mathf.Clamp(y0 - 1, 0, texture2D.height - 1), x1, Mathf.Clamp(y1 - 1, 0, texture2D.height - 1), Color.green);
					bresenham(texture2D, x0, Mathf.Clamp(y0 + 1, 0, texture2D.height - 1), x1, Mathf.Clamp(y1 + 1, 0, texture2D.height - 1), Color.green);

					buffGradeInfoItem.SetBuffProbability(currLevel, currProbability);
				}

				int curLevel = (Application.isPlaying ? PlayerManager.Instance.currentLevel : m_CurrentLevel);
				float probability = buffGradeInfoItem.GetBuffProbability(curLevel);
				int probabilityInt = Mathf.Clamp(Mathf.RoundToInt(probability), 0, texture2D.height - 1);

				drawCircle(texture2D, curLevel, probabilityInt, 3, Color.red);

				texture2D.Apply();

				buffGradeInfoItem.OnValidate(m_CurrentLevel);
				m_BuffGradeInfoMap[buffGrade] = buffGradeInfoItem;
			}

			void bresenham(Texture2D texture2D, int x0, int y0, int x1, int y1, Color color)
			{
				int dx = Mathf.Abs(x1 - x0);
				int dy = Mathf.Abs(y1 - y0);

				int sx = (x0 < x1) ? 1 : -1;
				int sy = (y0 < y1) ? 1 : -1;

				int err = dx - dy;

				while (true)
				{
					InfiniteLoopDetector.Run();
					texture2D.SetPixel(x0, y0, color);

					if (x0 == x1 && y0 == y1)
						break;

					int e2 = err * 2;
					if (e2 > -dy)
					{
						err -= dy;
						x0 += sx;
					}
					if (e2 < dx)
					{
						err += dx;
						y0 += sy;
					}
				}
			}
			void drawCircle(Texture2D texture2D, int cx, int cy, int r, Color color)
			{
				int x, y;
				float pi = Mathf.PI;
				float rad, drad;

				drad = pi / 360 * 10f;

				for (rad = 0; rad <= 2 * pi; rad += drad)
				{
					x = Mathf.Clamp(Mathf.RoundToInt(cx + r * Mathf.Cos(rad)), 0, texture2D.width - 1);
					y = Mathf.Clamp(Mathf.RoundToInt(cy + r * Mathf.Sin(rad)), 0, texture2D.height - 1);

					bresenham(texture2D, cx, cy, x, y, color);
				}
			}
		}
		private float BuffGradeRateFormula(float t, E_BuffGrade grade)
		{
			const int maxGrade = (int)E_BuffGrade.Max;

			const float PI2 = 2 * Mathf.PI;
			float width = m_Width + m_BuffGradeInfoMap[grade].width;
			float height = m_Height + m_BuffGradeInfoMap[grade].height;
			float offset = m_Offset + m_BuffGradeInfoMap[grade].offset;

			// 대충 직접 만든 공식
			// 그래프 그려볼 수 있는 사이트: https://www.desmos.com/calculator/kihjluohq9?lang=ko
			float value = height * Mathf.Cos(PI2 * (t - offset - (float)grade / maxGrade) / width);

			return Mathf.Clamp01(value);
		}
		// 퍼센트 값을 개별 등급의 퍼센트에서 전체 통합 퍼센트로 변경해주는 함수
		private void UpdateBuffGradePercent()
		{
			float[] minRates = new float[(int)E_BuffGrade.Max];
			float[] maxRates = new float[(int)E_BuffGrade.Max];
			float[] currentRates = new float[(int)E_BuffGrade.Max];

			float minTotal = 0f;
			float maxTotal = 0f;
			float currentTotal = 0f;

			int grade_index;

			const int minLevel = 1;

			for (E_BuffGrade grade = E_BuffGrade.Normal; grade < E_BuffGrade.Max; ++grade)
			{
				minTotal += m_BuffGradeInfoMap[grade].GetBuffProbability(minLevel);
				maxTotal += m_BuffGradeInfoMap[grade].GetBuffProbability(maxLevel);
				currentTotal += m_BuffGradeInfoMap[grade].GetBuffProbability(m_CurrentLevel);
			}

			for (E_BuffGrade grade = 0; grade < E_BuffGrade.Max; ++grade)
			{
				grade_index = (int)grade;

				minRates[grade_index] = m_BuffGradeInfoMap[grade].GetBuffProbability(minLevel) / minTotal;
				maxRates[grade_index] = m_BuffGradeInfoMap[grade].GetBuffProbability(maxLevel) / maxTotal;
				currentRates[grade_index] = m_BuffGradeInfoMap[grade].GetBuffProbability(m_CurrentLevel) / currentTotal;

				BuffGradeInfoItem gradeInfoItem = m_BuffGradeInfoMap[grade];

				gradeInfoItem.minLevelPercent = minRates[grade_index] * 100f;
				gradeInfoItem.maxLevelPercent = maxRates[grade_index] * 100f;
				gradeInfoItem.currentLevelPercent = currentRates[grade_index] * 100f;

				m_BuffGradeInfoMap[grade] = gradeInfoItem;
			}
		}

		[PropertyOrder(0f)]
		[Button("초기화", ButtonSizes.Large)]
		private void InitBuffGradeInfo()
		{
			m_BuffGradeInfoMap = new Dictionary<E_BuffGrade, BuffGradeInfoItem>();

			for (E_BuffGrade buffGrade = E_BuffGrade.Normal; buffGrade < E_BuffGrade.Max; ++buffGrade)
			{
				m_BuffGradeInfoMap[buffGrade] = new BuffGradeInfoItem(buffGrade);
			}
		}

		[System.Serializable]
		private struct BuffGradeInfoItem
		{
			#region 변수
			[SerializeField]
			private float m_Width;
			[SerializeField]
			private float m_Height;
			[SerializeField]
			private float m_Offset;

			[Space(5)]
			[OdinSerialize]
			[PreviewField(150, ObjectFieldAlignment.Center, FilterMode = FilterMode.Point)]
			private Texture2D m_Texture;
			[OdinSerialize, HideInInspector]
			private Dictionary<int, float> m_BuffProbabilityMap;

			[SerializeField, Sirenix.OdinInspector.ReadOnly]
			[FoldoutGroup("Debug")]
			private E_BuffGrade m_BuffGrade;
			[SerializeField, Sirenix.OdinInspector.ReadOnly]
			[FoldoutGroup("Debug")]
			private float m_MinLevelPercent;
			[SerializeField, Sirenix.OdinInspector.ReadOnly]
			[FoldoutGroup("Debug")]
			private float m_MaxLevelPercent;
			[SerializeField, Sirenix.OdinInspector.ReadOnly]
			[FoldoutGroup("Debug")]
			private float m_CurrentLevelPercent;

			[OdinSerialize, HideInInspector]
			private int m_MinLevel;
			[OdinSerialize, HideInInspector]
			private int m_MaxLevel;
			#endregion

			#region 프로퍼티
			public float width => m_Width;
			public float height => m_Height;
			public float offset => m_Offset;
			public Texture2D texture => m_Texture;

			public float minLevelPercent { set => m_MinLevelPercent = value; }
			public float maxLevelPercent { set => m_MaxLevelPercent = value; }
			public float currentLevelPercent { set => m_CurrentLevelPercent = value; }
			#endregion

			public BuffGradeInfoItem(E_BuffGrade buffGrade)
			{
				m_BuffGrade = buffGrade;
				m_Width = 0f;
				m_Height = 0f;
				m_Offset = 0f;

				m_Texture = null;
				m_BuffProbabilityMap = new Dictionary<int, float>();

				m_MinLevelPercent = 0f;
				m_MaxLevelPercent = 0f;
				m_CurrentLevelPercent = 0f;

				m_MinLevel = int.MaxValue;
				m_MaxLevel = int.MinValue;
			}

			public Texture2D CreateTexture()
			{
				if (m_Texture == null)
					m_Texture = new Texture2D(100, 100, TextureFormat.RGBA32, false);

				for (int y = 0; y < m_Texture.height; ++y)
				{
					for (int x = 0; x < m_Texture.width; ++x)
					{
						m_Texture.SetPixel(x, y, Color.black);
					}
				}

				return m_Texture;
			}
			public float GetBuffProbability(int level)
			{
				if (m_BuffProbabilityMap.TryGetValue(level, out float probability) == false)
				{
					Debug.LogError(level.ToString() + "에 맞는 버프 획득 확률 없음");
					return 0f;
				}

				return probability;
			}
			public void SetBuffProbability(int level, float probability)
			{
				if (m_BuffProbabilityMap == null)
				{
					Debug.Log("new");
					m_BuffProbabilityMap = new Dictionary<int, float>();
				}

				m_BuffProbabilityMap[level] = probability;

				m_MinLevel = Mathf.Min(m_MinLevel, level);
				m_MaxLevel = Mathf.Max(m_MaxLevel, level);
			}

			public void OnValidate(int level)
			{
				m_MinLevelPercent = m_BuffProbabilityMap[m_MinLevel];
				m_MaxLevelPercent = m_BuffProbabilityMap[m_MaxLevel];
				m_CurrentLevelPercent = m_BuffProbabilityMap[Mathf.Clamp(level, m_MinLevel, m_MaxLevel)];
			}
		}
	}
}