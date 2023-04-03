using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
	[SerializeField]
	Vector2 m_ClampOffset;
	[SerializeField]
	Vector2 m_ClampAreaSize;

	[SerializeField, ReadOnly]
	Tilemap m_TileMap;
	[SerializeField, ReadOnly]
	Tilemap m_ThroughMap;

	public Vector2 clampOffset
	{
		get { return m_ClampOffset; }
		set { m_ClampOffset = value; }
	}
	public Vector2 clampAreaSize
	{
		get { return m_ClampAreaSize; }
		set { m_ClampAreaSize = value; }
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0, 1, 0, 0.1f);
		Gizmos.DrawCube((Vector2)transform.position + m_ClampOffset, m_ClampAreaSize);
	}
}