using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Scriptable Object/Buff")]
public class SO_Buff : ScriptableObject
{
	[SerializeField]
	private BuffInfo m_Info;
}

[System.Serializable]
struct BuffInfo
{
	public string Name;
	public int Code;
	public E_BuffGrade Grade;
	public string Description;
}

enum E_BuffGrade
{
	Normal = 0,
	Uncommon,
	Rare,
	Unique,
	Epic,
	Legendary,
	God,

	Max
}