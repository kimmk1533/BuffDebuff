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
	string title = "버프 데이터";

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height =
			property.isExpanded ?
			EditorGUIUtility.singleLineHeight * 14f :
			0f;
		return base.GetPropertyHeight(property, label) + height;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		//base.OnGUI(position, property, label);
		var name = property.FindPropertyRelative("m_Name");
		var code = property.FindPropertyRelative("m_Code");
		var type = property.FindPropertyRelative("m_Type");
		var effectType = property.FindPropertyRelative("m_EffectType");
		var grade = property.FindPropertyRelative("m_Grade");
		var maxStack = property.FindPropertyRelative("m_MaxStack");
		var weapon = property.FindPropertyRelative("m_Weapon");
		var description = property.FindPropertyRelative("m_Description");

		Rect pos = position;
		pos.height = EditorGUIUtility.singleLineHeight;

		title = name.stringValue;
		property.isExpanded = EditorGUI.Foldout(pos, property.isExpanded, title, true);
		pos.y += EditorGUIUtility.singleLineHeight + 5f;

		++EditorGUI.indentLevel;

		if (property.isExpanded)
		{
			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 100f;

			EditorGUI.BeginProperty(position, label, property);
			{
				name.stringValue = EditorGUI.TextField(pos, "명칭", name.stringValue);
				if (name.stringValue != "")
					title = name.stringValue;
				else
					title = "버프 데이터";
				pos.y += EditorGUIUtility.singleLineHeight + 5f;

				code.intValue = EditorGUI.IntField(pos, "코드", code.intValue);
				pos.y += EditorGUIUtility.singleLineHeight + 5f;

				E_BuffType e_Type = (E_BuffType)type.enumValueIndex;
				e_Type = (E_BuffType)EditorGUI.EnumPopup(pos, "종류", e_Type);
				type.enumValueIndex = (int)e_Type;
				pos.y += EditorGUIUtility.singleLineHeight + 5f;

				E_BuffEffectType e_EffectType = (E_BuffEffectType)effectType.enumValueIndex;
				e_EffectType = (E_BuffEffectType)EditorGUI.EnumPopup(pos, "효과 종류", e_EffectType);
				effectType.enumValueIndex = (int)e_EffectType;
				pos.y += EditorGUIUtility.singleLineHeight + 5f;

				E_BuffGrade e_Grade = (E_BuffGrade)grade.enumValueIndex;
				e_Grade = (E_BuffGrade)EditorGUI.EnumPopup(pos, "등급", e_Grade);
				grade.enumValueIndex = (int)e_Grade;
				pos.y += EditorGUIUtility.singleLineHeight + 5f;

				maxStack.intValue = EditorGUI.IntField(pos, "최대 스택", maxStack.intValue);
				pos.y += EditorGUIUtility.singleLineHeight + 5f;

				E_BuffWeapon e_Weapon = (E_BuffWeapon)weapon.enumValueIndex;
				e_Weapon = (E_BuffWeapon)EditorGUI.EnumPopup(pos, "발동 무기", e_Weapon);
				weapon.enumValueIndex = (int)e_Weapon;
				pos.y += EditorGUIUtility.singleLineHeight + 5f;

				EditorGUI.LabelField(pos, "설명");
				pos.y += EditorGUIUtility.singleLineHeight + 5f;
				pos.height = (pos.height * 3f) + 5f;
				description.stringValue = EditorGUI.TextArea(pos, description.stringValue, "TextArea");
			}
			EditorGUI.EndProperty();

			EditorGUIUtility.labelWidth = labelWidth;
		}

		--EditorGUI.indentLevel;
	}
}
#endif
#endregion

#region enum
public enum E_BuffType
{
	Buff,
	Debuff,
	Bothbuff,

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
	Initialize,     // 버프를 얻을 때
	Finalize,       // 버프를 잃을 때
	Update,         // 일정 시간마다
	Jump,           // 점프 시
	Dash,           // 대쉬 시
	GiveDamage,     // 타격 시
	GetDamage,      // 피격 시
	AttackStart,    // 공격 시작 시
	Attack,         // 공격 시
	AttackEnd,      // 공격 종료 시
	KillEnemy,      // 적 처치 시

	Max
}
#endregion

#region interface
public interface IOnBuff
{

}
// 버프를 얻은 순간
public interface IOnBuffStart : IOnBuff
{
	public void OnBuffStart(ref Character character);
}
// 매 프레임마다
public interface IOnBuffUpdate : IOnBuff
{
	public void OnBuffUpdate();
}
// 점프했을 때
public interface IOnBuffJump : IOnBuff
{
	public void OnBuffJump();
}
// 대쉬했을 때
public interface IOnBuffDash : IOnBuff
{
	public void OnBuffDash();
}
// 대미지를 받을 때
public interface IOnBuffGetDamage : IOnBuff
{
	public void OnBuffGetDamage();
}
// 공격 시작할 때
public interface IOnBuffAttackStart : IOnBuff
{
	public void OnBuffAttackStart();
}
// 대미지를 줄 때
public interface IOnBuffGiveDamage : IOnBuff
{
	public void OnBuffGiveDamage();
}
// 공격을 끝낼 때
public interface IOnBuffAttackEnd : IOnBuff
{
	public void OnBuffAttackEnd();
}
#endregion