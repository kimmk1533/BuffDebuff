using System.Collections;
using System.Collections.Generic;
using Algorithms;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : Singleton<GridManager>
{
	#region 변수
	private Map m_Map;

	[Space(10)]
	[SerializeField]
	private bool m_ShowGrid;
	[SerializeField]
	private Color m_GridColor;

	[Space(10)]
	[SerializeField]
	private bool m_ShowGridText;
	[SerializeField]
	private Transform m_GridTextParent;
	[SerializeField]
	private Color m_GridTextColor;

	[Space(10)]
	[SerializeField]
	private Tilemap m_Tilemap;
	#endregion

	#region 프로퍼티
	public Map map => m_Map;
	#endregion

	#region 매니저
	protected static StageManager M_Stage => StageManager.Instance;
	#endregion

	public void Initialize()
	{
		m_Map = GetComponent<Map>();
		m_Map.Initialize();
	}

	[ContextMenu("Create Grid Text")]
	public void CreateGridText()
	{
		if (m_Tilemap == null)
			return;
		if (m_GridTextParent == null)
			return;

		DestroyGridText();

		BoundsInt boundsInt = m_Tilemap.cellBounds;

		Vector3Int min = boundsInt.min;
		Vector3Int max = boundsInt.max;

		for (int y = min.y; y < max.y; y++)
		{
			for (int x = min.x; x < max.x; x++)
			{
				Vector2 pos = new Vector2(x, y) + Vector2.one * 0.5f;
				string text = new Vector2Int(x, y).ToString();

				UtilClass.CreateWorldText(text, m_GridTextParent, pos, 0.1f, 25, m_GridTextColor, TextAnchor.MiddleCenter);
			}
		}
	}
	[ContextMenu("Destroy Grid Text")]
	public void DestroyGridText()
	{
		if (m_GridTextParent == null)
			return;

		int count = m_GridTextParent.childCount;
		for (int i = 0; i < count; ++i)
		{
			DestroyImmediate(m_GridTextParent.GetChild(0).gameObject);
		}
	}

	private void OnValidate()
	{
		m_GridTextParent.gameObject.SetActive(m_ShowGridText);
	}
	private void OnDrawGizmos()
	{
		if (m_ShowGrid == false)
			return;

		m_Tilemap = M_Stage.currentStage.currentRoom.GetTilemap(Room.E_RoomTilemapLayer.TileMap);

		if (m_Tilemap == null)
			return;

		BoundsInt boundsInt = m_Tilemap.cellBounds;

		Vector3 from = new Vector3(), to = new Vector3();
		Vector3Int min = boundsInt.min;
		Vector3Int max = boundsInt.max;

		Color color = Gizmos.color;

		Gizmos.color = m_GridColor;

		from.y = min.y;
		to.y = max.y;
		for (int x = min.x; x <= max.x; x++)
		{
			to.x = from.x = x;

			Gizmos.DrawLine(from, to);
		}
		from.x = min.x;
		to.x = max.x;
		for (int y = min.y; y <= max.y; y++)
		{
			to.y = from.y = y;

			Gizmos.DrawLine(from, to);
		}

		Gizmos.color = color;
	}
}