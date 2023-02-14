using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

public class TileEditorWindow : EditorWindow
{
	#region GridOptions
	private static readonly Vector2 m_CellSize = Vector2.one;
	private static readonly Color m_GridColor = new Color(1f, 1f, 1f, 0.08f);
	private static readonly Color m_IconColor = new Color(1f, 1f, 1f, 0.3f);
	private static readonly Color m_SelectedCellColor = Color.green;
	#endregion

	private static bool m_ShowPaletteOptions;

	private static List<string> m_PaletteList;
	private static Dictionary<string, List<Sprite>> m_PaletteDictionary;
	private static Dictionary<string, List<GUIContent>> m_PaletteIconDictionary;
	private static Dictionary<Vector2, Dictionary<int, SpriteRenderer>> m_Tilemap;

	private static string m_Palette;
	private static int m_PaletteIndex;
	private static int m_TileIndex;

	private static Vector2 m_PaletteScrollPos;

	private static List<string> m_SortingLayerList;
	private static int m_SortingLayerIndex;
	private static int sortingLayerID
	{
		get
		{
			return SortingLayer.NameToID(m_SortingLayerList[m_SortingLayerIndex]);
		}
	}
	private static string sortingLayerName
	{
		get
		{
			return m_SortingLayerList[m_SortingLayerIndex];
		}
		set
		{
			m_SortingLayerIndex = SortingLayer.GetLayerValueFromName(value) + 1;
		}
	}
	private static int m_SortingOrder;

	[InitializeOnLoadMethod]
	[MenuItem("Window/Custom Tilemap Tool/Enable")]
	public static void Enable()
	{
		Init();

		Tools.current = Tool.None;

		SceneView.duringSceneGui -= OnSceneGUI;
		SceneView.duringSceneGui += OnSceneGUI;

		SceneView.duringSceneGui -= OnSceneUpdate;
		SceneView.duringSceneGui += OnSceneUpdate;
	}
	[MenuItem("Window/Custom Tilemap Tool/Disable")]
	public static void Disable()
	{
		Tools.current = Tool.Move;

		SceneView.duringSceneGui -= OnSceneGUI;
		SceneView.duringSceneGui -= OnSceneUpdate;
	}

	private static void Init()
	{
		m_ShowPaletteOptions = true;

		string resourcesPath = Path.Combine(Application.dataPath, "Resources");
		if (!Directory.Exists(resourcesPath))
			Directory.CreateDirectory(resourcesPath);

		resourcesPath = Path.Combine(resourcesPath, "Tilemap Resources");
		if (!Directory.Exists(resourcesPath))
			Directory.CreateDirectory(resourcesPath);

		List<Sprite> spriteList = new List<Sprite>(Resources.LoadAll<Sprite>("Tilemap Resources"));

		m_PaletteList = new List<string>();
		m_PaletteDictionary = new Dictionary<string, List<Sprite>>();
		m_PaletteIconDictionary = new Dictionary<string, List<GUIContent>>();
		m_Tilemap = new Dictionary<Vector2, Dictionary<int, SpriteRenderer>>();

		foreach (var item in spriteList)
		{
			string palette = item.name.Split('_')[0];

			m_PaletteList.Add(palette);

			#region Tile
			if (!m_PaletteDictionary.ContainsKey(palette))
				m_PaletteDictionary[palette] = new List<Sprite>();

			m_PaletteDictionary[palette].Add(item);
			#endregion

			#region Icon
			if (!m_PaletteIconDictionary.ContainsKey(palette))
				m_PaletteIconDictionary[palette] = new List<GUIContent>();

			int offsetX = (int)item.rect.x;
			int offsetY = (int)item.rect.y;

			int width = (int)item.rect.width;
			int height = (int)item.rect.height;

			Texture2D icon = new Texture2D(width, height);

			for (int y = offsetY; y < offsetY + height; ++y)
			{
				for (int x = offsetX; x < offsetX + width; ++x)
				{
					icon.SetPixel(x, y, item.texture.GetPixel(x, y));
				}
			}

			icon.filterMode = FilterMode.Point;

			icon.Apply();

			m_PaletteIconDictionary[palette].Add(new GUIContent(icon));
			#endregion
		}
		m_PaletteScrollPos = new Vector2();

		m_Palette = m_PaletteList[0];

		m_SortingLayerList = new List<string>();
		SortingLayer[] layers = SortingLayer.layers;
		foreach (var item in layers)
		{
			m_SortingLayerList.Add(item.name);
		}
		sortingLayerName = "Default";

		m_SortingOrder = 0;
	}

	private static void OnSceneGUI(SceneView sceneView)
	{
		DisplayGrid();

		Handles.BeginGUI();
		{
			#region Icon
			#region size
			Vector2 one = sceneView.camera.WorldToScreenPoint(m_CellSize);
			Vector2 zero = sceneView.camera.WorldToScreenPoint(Vector2.zero);

			Vector2 size = one - zero;
			#endregion

			#region pos
			Vector2 cameraPos = sceneView.camera.WorldToScreenPoint(sceneView.camera.transform.position);
			Vector2 standardPos = new Vector2(zero.x, zero.y - size.y - (zero.y - cameraPos.y) * 2f);

			Vector2 cellPos = (GetSelectedCellPos() * size) / m_CellSize;
			cellPos.y = -cellPos.y;

			Vector2 pos = standardPos + cellPos;
			#endregion

			Texture texture = m_PaletteIconDictionary[m_Palette][m_TileIndex].image;

			GUI.DrawTexture(new Rect(pos, size), texture, ScaleMode.StretchToFill, true, 0f, m_IconColor, 0f, 0f);
			#endregion
		}
		Handles.EndGUI();

		Vector2 cellCenter = GetSelectedCellPos() + m_CellSize * 0.5f;
		DisplaySelectedCell(cellCenter);

		Handles.BeginGUI();
		{
			#region Palette
			GUILayout.BeginArea(new Rect(10, sceneView.position.height - 50, 50, 25));
			{
				m_ShowPaletteOptions = GUILayout.Toggle(m_ShowPaletteOptions, "", new GUIStyle("Foldout"));
			}
			GUILayout.EndArea();

			if (m_ShowPaletteOptions)
			{
				GUI.Box(new Rect(5, sceneView.position.height - 205, 310, 155), "");
				GUILayout.BeginArea(new Rect(10, sceneView.position.height - 200, 300, 145));
				{
					m_PaletteIndex = EditorGUILayout.Popup(m_PaletteIndex, m_PaletteList.ToArray());

					m_Palette = m_PaletteList[m_PaletteIndex];

					m_PaletteScrollPos = GUILayout.BeginScrollView(m_PaletteScrollPos);
					{
						m_TileIndex = GUILayout.SelectionGrid(m_TileIndex, m_PaletteIconDictionary[m_Palette].ToArray(), 5);
					}
					GUILayout.EndScrollView();
				}
				GUILayout.EndArea();
			}
			#endregion

			#region Layer
			GUI.Box(new Rect(5, sceneView.position.height - 370, 310, 160), "");
			GUILayout.BeginArea(new Rect(10, sceneView.position.height - 365, 300, 150));
			{
				m_SortingLayerIndex = EditorGUILayout.Popup(m_SortingLayerIndex, m_SortingLayerList.ToArray());
			}
			GUILayout.EndArea();
			#endregion
		}
		Handles.EndGUI();

		sceneView.Repaint();
	}
	private static void OnSceneUpdate(SceneView sceneView)
	{
		BrushTool();
	}

	private static Vector2 GetSelectedCellPos()
	{
		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		Vector3 mousePosition = ray.origin - ray.direction * (ray.origin.z / ray.direction.z);

		mousePosition -= (Vector3)m_CellSize * 0.5f;

		Vector2Int cell = new Vector2Int(Mathf.RoundToInt(mousePosition.x / m_CellSize.x), Mathf.RoundToInt(mousePosition.y / m_CellSize.y));

		return cell * m_CellSize;
	}
	private static void DisplaySelectedCell(Vector2 cellCenter)
	{
		// Vertices of our square
		Vector3 topLeft = cellCenter + Vector2.left * m_CellSize * 0.5f + Vector2.up * m_CellSize * 0.5f;
		Vector3 topRight = cellCenter - Vector2.left * m_CellSize * 0.5f + Vector2.up * m_CellSize * 0.5f;
		Vector3 bottomLeft = cellCenter + Vector2.left * m_CellSize * 0.5f - Vector2.up * m_CellSize * 0.5f;
		Vector3 bottomRight = cellCenter - Vector2.left * m_CellSize * 0.5f - Vector2.up * m_CellSize * 0.5f;

		// Rendering
		Handles.color = m_SelectedCellColor;
		Vector3[] lines = { topLeft, topRight, topRight, bottomRight, bottomRight, bottomLeft, bottomLeft, topLeft };
		Handles.DrawLines(lines);

	}
	private static void DisplayGrid()
	{
		float width = m_CellSize.x;
		float height = m_CellSize.y;

		if (width <= 0 || height <= 0)
			return;

		Handles.color = m_GridColor;
		for (float y = -540f; y < 540f; y += height)
		{
			Handles.DrawLine(new Vector3(10000000.0f, Mathf.Floor(y / height) * height, 0f), new Vector3(-10000000, Mathf.Floor(y / height) * height, 0f));
		}

		for (float x = -540f; x < 540f; x += width)
		{
			Handles.DrawLine(new Vector3(Mathf.Floor(x / width) * width, 10000000, 0f), new Vector3(Mathf.Floor(x / width) * width, -10000000, 0f));
		}

		Handles.color = Color.white;
		Handles.DrawLine(new Vector3(10000000.0f, 0.0f, 0.0f), new Vector3(-10000000.0f, 0.0f, 0.0f));
		Handles.DrawLine(new Vector3(0.0f, 10000000.0f, 0.0f), new Vector3(0.0f, -10000000.0f, 0.0f));
	}

	private static void DrawTile(Vector2 tilePos)
	{
		if (!m_Tilemap.ContainsKey(tilePos))
			m_Tilemap[tilePos] = new Dictionary<int, SpriteRenderer>();

		if (m_Tilemap[tilePos].ContainsKey(m_SortingOrder) &&
			m_Tilemap[tilePos][m_SortingOrder] != null)
		{
			// 수정해야함.
			if (m_Tilemap[tilePos][m_SortingOrder].sprite != m_PaletteDictionary[m_Palette][m_TileIndex] ||
				m_Tilemap[tilePos][m_SortingOrder].sortingLayerID != sortingLayerID ||
				m_Tilemap[tilePos][m_SortingOrder].sortingOrder != m_SortingOrder)
				Undo.DestroyObjectImmediate(m_Tilemap[tilePos][m_SortingOrder].gameObject);
			else
				return;
		}

		GameObject gameObject = new GameObject("tile", typeof(SpriteRenderer));
		SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = m_PaletteDictionary[m_Palette][m_TileIndex];
		spriteRenderer.sortingLayerID = sortingLayerID;
		spriteRenderer.sortingOrder = m_SortingOrder;

		gameObject.transform.position = tilePos;

		m_Tilemap[tilePos][m_SortingOrder] = spriteRenderer;

		Undo.RegisterCreatedObjectUndo(m_Tilemap[tilePos][m_SortingOrder].gameObject, "tile");
	}
	private static void DestroyTile(Vector2 tilePos)
	{
		if (!m_Tilemap.ContainsKey(tilePos) ||
			!m_Tilemap[tilePos].ContainsKey(m_SortingOrder) ||
			m_Tilemap[tilePos][m_SortingOrder] == null ||
			m_Tilemap[tilePos][m_SortingOrder].sprite == null ||
			m_Tilemap[tilePos][m_SortingOrder].sortingLayerID != sortingLayerID ||
			m_Tilemap[tilePos][m_SortingOrder].sortingOrder != m_SortingOrder)
			return;

		Undo.DestroyObjectImmediate(m_Tilemap[tilePos][m_SortingOrder].gameObject);
	}

	private static void BrushTool()
	{
		int currentID = GUIUtility.GetControlID(FocusType.Passive);

		Sprite sprite = m_PaletteDictionary[m_Palette][m_TileIndex];
		Vector2 pivot = sprite.pivot / sprite.rect.size;
		Vector2 tilePos = GetSelectedCellPos() + pivot * m_CellSize;

		if (Event.current.type == EventType.MouseDown)
		{
			if (Event.current.button == 0)
			{
				DrawTile(tilePos);
				GUIUtility.hotControl = currentID;
				Event.current.Use();
			}
			else if (Event.current.button == 1)
			{
				DestroyTile(tilePos);
				GUIUtility.hotControl = currentID;
				Event.current.Use();
			}
		}
		else if (Event.current.type == EventType.MouseDrag)
		{
			if (Event.current.button == 0)
			{
				DrawTile(tilePos);
			}
			else if (Event.current.button == 1)
			{
				DestroyTile(tilePos);
			}
		}
	}
}