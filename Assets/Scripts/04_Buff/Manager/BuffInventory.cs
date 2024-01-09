using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffInventory : MonoBehaviour
{
	#region 변수
	[SerializeField]
	protected DoubleKeyDictionary<int, string, AbstractBuff> m_BuffList = null;

	#endregion

	#region 프로퍼티
	#endregion

	#region 이벤트
	public event System.Action<AbstractBuff> onBuffAdded = null;
	public event System.Action<AbstractBuff> onBuffRemoved = null;
	#endregion

	#region 매니저
	private static BuffManager M_Buff => BuffManager.Instance;
	#endregion

	public virtual void Initialize()
	{
		// BuffList Init
		if (m_BuffList == null)
			m_BuffList = new DoubleKeyDictionary<int, string, AbstractBuff>();

	}
	public virtual void Finallize()
	{
		if (m_BuffList != null)
			m_BuffList.Clear();
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

		if (m_BuffList.TryGetValue(buffData.code, out AbstractBuff buff) &&
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

		buff = M_Buff.CreateBuff(buffData);

		m_BuffList.Add(buffData.code, buffData.title, buff);

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

		if (m_BuffList.TryGetValue(buffData.code, out AbstractBuff buff) &&
			buff != null)
		{
			if (buff.count > 0)
			{
				--buff.count;
			}
			else
			{
				m_BuffList.Remove(buffData.title);
			}

			onBuffRemoved?.Invoke(buff);

			return true;
		}

		Debug.Log("버프 없는데 제거");

		return false;
	}

	public bool Contains(string buffName)
	{
		return m_BuffList.ContainsKey2(buffName);
	}
}