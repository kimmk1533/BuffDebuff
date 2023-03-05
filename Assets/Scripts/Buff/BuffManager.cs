using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using static BuffData;

public class BuffManager : Singleton<BuffManager>
{
	public string m_AssociatedSheet = "1vDHbPcXSj3IUdHfGhmYbG7bse7FmDJKVRJ8DDd-slDQ";
	public string m_AssociatedWorkSheet = "버프 목록";

	// [ 코드, 버프 ]
	public Dictionary<int, BaseBuff> m_BuffDictionary;
	public List<BaseBuff> m_BuffList = new List<BaseBuff>();

	private void Awake()
	{
		Initialize();

		int code;
		BaseBuff buff = null;

		/*
		 * ↓ 엑셀에서 읽어온 데이터로 딕셔너리에 버프 생성 (반복문)
		 */

		code = 101001;
		buff = new BaseBuff("체력 증가", code, E_Grade.Normal, E_Condition.All, "체력 고정수치 증가");
		m_BuffDictionary.Add(code, buff);

		code = 101002;
		buff = new BaseBuff("5초당 체력 회복", code, E_Grade.Normal, E_Condition.All, "5초당 체력 회복");
		m_BuffDictionary.Add(code, buff);

		/*
		 * ↑ 임시
		 */

		// 001. 체력 증가
		{
			m_BuffDictionary[101001]
				.OnBuffInitialize.OnBuffEvent += (ref Character character) =>
			{
				character.m_BuffStat.MaxHP += 100;
			};
		}

		// 002. 5초 당 체력 회복
		{
			UtilsClass.Timer timer = new UtilsClass.Timer(5f);
			m_BuffDictionary[101002]
				.OnBuffUpdate.OnBuffEvent += (ref Character character) =>
				{
					if (timer.Update())
					{
						character.m_CurrentStat.HP += 10;
					}
				};
		}
	}

	private void Initialize()
	{
		if (m_BuffDictionary == null)
		{
			m_BuffDictionary = new Dictionary<int, BaseBuff>();
		}
	}

	[ContextMenu("LoadBuff")]
	public void LoadBuff()
	{
		SpreadsheetManager.Read(new GSTU_Search(m_AssociatedSheet, m_AssociatedWorkSheet, "C1", "G44", "C", 1), UpdateMethodOne);
	}
	private void UpdateMethodOne(GstuSpreadSheet spreadSheet)
	{
		m_BuffList.Clear();

		foreach (var item in spreadSheet.rows.primaryDictionary)
		{
			var list = item.Value;

			string name = list[0].value;
			int.TryParse(list[1].value, out int code);
			System.Enum.TryParse(list[2].value, out E_Grade grade);
			string conditionStr = list[3].value;
			switch (conditionStr)
			{
				case "공통":
					conditionStr = "All";
					break;
				case "근거리":
					conditionStr = "Melee";
					break;
				case "원거리":
					conditionStr = "Ranged";
					break;
			}
			System.Enum.TryParse(conditionStr, out E_Condition condition);
			string description = list[4].value;

			BaseBuff buff = new BaseBuff(name, code, grade, condition, description);

			m_BuffList.Add(buff);
		}

		m_BuffList.RemoveAt(0);
		Debug.Log("Load Buff is Completed.");
	}
}