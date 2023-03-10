using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct BuffData
{
	[SerializeField, ReadOnly]
	private string m_Name;
	[SerializeField, ReadOnly]
	private int m_Code;
	[SerializeField, ReadOnly]
	private E_BuffType m_Type;
	[SerializeField, ReadOnly]
	private E_BuffEffectType m_EffectType;
	[SerializeField, ReadOnly]
	private E_BuffGrade m_Grade;
	[SerializeField, ReadOnly]
	private int m_MaxStack;
	[SerializeField, ReadOnly]
	private E_BuffWeapon m_Weapon;
	[SerializeField, ReadOnly]
	private string m_Description;

	public string name
	{
		get { return m_Name; }
	}
	public int code
	{
		get { return m_Code; }
	}
	public E_BuffType type
	{
		get { return m_Type; }
	}
	public E_BuffEffectType effectType
	{
		get { return m_EffectType; }
	}
	public E_BuffGrade grade
	{
		get { return m_Grade; }
	}
	public int maxStack
	{
		get { return m_MaxStack; }
	}
	public E_BuffWeapon weapon
	{
		get { return m_Weapon; }
	}
	public string description
	{
		get { return m_Description; }
	}

	public BuffData(string _name, int _code, E_BuffType _type, E_BuffEffectType _effectType, E_BuffGrade _grade, int _maxStack, E_BuffWeapon _weapon, string _description)
	{
		m_Name = _name;
		m_Code = _code;
		m_Type = _type;
		m_EffectType = _effectType;
		m_Grade = _grade;
		m_MaxStack = _maxStack;
		m_Weapon = _weapon;
		m_Description = _description;
	}
}

#region PropertyDrawer
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BuffData))]
public class BuffDataDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight(property, label) * 12.5f;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		//base.OnGUI(position, property, label);
		var name = property.FindPropertyRelative("m_Name");
		var code = property.FindPropertyRelative("m_Code");
		var type = property.FindPropertyRelative("m_Type");
		var effectType = property.FindPropertyRelative("m_EffectType");
		var grade = property.FindPropertyRelative("m_Grade");
		var weapon = property.FindPropertyRelative("m_Weapon");
		var description = property.FindPropertyRelative("m_Description");

		EditorGUI.BeginProperty(position, label, property);
		{
			Rect pos = position;
			pos.height = EditorGUIUtility.singleLineHeight;

			name.stringValue = EditorGUI.TextField(pos, "??????", name.stringValue);
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			code.intValue = EditorGUI.IntField(pos, "??????", code.intValue);
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			E_BuffType e_Type = (E_BuffType)type.enumValueIndex;
			e_Type = (E_BuffType)EditorGUI.EnumPopup(pos, "??????", e_Type);
			type.enumValueIndex = (int)e_Type;
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			E_BuffEffectType e_EffectType = (E_BuffEffectType)effectType.enumValueIndex;
			e_EffectType = (E_BuffEffectType)EditorGUI.EnumPopup(pos, "?????? ??????", e_EffectType);
			effectType.enumValueIndex = (int)e_EffectType;
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			E_BuffGrade e_Grade = (E_BuffGrade)grade.enumValueIndex;
			e_Grade = (E_BuffGrade)EditorGUI.EnumPopup(pos, "??????", e_Grade);
			grade.enumValueIndex = (int)e_Grade;
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			E_BuffWeapon e_Weapon = (E_BuffWeapon)weapon.enumValueIndex;
			e_Weapon = (E_BuffWeapon)EditorGUI.EnumPopup(pos, "?????? ??????", e_Weapon);
			weapon.enumValueIndex = (int)e_Weapon;
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			EditorGUI.LabelField(pos, "??????");
			pos.y += EditorGUIUtility.singleLineHeight + 5f;
			pos.height = (pos.height * 3f) + 5f;
			description.stringValue = EditorGUI.TextArea(pos, description.stringValue, "TextArea");
		}
		EditorGUI.EndProperty();
	}
}
#endif
#endregion

#region enum
public enum E_BuffType
{
	Buff,
	Debuff,

	Max
}
public enum E_BuffEffectType
{
	Stat,
	Weapon,
	Combat,

	Max
}
public enum E_BuffGrade
{
	Normal = 0,
	Uncommon,
	Rare,
	Unique,
	Epic,
	Legendary,
	GOD,

	Max
}
public enum E_BuffWeapon
{
	All,
	Melee,
	Ranged,

	Max
}
public enum E_BuffInvokeCondition
{
	Initialize,		// ????????? ?????? ???
	Finalize,		// ????????? ?????? ???
	Update,			// ?????? ????????????
	Jump,			// ?????? ???
	Dash,			// ?????? ???
	GiveDamage,		// ?????? ???
	GetDamage,		// ?????? ???
	AttackStart,	// ?????? ?????? ???
	Attack,			// ?????? ???
	AttackEnd,		// ?????? ?????? ???
	KillEnemy,		// ??? ?????? ???

	Max
}
#endregion

#region interface
public interface IOnBuff
{

}
// ????????? ?????? ??????
public interface IOnBuffStart : IOnBuff
{
	public void OnBuffStart(ref Character character);
}
// ??? ???????????????
public interface IOnBuffUpdate : IOnBuff
{
	public void OnBuffUpdate();
}
// ???????????? ???
public interface IOnBuffJump : IOnBuff
{
	public void OnBuffJump();
}
// ???????????? ???
public interface IOnBuffDash : IOnBuff
{
	public void OnBuffDash();
}
// ???????????? ?????? ???
public interface IOnBuffGetDamage : IOnBuff
{
	public void OnBuffGetDamage();
}
// ?????? ????????? ???
public interface IOnBuffAttackStart : IOnBuff
{
	public void OnBuffAttackStart();
}
// ???????????? ??? ???
public interface IOnBuffGiveDamage : IOnBuff
{
	public void OnBuffGiveDamage();
}
// ????????? ?????? ???
public interface IOnBuffAttackEnd : IOnBuff
{
	public void OnBuffAttackEnd();
}
#endregion