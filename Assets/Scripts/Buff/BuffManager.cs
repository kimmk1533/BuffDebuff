using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GreenerGames;
using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using static BuffData;

public class BuffManager : Singleton<BuffManager>
{
	public string m_AssociatedSheet = "1vDHbPcXSj3IUdHfGhmYbG7bse7FmDJKVRJ8DDd-slDQ";
	public string m_AssociatedWorkSheet = "버프 목록";

	public string m_StartCell = "C1";
	public string m_EndCell = "J55";

	// [ 코드, 버프 ]
	public SecondaryKeyDictionary<int, string, Buff> m_BuffDictionary;
#if UNITY_EDITOR
	[SerializeField, ReadOnly]
	private DebugDictionary<string, int> m_Debug1;
	[SerializeField, ReadOnly]
	private DebugDictionary<int, Buff> m_Debug2;
#endif

	private void Awake()
	{
		Initialize();

		// 엑셀에서 읽어온 데이터로 딕셔너리에 버프 생성
		LoadBuffAll();
	}

	private void Initialize()
	{
		if (m_BuffDictionary == null)
		{
			m_BuffDictionary = new SecondaryKeyDictionary<int, string, Buff>();
		}
		else
		{
			m_BuffDictionary.primaryDictionary.Clear();
			m_BuffDictionary.secondaryKeyLink.Clear();
		}

#if UNITY_EDITOR
		if (m_Debug1 == null)
		{
			m_Debug1 = new DebugDictionary<string, int>();
		}
		else
		{
			m_Debug1.Clear();
		}
		if (m_Debug2 == null)
		{
			m_Debug2 = new DebugDictionary<int, Buff>();
		}
		else
		{
			m_Debug2.Clear();
		}
#endif
	}

	[ContextMenu("LoadBuff")]
	private void LoadBuffAll()
	{
		Initialize();

		string titleColumn = m_StartCell.Substring(0, 1);
		int titleRow = int.Parse(m_StartCell.Substring(1, m_StartCell.Length - 1));
		GSTU_Search search = new GSTU_Search(m_AssociatedSheet, m_AssociatedWorkSheet, m_StartCell, m_EndCell, titleColumn, titleRow);

		SpreadsheetManager.Read(search, LoadBuffCallBack);
	}
	private void LoadBuffCallBack(GstuSpreadSheet spreadSheet)
	{
		LoadBuff(spreadSheet);

		/*
		 * 버프 구현
		 */
		{
			// 001. 체력 증가
			{
				m_BuffDictionary["체력 증가"]
					.OnBuffInitialize.OnBuffEvent += (ref Character character) =>
					{
						character.m_BuffStat.MaxHP += 100;
					};
				m_BuffDictionary["체력 증가"]
					.OnBuffFinalize.OnBuffEvent += (ref Character character) =>
					{
						character.m_BuffStat.MaxHP -= 100;
					};
			}

			// 002. 5초 당 체력 회복
			{
				UtilsClass.Timer timer = new UtilsClass.Timer(5f);
				m_BuffDictionary["초 당 체력 회복"]
					.OnBuffUpdate.OnBuffEvent += (ref Character character) =>
					{
						if (timer.Update())
						{
							character.m_CurrentStat.HP += 10;
						}
					};
			}
		}
		/*
		 * 버프 구현 끝
		 */
	}
	private void LoadBuff(GstuSpreadSheet spreadSheet)
	{
		bool first = true;
		foreach (var item in spreadSheet.rows.primaryDictionary)
		{
			if (first)
			{
				first = false;
				continue;
			}

			var list = item.Value;

			string typeStr = list[2].value;
			switch (typeStr)
			{
				case "버프":
					typeStr = "Buff";
					break;
				case "디버프":
					typeStr = "Debuff";
					break;
			}
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
			string targetStr = list[5].value;
			switch (targetStr)
			{
				case "공통":
					targetStr = "All";
					break;
				case "근거리 무기":
					targetStr = "Melee";
					break;
				case "원거리 무기":
					targetStr = "Ranged";
					break;
			}

			string name = list[0].value;
			int.TryParse(list[1].value, out int code);
			System.Enum.TryParse(typeStr, out E_BuffType type);
			System.Enum.TryParse(effectTypeStr, out E_BuffEffectType effectType);
			System.Enum.TryParse(list[4].value, out E_BuffGrade grade);
			System.Enum.TryParse(targetStr, out E_BuffWeapon target);
			string description = list[7].value;

			BuffData buffData = new BuffData(name, code, type, effectType, grade, target, description);
			Buff buff = new Buff(buffData);

			m_BuffDictionary.Add(code, buff, name);
#if UNITY_EDITOR
			m_Debug1.Add(name, code);
			m_Debug2.Add(code, buff);
#endif
		}

		Debug.Log("Load Buff is Completed.");
	}

	
}