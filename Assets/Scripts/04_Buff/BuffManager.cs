using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.IO;
using UnityEngine;
using BuffDebuff.Enum;
using BuffTypeGrade = System.Tuple<BuffDebuff.Enum.E_BuffType, BuffDebuff.Enum.E_BuffGrade>;

namespace BuffDebuff
{
	[RequireComponent(typeof(BuffInventory))]
	public class BuffManager : SerializedSingleton<BuffManager>
	{
		#region 기본 템플릿
		#region 변수
		[Space(10)]
		[SerializeField, InlineEditor]
		private BuffGradeInfo m_BuffGradeInfo = null;

		// < 코드, 명칭, 버프 >
		private DoubleKeyDictionary<int, string, BuffData> m_BuffDataMap = null;
		// < 타입, 등급, 코드 >
		private Dictionary<BuffTypeGrade, List<int>> m_BuffTypeGradeMap = null;

		private BuffInventory m_Inventory = null;
		#endregion

		#region 프로퍼티
		public BuffInventory inventory => m_Inventory;
		#endregion

		#region 이벤트

		#region 이벤트 함수
		#endregion
		#endregion

		#region 매니저
		private static PlayerManager M_Player => PlayerManager.Instance;
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 초기화 함수 (Init Scene 진입 시, 즉 게임 실행 시 호출)
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			// m_BuffGradeInfo
			if (m_BuffGradeInfo == null)
				m_BuffGradeInfo = new BuffGradeInfo();

			// m_BuffMap
			if (m_BuffDataMap == null)
				m_BuffDataMap = new DoubleKeyDictionary<int, string, BuffData>();

			// m_BuffTypeGradeMap
			if (m_BuffTypeGradeMap == null)
				m_BuffTypeGradeMap = new Dictionary<BuffTypeGrade, List<int>>();

			this.NullCheckGetComponent<BuffInventory>(ref m_Inventory);

			LoadAllBuffData();
		}
		/// <summary>
		/// 마무리화 함수 (게임 종료 시 호출)
		/// </summary>
		public override void Finallize()
		{
			base.Finallize();


		}

		/// <summary>
		/// 메인 초기화 함수 (본인 Main Scene 진입 시 호출)
		/// </summary>
		public override void InitializeMain()
		{
			base.InitializeMain();

			m_BuffGradeInfo.UpdateBuffGradeTexture();

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

			// 버프 인벤토리 초기화
			m_Inventory.Initialize();
		}
		/// <summary>
		/// 메인 마무리화 함수 (본인 Main Scene 나갈 시 호출)
		/// </summary>
		public override void FinallizeMain()
		{
			base.FinallizeMain();

			// 버프 인벤토리 마무리화
			m_Inventory.Finallize();
		}
		#endregion

		#region 유니티 콜백 함수
		#endregion
		#endregion

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
			string path = Path.Combine(BuffSOManager.resourcesPath, buffType.ToString());
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
		//public void OnValidate()
		//{
		//	if (Application.isPlaying == true)
		//		return;

		//	if (m_BuffGradeInfo != null)
		//		m_BuffGradeInfo.OnValidate();
		//}
#endif
	}
}