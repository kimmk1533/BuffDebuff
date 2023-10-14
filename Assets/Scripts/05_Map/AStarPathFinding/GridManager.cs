using System.Collections;
using System.Collections.Generic;
using Algorithms;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : Singleton<GridManager>
{
	[SerializeField]
	private Map m_Map;

	[SerializeField]
	private bool m_ShowGrid;
	[SerializeField]
	private Color m_GridColor;

	[Space(10)]
	[SerializeField]
	private Transform m_GridTextParent;
	[SerializeField]
	private bool m_ShowGridText;
	[SerializeField]
	private Color m_GridTextColor;

	[Space(10)]
	[SerializeField]
	private Tilemap m_Tilemap;

	public Map map => m_Map;

	protected StageManager M_Stage => StageManager.Instance;

	public void Initialize()
	{
		m_Tilemap = M_Stage.currentRoom.GetTilemap(Room.E_RoomTilemapLayer.TileMap);

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

		BoundsInt boundsInt = m_Tilemap.cellBounds;

		Vector3Int min = boundsInt.min;
		Vector3Int max = boundsInt.max;

		DestroyGridText();

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

	public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, int characterWidth, int characterHeight, short maxCharacterJumpHeight)
	{
		return m_Map.FindPath(start, end, characterWidth, characterHeight, maxCharacterJumpHeight);
	}
}