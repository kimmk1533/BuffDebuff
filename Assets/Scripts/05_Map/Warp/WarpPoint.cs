using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPoint : MonoBehaviour
{
	private static readonly Color colliderColor = new Color(145 / 255f, 244 / 255f, 139 / 255f, 192 / 255f);

	public enum E_Direction
	{
		Up,
		Down,
		Left,
		Right,

		Max
	}

	private Room m_Room;

	[SerializeField]
	private Vector2 m_Offset;
	[SerializeField, Min(0.0001f)]
	private Vector2 m_Size = Vector2.one;

	[Space(10)]
	[SerializeField]
	private E_Direction m_Direction;

	public E_Direction direction => m_Direction;

	public void Initialize(Room room)
	{
		m_Room = room;
	}

	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;

		Gizmos.color = colliderColor;

		Gizmos.DrawWireCube(transform.position + (Vector3)m_Offset, m_Size);

		Gizmos.color = color;
	}
}