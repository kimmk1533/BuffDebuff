using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : Singleton<BuffManager>
{
	public Dictionary<int, BaseBuff> m_BuffDictionary;

	private void Awake()
	{
		m_BuffDictionary = new Dictionary<int, BaseBuff>();

		int code;
		BaseBuff buff = null;

		/*
		 * ↓ 엑셀에서 읽어온 데이터로 딕셔너리에 버프 생성 (반복문)
		 */

		code = 101001;
		buff = new BaseBuff("체력 증가", code, BaseBuff.E_Grade.Normal, "체력 고정수치 증가");
		m_BuffDictionary.Add(code, buff);

		code = 101002;
		buff = new BaseBuff("5초당 체력 회복", code, BaseBuff.E_Grade.Normal, "5초당 체력 회복");
		m_BuffDictionary.Add(code, buff);

		/*
		 * ↑ 임시
		 */

		// 001. 체력 증가
		{
			m_BuffDictionary[101001].OnBuffInitialize.OnBuffEvent += (ref Character character) =>
			{
				character.m_BuffStat.MaxHP += 100;
			};
		}

		// 002. 5초 당 체력 회복
		{
			UtilsClass.Timer timer = new UtilsClass.Timer(5f);
			m_BuffDictionary[101002].OnBuffUpdate.OnBuffEvent += (ref Character character) =>
			{
				if (timer.Update())
				{
					character.m_CurrentStat.HP += 10;
				}
			};
		}
	}

}