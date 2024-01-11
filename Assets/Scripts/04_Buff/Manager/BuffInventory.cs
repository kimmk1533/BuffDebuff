using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BuffInventory : Singleton<BuffInventory>
{
	#region 변수
	[SerializeField]
	protected DoubleKeyDictionary<int, string, Buff> m_BuffInventory = null;

	#endregion

	#region 프로퍼티
	#region 인덱서
	public Buff this[int code]
	{
		get => m_BuffInventory[code];
		set => m_BuffInventory[code] = value;
	}
	public Buff this[string buffName]
	{
		get => m_BuffInventory[buffName];
		set => m_BuffInventory[buffName] = value;
	}
	#endregion
	#endregion

	#region 이벤트
	public event System.Action<Buff> onBuffAdded = null;
	public event System.Action<Buff> onBuffRemoved = null;
	#endregion

	#region 매니저
	private static BuffManager M_Buff => BuffManager.Instance;
	#endregion

	public virtual void Initialize()
	{
		// BuffList Init
		if (m_BuffInventory == null)
			m_BuffInventory = new DoubleKeyDictionary<int, string, Buff>();

	}
	public virtual void Finallize()
	{
		if (m_BuffInventory != null)
			m_BuffInventory.Clear();
	}

	// Buff Func
	public bool AddBuff(string buffName)
	{
		if (buffName == null || buffName == string.Empty)
			return false;

		BuffData buffData = M_Buff.GetBuffData(buffName);

		return this.AddBuff(buffData);
	}
	public bool AddBuff(BuffData buffData)
	{
		if (buffData == null)
			return false;

		if (m_BuffInventory.TryGetValue(buffData.code, out Buff buff) &&
			buff != null)
		{
			if (buff.count < buffData.maxStack)
			{
				++buff.count;
				onBuffAdded?.Invoke(buff);
			}
			else
			{
				Debug.Log("Buff Count is Max. title =" + buffData.title + ", maxStack = " + buffData.maxStack.ToString());

				return false;
			}

			return true;
		}

		buff = new Buff(buffData);

		m_BuffInventory.Add(buffData.code, buffData.title, buff);

		onBuffAdded?.Invoke(buff);

		return true;
	}
	public bool RemoveBuff(string buffName)
	{
		if (buffName == null || buffName == string.Empty)
			return false;

		BuffData buffData = M_Buff.GetBuffData(buffName);

		return this.RemoveBuff(buffData);
	}
	public bool RemoveBuff(BuffData buffData)
	{
		if (buffData == null)
			return false;

		if (m_BuffInventory.TryGetValue(buffData.code, out Buff buff) &&
			buff != null)
		{
			if (buff.count > 0)
			{
				--buff.count;
			}
			else
			{
				m_BuffInventory.Remove(buffData.title);
			}

			onBuffRemoved?.Invoke(buff);

			return true;
		}

		Debug.Log("버프 없는데 제거");

		return false;
	}

	public bool HasBuff(int code) => m_BuffInventory.ContainsKey1(code);
	public bool HasBuff(string buffName) => m_BuffInventory.ContainsKey2(buffName);
	public bool HasBuff(BuffData buffData) => m_BuffInventory.ContainsKey1(buffData.code);

	public bool TryGetBuff(int code, out Buff buff) => m_BuffInventory.TryGetValue(code, out buff);
	public bool TryGetBuff(string buffName, out Buff buff) => m_BuffInventory.TryGetValue(buffName, out buff);
}