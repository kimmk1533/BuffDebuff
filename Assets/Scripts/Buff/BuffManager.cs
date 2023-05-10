using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleSheetsToUnity;

public sealed class BuffManager : Singleton<BuffManager>
{
	[SerializeField, ReadOnly(true)]
	private string m_AssociatedSheet;

	public List<WorkSheetData> m_WorkSheetDatas;

	// < 코드, 명칭, 버프 >
	Dictionary<E_BuffType, BuffList> m_BuffDictionary;
#if UNITY_EDITOR
	[SerializeField]
	DebugDictionary<E_BuffType, DebugBuffList> Debug_BuffDictionary;

	[System.Serializable]
	public class DebugBuffList
	{
		[SerializeField, HideInInspector]
		private string m_Name;
		public string name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}
		public List<Buff> buffList = new List<Buff>();
	}
#endif

	private void Reset()
	{
		m_AssociatedSheet = "1vDHbPcXSj3IUdHfGhmYbG7bse7FmDJKVRJ8DDd-slDQ";
	}
	private void Awake()
	{
		Initialize();

		// 엑셀에서 읽어온 데이터로 딕셔너리에 버프 생성
		LoadBuffAll();
	}

	private void Initialize()
	{
		m_BuffDictionary = new Dictionary<E_BuffType, BuffList>();
#if UNITY_EDITOR
		Debug_BuffDictionary = new DebugDictionary<E_BuffType, DebugBuffList>();
#endif

		for (E_BuffType i = 0; i < E_BuffType.Max - 1; ++i)
		{
			m_BuffDictionary[i] = new BuffList();

#if UNITY_EDITOR
			Debug_BuffDictionary[i] = new DebugBuffList();
			Debug_BuffDictionary[i].name = i.ToString();
#endif
		}
	}

	[ContextMenu("LoadBuffAll")]
	private void LoadBuffAll()
	{
		float start = Time.realtimeSinceStartup;
		Debug.Log("Load Buff is Started.");

		Initialize();

		foreach (var item in m_WorkSheetDatas)
		{
			string titleColumn = item.StartCell.column;
			int titleRow = item.StartCell.row;
			GSTU_Search search = new GSTU_Search(m_AssociatedSheet, item.WorkSheetName, item.StartCell, item.EndCell, titleColumn, titleRow);

			SpreadsheetManager.Read(search, LoadBuff);
		}

		float end = Time.realtimeSinceStartup;
		Debug.Log("Load Buff is Completed. t = " + (end - start).ToString());
	}
	private void LoadBuff(GstuSpreadSheet spreadSheet)
	{
		bool first = true;

		var dict = spreadSheet.rows.primaryDictionary.Values;

		foreach (var item in dict)
		{
			// 첫 번째 줄 건너뛰기
			if (first)
			{
				first = false;
				continue;
			}

			#region 명칭
			// 명칭 불러오기
			string name = item[0].value;
			#endregion

			#region 코드
			// 코드 불러오기
			string codeStr = item[1].value;

			// 자료형 파싱
			if (int.TryParse(codeStr, out int code) == false)
			{
				Debug.LogError("버프 코드 불러오기 오류! | 코드: " + codeStr);
				return;
			}
			#endregion

			#region 버프 종류
			// 버프 종류 불러오기
			string typeStr = codeStr[0].ToString();

			// 자료형 파싱
			if (int.TryParse(typeStr, out int typeInt) == false)
			{
				Debug.LogError("버프 종류 전환 오류! | 버프 종류: " + typeStr);
				return;
			}

			E_BuffType type = (E_BuffType)(typeInt - 1);
			#endregion

			#region 효과 종류
			// 효과 종류 불러오기
			string effectTypeStr = item[2].value;

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
			string gradeStr = item[3].value;

			// 자료형 파싱
			if (System.Enum.TryParse(gradeStr, out E_BuffGrade grade) == false)
			{
				Debug.LogError("버프 등급 전환 오류! | 버프 등급: " + gradeStr);
				return;
			}
			#endregion

			#region 최대 스택
			// 최대 스택 불러오기
			string maxStackStr = item[4].value;

			// 자료형 파싱
			if (int.TryParse(maxStackStr, out int maxStack) == false)
			{
				Debug.LogError("버프 최대 스택 전환 오류! | 버프 최대 스택: " + maxStackStr);
				return;
			}
			#endregion

			#region 적용 무기 타입
			// 적용되는 무기 타입 불러오기
			string weaponStr = item[5].value;

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
			string description = item[7].value;
			#endregion

			#region 버프 생성
			BuffData buffData = new BuffData(name, code, type, effectType, grade, maxStack, weapon, description);
			Buff buff = new Buff(buffData);
			#endregion

			#region 버프 추가
			m_BuffDictionary[type].Add((code, name), buff);

#if UNITY_EDITOR
			//Debug_BuffDictionary[type].Add(buff);
			Debug_BuffDictionary[type].buffList.Add(buff);
#endif
			#endregion
		}

		if (Application.isPlaying)
			CodeBuff();
	}
	private void CodeBuff()
	{
		#region 001. 체력 증가
		m_BuffDictionary[E_BuffType.Buff]["체력 증가"]
			.OnBuffInitialize.OnBuffEvent += (Character character) =>
			{
				Character.CharacterStat buffStat = character.buffStat;
				buffStat.MaxHp += 100f;
				character.buffStat = buffStat;
			};
		m_BuffDictionary[E_BuffType.Buff]["체력 증가"]
			.OnBuffFinalize.OnBuffEvent += (Character character) =>
			{
				Character.CharacterStat buffStat = character.buffStat;
				buffStat.MaxHp -= 100f;
				character.buffStat = buffStat;
			};
		#endregion

		//#region 003. 재생
		//m_BuffDictionary["재생"]
		//	.OnBuffInitialize.OnBuffEvent += (Character character) =>
		//	{
		//		character.m_CurrentStat.HPregen = 10f;
		//	};
		//m_BuffDictionary["재생"]
		//	.OnBuffFinalize.OnBuffEvent += (Character character) =>
		//	{
		//		character.m_CurrentStat.HPregen = 0f;
		//	};
		//#endregion

		//#region 004. 빠른 재생
		//m_BuffDictionary["빠른 재생"]
		//	.OnBuffInitialize.OnBuffEvent += (Character character) =>
		//	{
		//		character.m_CurrentStat.HPregenCooldown -= 1f;
		//		character.m_HealTimer.interval = character.m_CurrentStat.HPregenCooldown;
		//	};
		//m_BuffDictionary["빠른 재생"]
		//	.OnBuffFinalize.OnBuffEvent += (Character character) =>
		//	{
		//		character.m_CurrentStat.HPregenCooldown += 1f;
		//		character.m_HealTimer.interval = character.m_CurrentStat.HPregenCooldown;
		//	};
		//#endregion

		//#region 005. 느린 재생
		//m_BuffDictionary["느린 재생"]
		//	.OnBuffInitialize.OnBuffEvent += (Character character) =>
		//	{
		//		character.m_CurrentStat.HPregenCooldown += 1f;
		//		character.m_HealTimer.interval = character.m_CurrentStat.HPregenCooldown;
		//	};
		//m_BuffDictionary["느린 재생"]
		//	.OnBuffFinalize.OnBuffEvent += (Character character) =>
		//	{
		//		character.m_CurrentStat.HPregenCooldown -= 1f;
		//		character.m_HealTimer.interval = character.m_CurrentStat.HPregenCooldown;
		//	};
		//#endregion
	}

	public Buff GetBuff(int key)
	{
		Buff buff = null;

		for (E_BuffType i = 0; i < E_BuffType.Max; ++i)
		{
			if (TryGetBuff(i, key, out buff) == true)
				break;
		}

		return buff;
	}
	public bool TryGetBuff(E_BuffType buffType, int key, out Buff buff)
	{
		BuffList buffDictionary = m_BuffDictionary[buffType];

		if (buffDictionary == null)
		{
			buff = null;

			Debug.LogError("버프 목록 생성 안됨");
			return false;
		}

		if (buffDictionary.TryGetValue(key, out buff))
		{
			buff = null;

			Debug.LogError("버프 가져오기 실패");
			return false;
		}

		return true;
	}
	public Buff GetBuff(string key)
	{
		Buff buff = null;

		for (E_BuffType i = 0; i < E_BuffType.Max; ++i)
		{
			if (TryGetBuff(i, key, out buff) == true)
				break;
		}

		return buff;
	}
	public bool TryGetBuff(E_BuffType buffType, string key, out Buff buff)
	{
		BuffList buffDictionary = m_BuffDictionary[buffType];

		if (buffDictionary == null)
		{
			buff = null;

			Debug.LogError("버프 목록 생성 안됨");
			return false;
		}

		if (buffDictionary.TryGetValue(key, out buff) == false)
		{
			buff = null;

			Debug.LogError("버프 가져오기 실패");
			return false;
		}

		return true;
	}

	[System.Serializable]
	public struct WorkSheetData
	{
		public string WorkSheetName;

		public Cell StartCell;
		public Cell EndCell;

		[System.Serializable]
		public struct Cell
		{
			public string column;
			public int row;

			public override string ToString()
			{
				if (row <= 0)
					return column;

				return column + row.ToString();
			}
			public static implicit operator string(Cell cell)
			{
				return cell.ToString();
			}
		}
	}
	public class BuffList
	{
		public Dictionary<string, int> m_NameDictionary = new Dictionary<string, int>();
		public Dictionary<int, Buff> m_BuffDictionary = new Dictionary<int, Buff>();

		public Buff this[int code]
		{
			get
			{
				if (m_BuffDictionary.TryGetValue(code, out Buff buff) == false)
					return null;

				return buff;
			}
			set
			{
				m_BuffDictionary[code] = value;
			}
		}
		public Buff this[string name]
		{
			get
			{
				if (m_NameDictionary.TryGetValue(name, out int code) == false)
					return null;

				return this[code];
			}
			set
			{
				if (m_NameDictionary.TryGetValue(name, out int code) == false)
					return;

				this[code] = value;
			}
		}

		public void Add((int code, string name) key, Buff buff)
		{
			m_NameDictionary.Add(key.name, key.code);
			m_BuffDictionary.Add(key.code, buff);
		}
		public void Add(int code, string name, Buff buff)
		{
			m_NameDictionary.Add(name, code);
			m_BuffDictionary.Add(code, buff);
		}
		public bool TryAdd((int code, string name) key, Buff buff)
		{
			if (m_NameDictionary.TryAdd(key.name, key.code) == false)
				return false;
			if (m_BuffDictionary.TryAdd(key.code, buff) == false)
				return false;

			return true;
		}
		public bool TryAdd(int code, string name, Buff buff)
		{
			if (m_NameDictionary.TryAdd(name, code) == false)
				return false;
			if (m_BuffDictionary.TryAdd(code, buff) == false)
				return false;

			return true;
		}
		public bool TryGetValue(int code, out Buff buff)
		{
			return m_BuffDictionary.TryGetValue(code, out buff);
		}
		public bool TryGetValue(string name, out Buff buff)
		{
			if (m_NameDictionary.TryGetValue(name, out int code) == false)
			{
				buff = default(Buff);
				return false;
			}

			return TryGetValue(code, out buff);
		}
	}
}