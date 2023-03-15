using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GreenerGames;
using GoogleSheetsToUnity;

public sealed class BuffManager : Singleton<BuffManager>
{
	[SerializeField, ReadOnly(true)]
	private string m_AssociatedSheet;

	public List<WorkSheetData> m_WorkSheetDatas;

	// < 코드, 명칭, 버프 >
	Dictionary<E_BuffType, SecondaryKeyDictionary<int, string, Buff>> m_BuffDictionary;
#if UNITY_EDITOR
	[SerializeField]
	DebugDictionary<E_BuffType, BuffList> Debug_BuffDictionary;

	[System.Serializable]
	public struct BuffList
	{
		public List<Buff> buffs;

		public BuffList(int a)
		{
			buffs = new List<Buff>();
		}
	}
#endif

	public List<List<int>> m_Test;

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
		m_BuffDictionary = new Dictionary<E_BuffType, SecondaryKeyDictionary<int, string, Buff>>();

		for (E_BuffType i = 0; i < E_BuffType.Max; ++i)
		{
			m_BuffDictionary[i] = new SecondaryKeyDictionary<int, string, Buff>();
		}

#if UNITY_EDITOR
		//Debug_BuffDictionary = new DebugDictionary<E_BuffType, List<Buff>>();
		Debug_BuffDictionary = new DebugDictionary<E_BuffType, BuffList>();

		for (E_BuffType i = 0; i < E_BuffType.Max; ++i)
		{
			Debug_BuffDictionary[i] = new BuffList(0);
		}
#endif
	}

	[ContextMenu("LoadBuff")]
	private void LoadBuffAll()
	{
		Initialize();

		foreach (var item in m_WorkSheetDatas)
		{
			string titleColumn = item.StartCell.column;
			int titleRow = item.StartCell.row;
			GSTU_Search search = new GSTU_Search(m_AssociatedSheet, item.WorkSheetName, item.StartCell, item.EndCell, titleColumn, titleRow);

			SpreadsheetManager.Read(search, LoadBuffCallBack);
		}
	}
	private void LoadBuffCallBack(GstuSpreadSheet spreadSheet)
	{
		LoadBuff(spreadSheet);

		if (Application.isPlaying)
			CodeBuff();
	}
	private void LoadBuff(GstuSpreadSheet spreadSheet)
	{
		Debug.Log("Load Buff is Started.");

		bool first = true;
		foreach (var item in spreadSheet.rows.primaryDictionary)
		{
			if (first)
			{
				first = false;
				continue;
			}

			var list = item.Value;

			// 명칭
			string name = list[0].value;

			// 코드
			int.TryParse(list[1].value, out int code);

			// 버프 종류
			string typeStr = list[2].value;
			switch (typeStr)
			{
				case "버프":
					typeStr = "Buff";
					break;
				case "디버프":
					typeStr = "Debuff";
					break;
				case "양면버프":
					typeStr = "Bothbuff";
					break;
			}
			System.Enum.TryParse(typeStr, out E_BuffType type);

			// 효과 종류
			string effectTypeStr = list[3].value;
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
			}
			System.Enum.TryParse(effectTypeStr, out E_BuffEffectType effectType);

			// 등급
			System.Enum.TryParse(list[4].value, out E_BuffGrade grade);

			// 최대 스택
			int.TryParse(list[5].value, out int maxStack);

			// 적용 무기
			string weaponStr = list[6].value;
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
			}
			System.Enum.TryParse(weaponStr, out E_BuffWeapon weapon);

			// 설명
			string description = list[8].value;

			BuffData buffData = new BuffData(name, code, type, effectType, grade, maxStack, weapon, description);
			Buff buff = new Buff(buffData);

			m_BuffDictionary[type].Add(code, buff, name);

#if UNITY_EDITOR
			//Debug_BuffDictionary[type].Add(buff);
			Debug_BuffDictionary[type].buffs.Add(buff);
#endif
		}

		Debug.Log("Load Buff is Completed.");
	}
	private void CodeBuff()
	{
		//#region 001. 체력 증가
		//m_BuffDictionary["체력 증가"]
		//	.OnBuffInitialize.OnBuffEvent += (Character character) =>
		//	{
		//		character.m_CurrentStat.MaxHP += 100f;
		//		//character.m_BuffStat.MaxHP += 100f;
		//	};
		//m_BuffDictionary["체력 증가"]
		//	.OnBuffFinalize.OnBuffEvent += (Character character) =>
		//	{
		//		character.m_CurrentStat.MaxHP -= 100f;
		//		//character.m_BuffStat.MaxHP -= 100f;
		//	};
		//#endregion

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
			buff = GetBuff(i, key);

			if (buff != null)
				break;
		}

		return buff;
	}
	public Buff GetBuff(E_BuffType buffType, int key)
	{
		return m_BuffDictionary[buffType][key];
	}
	public Buff GetBuff(string key)
	{
		Buff buff = null;

		for (E_BuffType i = 0; i < E_BuffType.Max; ++i)
		{
			buff = GetBuff(i, key);

			if (buff != null)
				break;
		}

		return buff;
	}
	public Buff GetBuff(E_BuffType buffType, string key)
	{
		return m_BuffDictionary[buffType][key];
	}

	[System.Serializable]
	public struct WorkSheetData
	{
		public string WorkSheetName;

		public readonly E_BuffType BuffType;

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
}