using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

public class TileEditorWindow : EditorWindow
{
	#region RoomOptions
	private static bool m_ShowRoomOptions = true;
	private static int m_LevelWidth = 1;
	private static int m_LevelHeight = 1;
	#endregion

	#region GridOptions
	private static readonly Vector2 m_CellSize = Vector2.one;
	private static readonly Color m_GridColor = new Color(1f, 1f, 1f, 0.08f);
	private static readonly Color m_SelectedCellColor = Color.green;
	#endregion

	private static bool m_ShowPaletteOptions;
	private static List<string> m_PaletteList;
	private static Dictionary<string, List<Sprite>> m_PaletteDictionary;
	private static Dictionary<string, List<GUIContent>> m_PaletteIconDictionary;
	private static string m_Palette;
	private static Vector2 m_PaletteScrollPos;

	private static int m_PaletteIndex;
	private static int m_TileIndex;

	[InitializeOnLoadMethod]
	[MenuItem("Window/Custom Tools/Enable")]
	public static void Enable()
	{
		Init();

		Tools.current = Tool.Custom;

		SceneView.duringSceneGui -= OnSceneGUI;
		SceneView.duringSceneGui += OnSceneGUI;
	}
	[MenuItem("Window/Custom Tools/Disable")]
	public static void Disable()
	{
		Tools.current = Tool.Move;

		SceneView.duringSceneGui -= OnSceneGUI;
	}

	private static void Init()
	{
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

		m_ShowPaletteOptions = true;

		m_Palette = m_PaletteList[0];

		//string root = Path.Combine(Application.dataPath, "Tilemap Resources");
		//if (!Directory.Exists(root))
		//	Directory.CreateDirectory(root);

		//GameObject parent;
		//if (!GameObject.Find("Tilemap Parent"))
		//	parent = new GameObject("Tilemap Parent");
	}

	private static void OnSceneGUI(SceneView obj)
	{
		DisplayGrid();

		Handles.BeginGUI();
		{
			#region SelectedCell
			#region size
			Vector2 one = obj.camera.WorldToScreenPoint(m_CellSize);
			Vector2 zero = obj.camera.WorldToScreenPoint(Vector2.zero);

			Vector2 size = one - zero;
			#endregion

			#region pos
			Vector2 cameraPos = obj.camera.WorldToScreenPoint(obj.camera.transform.position);
			Vector2 standardPos = new Vector2(zero.x, zero.y - size.y - (zero.y - cameraPos.y) * 2f);

			Vector2 cellPos = (GetSelectedCellPos() * size) / m_CellSize;
			cellPos.y = -cellPos.y;

			Vector2 pos = standardPos + cellPos;
			#endregion

			Texture texture = m_PaletteIconDictionary[m_Palette][m_TileIndex].image;
			GUI.DrawTexture(new Rect(pos, size), texture, ScaleMode.StretchToFill);
			#endregion
		}
		Handles.EndGUI();

		Vector2 cellCenter = GetSelectedCellPos() + m_CellSize * 0.5f;
		DisplaySelectedCell(cellCenter);

		Handles.BeginGUI();
		{
			#region Palette

			GUILayout.BeginArea(new Rect(10, obj.position.height - 50, 50, 25));
			{
				m_ShowPaletteOptions = GUILayout.Toggle(m_ShowPaletteOptions, "", new GUIStyle("Foldout"));
			}
			GUILayout.EndArea();

			if (m_ShowPaletteOptions)
			{
				GUI.Box(new Rect(5, obj.position.height - 205, 310, 155), "");
				GUILayout.BeginArea(new Rect(10, obj.position.height - 200, 300, 145));
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
		}
		Handles.EndGUI();

		obj.Repaint();
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

	//private void OnGUI()
	//{
	//	m_ShowRoomOptions = EditorGUILayout.Foldout(m_ShowRoomOptions, "방 옵션", true);
	//	if (m_ShowRoomOptions)
	//	{
	//		EditorGUILayout.Space();
	//		++EditorGUI.indentLevel;

	//		EditorGUILayout.BeginHorizontal(); // Level Value
	//		{
	//			EditorGUILayout.BeginVertical("HelpBox");
	//			{
	//				m_LevelWidth = EditorGUILayout.IntField("너비", m_LevelWidth);
	//				m_LevelHeight = EditorGUILayout.IntField("높이", m_LevelHeight);

	//				if (m_LevelWidth < 1)
	//					m_LevelWidth = 1;
	//				if (m_LevelHeight < 1)
	//					m_LevelHeight = 1;
	//			}
	//			EditorGUILayout.EndVertical();
	//		}
	//		EditorGUILayout.EndHorizontal();

	//		EditorGUILayout.Space();

	//		//EditorGUILayout.BeginHorizontal();
	//		//GUILayout.Button("New Map Spawner");
	//		//GUILayout.Button("Find Map Spawner");
	//		//EditorGUILayout.EndHorizontal();

	//		--EditorGUI.indentLevel;
	//	}

	//	m_ShowGridOptions = EditorGUILayout.Foldout(m_ShowGridOptions, "그리드 옵션", true);
	//	if (m_ShowGridOptions)
	//	{
	//		EditorGUILayout.Space();
	//		++EditorGUI.indentLevel;

	//		EditorGUILayout.BeginHorizontal(); // Grid Value
	//		{
	//			EditorGUILayout.BeginVertical("HelpBox");
	//			{
	//				m_CellSize = EditorGUILayout.Vector2Field("칸 크기", m_CellSize);

	//				if (m_CellSize.x < 0)
	//					m_CellSize.x = 0;
	//				if (m_CellSize.y < 0)
	//					m_CellSize.y = 0;
	//			}
	//			EditorGUILayout.EndVertical();
	//		}
	//		EditorGUILayout.EndHorizontal();

	//		--EditorGUI.indentLevel;
	//	}
	//	//EditorGUILayout.Space();

	//	//m_PaintMode = GUILayout.Toggle(m_PaintMode, "그리기 시작", "Button", GUILayout.Height(60f));

	//	//EditorGUILayout.Space();

	//	++EditorGUI.indentLevel;

	//	EditorGUILayout.BeginVertical();

	//	EditorGUILayout.LabelField("현재 레벨:");

	//	EditorGUILayout.BeginHorizontal();
	//	GUILayout.Button("<");
	//	GUILayout.Button(">");
	//	EditorGUILayout.EndHorizontal();

	//	EditorGUILayout.EndVertical();

	//	m_PaletteIndex = EditorGUILayout.Popup("선택한 팔레트", m_PaletteIndex, m_PaletteNames);

	//	EditorGUILayout.LabelField("선택된 타일");

	//	--EditorGUI.indentLevel;
	//}
}