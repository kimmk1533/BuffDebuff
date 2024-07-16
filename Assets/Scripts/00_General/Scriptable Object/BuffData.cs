using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BuffDebuff.Enum;

namespace BuffDebuff
{
	//[CreateAssetMenu(fileName = "New Buff Data", menuName = "Scriptable Object/Buff Data", order = int.MinValue)]
	public class BuffData : SOData
	{
		#region 변수
		[SerializeField, ReadOnly]
		private int m_Code;
		[SerializeField, ReadOnly]
		private string m_Title;
		[SerializeField, ReadOnly]
		private E_BuffType m_BuffType;
		[SerializeField, ReadOnly]
		private E_BuffEffectType m_BuffEffectType;
		[SerializeField, ReadOnly]
		private E_BuffGrade m_BuffGrade;
		[SerializeField, ReadOnly]
		private int m_MaxStack;
		[SerializeField, ReadOnly]
		private E_BuffWeapon m_BuffWeapon;
		[SerializeField, ReadOnly]
		private E_BuffInvokeCondition m_BuffInvokeCondition;
		[SerializeField, ReadOnly]
		private float m_BuffValue;
		[SerializeField, ReadOnly]
		private float m_BuffTime;
		[SerializeField, ReadOnly, TextArea]
		private string m_Description;
		[SerializeField, ReadOnly]
		private Sprite m_Sprite;
		#endregion

		#region 프로퍼티
		public int code => m_Code;
		public string title => m_Title;
		public E_BuffType buffType => m_BuffType;
		public E_BuffEffectType buffEffectType => m_BuffEffectType;
		public E_BuffGrade buffGrade => m_BuffGrade;
		public int maxStack => m_MaxStack;
		public E_BuffWeapon buffWeapon => m_BuffWeapon;
		public E_BuffInvokeCondition buffInvokeCondition => m_BuffInvokeCondition;
		public float buffValue => m_BuffValue;
		public float buffTime => m_BuffTime;
		public string description => m_Description;
		public Sprite sprite => m_Sprite;
		#endregion

		public void Initialize(string _title, int _code, E_BuffType _buffType, E_BuffEffectType _buffEffectType, E_BuffGrade _buffGrade, int _maxStack, E_BuffWeapon _buffWeapon, E_BuffInvokeCondition _buffInvokeCondition, float _value, float _time, string _description, Sprite _sprite)
		{
			m_Title = _title;
			m_Code = _code;
			m_BuffType = _buffType;
			m_BuffEffectType = _buffEffectType;
			m_BuffGrade = _buffGrade;
			m_MaxStack = _maxStack;
			m_BuffWeapon = _buffWeapon;
			m_BuffInvokeCondition = _buffInvokeCondition;
			m_BuffValue = _value;
			m_BuffTime = _time;
			m_Description = _description;
			m_Sprite = _sprite;
		}
	}
}