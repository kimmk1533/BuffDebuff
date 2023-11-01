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
	private static readonly Vector2 CellSize = Vector2.one;

	private static readonly Color GridColor = new Color(1f, 1f, 1f, 0.2f);
	private static readonly Color IconColor = new Color(1f, 1f, 1f, 0.3f);
	private static readonly Color SelectedCellColor = Color.green;
	#endregion

	// Show Options
	private static bool m_ShowPaletteOptions;
	private static bool m_ShowTilemapOptions;

	// Selected Option Index
	private static int m_PaletteIndex;
	private static int m_TileMapIndex;

	// Name List
	private static List<string> m_PaletteNameList;
	private static List<string> m_TileMapNameList;
	private static List<string> m_LayerNameList;

	// 씬에 존재하는 타일맵 리스트
	private static List<MyTileMap> m_TileMapList;
	// Palette Variables
	private static string m_PaletteName;
	private static int m_SelectedTileIndex;
	private static Vector2 m_PaletteScrollPos;
	// Palette Container
	private static Dictionary<string, List<Sprite>> m_PaletteMap;
	private static Dictionary<string, List<GUIContent>> m_PaletteIconDictionary;

	// Sorting Variables
	private static int m_SortingOrder;
	private static int m_SelectedLayerIndex;

	// Drawing Tile Dictionary
	private static Dictionary<int, Dictionary<Vector2, Dictionary<int, SpriteRenderer>>> m_TileMapDictionary;

	private static MyTileMap selectedTilemap
	{
		get
		{
			m_TileMapList[m_TileMapIndex].sortingLayerID = sortingLayerID;

			return m_TileMapList[m_TileMapIndex];
		}
	}
	private static int sortingLayerID
	{
		get
		{
			return SortingLayer.NameToID(m_LayerNameList[m_SelectedLayerIndex]);
		}
	}
	private static string sortingLayerName
	{
		get
		{
			return m_LayerNameList[m_SelectedLayerIndex];
		}
		set
		{
			m_SelectedLayerIndex = SortingLayer.GetLayerValueFromName(value) + 1;
		}
	}

	//[InitializeOnLoadMethod]
	[MenuItem("Custom Tilemap Tool/Enable")]
	public static void Enable()
	{
		Initialize();

		selectedTilemap.Initialize();

		Tools.current = Tool.None;

		SceneView.duringSceneGui -= OnSceneGUI;
		SceneView.duringSceneGui += OnSceneGUI;

		SceneView.duringSceneGui -= OnSceneUpdate;
		SceneView.duringSceneGui += OnSceneUpdate;
	}
	[MenuItem("Custom Tilemap Tool/Disable")]
	public static void Disable()
	{
		Tools.current = Tool.Move;

		SceneView.duringSceneGui -= OnSceneGUI;
		SceneView.duringSceneGui -= OnSceneUpdate;
	}

	private static void Initialize()
	{
		m_ShowPaletteOptions = true;

		string resourcesPath = Path.Combine(Application.dataPath, "Resources");
		if (!Directory.Exists(resourcesPath))
		{
			//Directory.CreateDirectory(resourcesPath);

			// 오류: Resources 폴더 존재하지 않음.
			throw new Exception(resourcesPath + " Folder doesn`t exist.");
		}

		resourcesPath = Path.Combine(resourcesPath, "Tilemap");
		if (!Directory.Exists(resourcesPath))
		{
			//Directory.CreateDirectory(resourcesPath);

			// 오류: Resources/Tilemap 폴더 존재하지 않음.
			throw new Exception("\"" + resourcesPath + "\" Folder doesn`t exist.");
		}

		List<Sprite> spriteList = new List<Sprite>(Resources.LoadAll<Sprite>("Tilemap"));
		if (spriteList.Count == 0)
		{
			// 오류: Resources/Tilemap 경로에 리소스가 존재하지 않음.
			throw new Exception("The Tilemap Resources doesn`t exist. Please insert the Sprite Resource into the \"" + resourcesPath + "\"");
		}

		m_PaletteNameList = new List<string>();
		m_PaletteMap = new Dictionary<string, List<Sprite>>();
		m_PaletteIconDictionary = new Dictionary<string, List<GUIContent>>();

		foreach (var item in spriteList)
		{
			string palette = item.name.Split('_')[0];

			m_PaletteNameList.Add(palette);

			#region Tile
			if (!m_PaletteMap.ContainsKey(palette))
				m_PaletteMap[palette] = new List<Sprite>();

			m_PaletteMap[palette].Add(item);
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
					icon.SetPixel(x - offsetX, y - offsetY, item.texture.GetPixel(x, y));
				}
			}

			icon.filterMode = FilterMode.Point;

			icon.Apply();

			m_PaletteIconDictionary[palette].Add(new GUIContent(icon));
			#endregion
		}
		m_PaletteScrollPos = new Vector2();

		m_PaletteName = m_PaletteNameList[0];

		m_LayerNameList = new List<string>();
		SortingLayer[] layers = SortingLayer.layers;
		foreach (var item in layers)
		{
			m_LayerNameList.Add(item.name);
		}
		sortingLayerName = "Default";

		m_SortingOrder = 0;

		m_ShowTilemapOptions = true;

		m_TileMapIndex = 0;
		m_TileMapList = new List<MyTileMap>(FindObjectsOfType<MyTileMap>());
		m_TileMapDictionary = new Dictionary<int, Dictionary<Vector2, Dictionary<int, SpriteRenderer>>>();

		m_TileMapList = m_TileMapList.OrderBy(x => x.name).ToList();
		m_TileMapNameList = new List<string>();
		foreach (var item in m_TileMapList)
		{
			m_TileMapNameList.Add(item.name);
		}

		foreach (var item in SortingLayer.layers)
		{
			m_TileMapDictionary.Add(item.id, new Dictionary<Vector2, Dictionary<int, SpriteRenderer>>());
		}
	}

	private static void OnSceneGUI(SceneView sceneView)
	{
		DisplayGrid();

		Handles.BeginGUI();
		{
			#region Icon
			#region size
			Vector2 one = sceneView.camera.WorldToScreenPoint(CellSize);
			Vector2 zero = sceneView.camera.WorldToScreenPoint(Vector2.zero);

			Vector2 size = one - zero;
			#endregion

			#region pos
			Vector2 cameraPos = sceneView.camera.WorldToScreenPoint(sceneView.camera.transform.position);
			Vector2 standardPos = new Vector2(zero.x, -zero.y - size.y - cameraPos.y * -2f);

			Vector2 cellPos = (GetSelectedCellPos() * size) / CellSize;
			cellPos.y = -cellPos.y;

			Vector2 pos = standardPos + cellPos;
			#endregion

			Texture texture = m_PaletteIconDictionary[m_PaletteName][m_SelectedTileIndex].image;

			GUI.DrawTexture(new Rect(pos, size), texture, ScaleMode.StretchToFill, true, 0f, IconColor, 0f, 0f);
			#endregion
		}
		Handles.EndGUI();

		Vector2 cellCenter = GetSelectedCellPos() + CellSize * 0.5f;
		DisplaySelectedCell(cellCenter);

		Handles.BeginGUI();
		{
			float yOffset = 0;

			#region Palette
			GUILayout.BeginArea(new Rect(10, sceneView.position.height - 50 + yOffset, 200, 25));
			{
				m_ShowPaletteOptions = GUILayout.Toggle(m_ShowPaletteOptions, "Palette Options", new GUIStyle("Foldout"));
			}
			GUILayout.EndArea();

			if (m_ShowPaletteOptions)
			{
				GUI.Box(new Rect(5, sceneView.position.height - 205 + yOffset, 510, 155), "");

				GUILayout.BeginArea(new Rect(10, sceneView.position.height - 200 + yOffset, 500, 145));
				{
					m_PaletteIndex = EditorGUILayout.Popup(m_PaletteIndex, m_PaletteNameList.ToArray());

					m_PaletteName = m_PaletteNameList[m_PaletteIndex];

					m_PaletteScrollPos = GUILayout.BeginScrollView(m_PaletteScrollPos);
					{
						m_SelectedTileIndex = GUILayout.SelectionGrid(m_SelectedTileIndex, m_PaletteIconDictionary[m_PaletteName].ToArray(), 5);
					}
					GUILayout.EndScrollView();
				}
				GUILayout.EndArea();
			}
			else
			{
				yOffset += 155f;
			}
			#endregion

			#region Tilemap
			GUILayout.BeginArea(new Rect(10, sceneView.position.height - 225 + yOffset, 200, 25));
			{
				m_ShowTilemapOptions = GUILayout.Toggle(m_ShowTilemapOptions, "Tilemap Options", new GUIStyle("Foldout"));
			}
			GUILayout.EndArea();

			if (m_ShowTilemapOptions)
			{
				GUI.Box(new Rect(5, sceneView.position.height - 280 + yOffset, 310, 55), "");

				GUILayout.BeginArea(new Rect(10, sceneView.position.height - 275 + yOffset, 300, 50));
				{
					m_TileMapIndex = EditorGUILayout.Popup(m_TileMapIndex, m_TileMapNameList.ToArray());

					GUILayout.Space(5f);

					m_SelectedLayerIndex = EditorGUILayout.Popup(m_SelectedLayerIndex, m_LayerNameList.ToArray());
				}
				GUILayout.EndArea();
			}
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

		mousePosition -= (Vector3)CellSize * 0.5f;

		Vector2Int cell = new Vector2Int(Mathf.RoundToInt(mousePosition.x / CellSize.x), Mathf.RoundToInt(mousePosition.y / CellSize.y));

		return cell * CellSize;
	}
	private static void DisplaySelectedCell(Vector2 cellCenter)
	{
		// Vertices of our square
		Vector3 topLeft = cellCenter + Vector2.left * CellSize * 0.5f + Vector2.up * CellSize * 0.5f;
		Vector3 topRight = cellCenter - Vector2.left * CellSize * 0.5f + Vector2.up * CellSize * 0.5f;
		Vector3 bottomLeft = cellCenter + Vector2.left * CellSize * 0.5f - Vector2.up * CellSize * 0.5f;
		Vector3 bottomRight = cellCenter - Vector2.left * CellSize * 0.5f - Vector2.up * CellSize * 0.5f;

		// Rendering
		Handles.color = SelectedCellColor;
		Vector3[] lines = { topLeft, topRight, topRight, bottomRight, bottomRight, bottomLeft, bottomLeft, topLeft };
		Handles.DrawLines(lines);

	}
	private static void DisplayGrid()
	{
		float width = CellSize.x;
		float height = CellSize.y;

		if (width <= 0 || height <= 0)
			return;

		Handles.color = GridColor;
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
		var layerMap = m_TileMapDictionary[sortingLayerID];
		if (!layerMap.ContainsKey(tilePos))
			layerMap.Add(tilePos, new Dictionary<int, SpriteRenderer>());

		var coordMap = layerMap[tilePos];
		if (coordMap.ContainsKey(m_SortingOrder) &&
			coordMap[m_SortingOrder] != null)
		{
			var sortingOrderMap = coordMap[m_SortingOrder];

			if (sortingOrderMap.sprite != m_PaletteMap[m_PaletteName][m_SelectedTileIndex] ||
				sortingOrderMap.sortingLayerID != sortingLayerID ||
				sortingOrderMap.sortingOrder != m_SortingOrder)
				Undo.DestroyObjectImmediate(sortingOrderMap.gameObject);
			else
				return;
		}

		//GameObject gameObject = new GameObject("tile", typeof(SpriteRenderer));
		//gameObject.transform.SetParent(selectedTilemap.transform);

		//SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		//spriteRenderer.sprite = m_PaletteDictionary[m_Palette][m_TileIndex];
		//spriteRenderer.sortingLayerID = sortingLayerID;
		//spriteRenderer.sortingOrder = m_SortingOrder;

		//gameObject.transform.localPosition = tilePos;

		//m_TilemapDictionary[tilePos][m_SortingOrder] = spriteRenderer;
		selectedTilemap.DrawTile(tilePos, m_PaletteMap[m_PaletteName][m_SelectedTileIndex]);

		//Undo.RegisterCreatedObjectUndo(m_TilemapDictionary[tilePos][m_SortingOrder].gameObject, "tile");
	}
	private static void DestroyTile(Vector2 tilePos)
	{
		//if (!m_TilemapDictionary.ContainsKey(tilePos) ||
		//	!m_TilemapDictionary[tilePos].ContainsKey(m_SortingOrder) ||
		//	m_TilemapDictionary[tilePos][m_SortingOrder] == null ||
		//	m_TilemapDictionary[tilePos][m_SortingOrder].sprite == null ||
		//	m_TilemapDictionary[tilePos][m_SortingOrder].sortingOrder != m_SortingOrder)
		//	return;

		//Undo.DestroyObjectImmediate(m_TilemapDictionary[tilePos][m_SortingOrder].gameObject);

		selectedTilemap.DestroyTile(tilePos);
	}

	private static void BrushTool()
	{
		int currentID = GUIUtility.GetControlID(FocusType.Passive);

		Sprite sprite = m_PaletteMap[m_PaletteName][m_SelectedTileIndex];
		// sprite.pivot은 절대 좌표이기 때문에 최대 크기로 나누어서 비율로 전환
		Vector2 pivot = sprite.pivot / sprite.rect.size;
		Vector2 tilePos = GetSelectedCellPos() + pivot * CellSize;

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