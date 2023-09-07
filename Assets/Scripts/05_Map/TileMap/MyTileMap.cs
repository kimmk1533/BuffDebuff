using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MyTileMap : MonoBehaviour
{
	// 추후 Value는 Scriptable Object로 수정하여 메모리를 절약할 것
	// 또는 유니티의 스프라이트처럼 한 장의 전체 텍스쳐와 범위만 가지고 있는 것은 어떨까?
	private Dictionary<int, Dictionary<Vector2, Sprite>> m_SpriteDictionary = null;

	private Dictionary<int, SpriteRenderer> m_SpriteRendererMap = null;
	private Dictionary<int, GameObject> m_LayerObjectMap = null;
	// 전체 텍스쳐
	private Dictionary<int, Texture2D> m_TextureMap = null;

	[Min(0.001f)]
	public float m_PixelsPerUnit = 16f;

	private int m_SelectedSortingLayerID;

	// 텍스쳐 영역
	private Rect m_Rect;

	// 배경 색
	private Color m_AlphaColor = Color.clear;

	public int width => Mathf.RoundToInt(m_Rect.width);
	public int height => Mathf.RoundToInt(m_Rect.height);
	public int sortingLayerID
	{
		get
		{
			return m_SelectedSortingLayerID;
		}
		set
		{
			m_SelectedSortingLayerID = value;
		}
	}

	[ContextMenu("Init")]
	public void Initialize()
	{
		if (m_SpriteDictionary == null)
		{
			m_SpriteDictionary = new Dictionary<int, Dictionary<Vector2, Sprite>>();

			foreach (var item in SortingLayer.layers)
			{
				m_SpriteDictionary.Add(item.id, new Dictionary<Vector2, Sprite>());
			}
		}


		if (m_SpriteRendererMap == null)
			m_SpriteRendererMap = new Dictionary<int, SpriteRenderer>();
		else
			m_SpriteRendererMap.Clear();

		if (m_LayerObjectMap == null)
			m_LayerObjectMap = new Dictionary<int, GameObject>();
		else
			m_LayerObjectMap.Clear();

		GameObject layerObject;
		SpriteRenderer spriteRenderer;
		foreach (var item in SortingLayer.layers)
		{
			Transform layerTF = transform.Find(item.name);

			if (layerTF != null)
			{
				layerObject = layerTF.gameObject;

				spriteRenderer = layerObject.GetComponent<SpriteRenderer>();
			}
			else
			{
				layerObject = new GameObject(item.name);

				layerObject.transform.SetParent(transform);

				layerObject.transform.position = Vector3.zero;

				spriteRenderer = layerObject.AddComponent<SpriteRenderer>();
				spriteRenderer.sortingLayerID = item.id;
			}

			m_LayerObjectMap.Add(item.id, layerObject);

			m_SpriteRendererMap.Add(item.id, spriteRenderer);
		}

		if (m_Rect == null)
			m_Rect = new Rect();

		if (m_TextureMap == null)
		{
			m_TextureMap = new Dictionary<int, Texture2D>();

			int size = Mathf.RoundToInt(m_PixelsPerUnit);

			foreach (var item in SortingLayer.layers)
			{
				m_TextureMap.Add(item.id, new Texture2D(size, size)
				{
					alphaIsTransparency = true,
					filterMode = FilterMode.Point,
				});
			}
		}
	}

	public void DrawTile(Vector2 pos, Sprite tile)
	{
		// 예외처리
		if (m_SpriteDictionary == null)
			return;

		// 정보 저장
		m_SpriteDictionary[m_SelectedSortingLayerID][pos] = tile;

		// 새로운 정보를 바탕으로 크기 계산
		ReSize();

		// 저장된 정보를 바탕으로 렌더링
		UpdateRenderer(pos, tile);
	}
	public void DestroyTile(Vector2 pos)
	{
		// 예외처리
		if (m_SpriteDictionary == null)
			return;

		// 정보 제거
		if (m_SpriteDictionary[m_SelectedSortingLayerID].Remove(pos) == false)
			return;

		// 새로운 정보를 바탕으로 크기 계산
		ReSize();

		// 저장된 정보를 바탕으로 렌더링
		UpdateRenderer(pos, null);
	}

	private void ReSize()
	{
		// 예외처리
		if (m_SpriteDictionary[m_SelectedSortingLayerID].Count == 0)
		{
			m_Rect.width = 0;
			m_Rect.height = 0;
			return;
		}

		var posList = m_SpriteDictionary[m_SelectedSortingLayerID]
			.Select(item => item.Key);

		// x좌표를 기준으로 정렬
		var xList = posList
			.OrderBy(item => item.x)
			.ToList();

		float xMin = (xList.First().x - 0.5f) * m_PixelsPerUnit;
		float xMax = (xList.Last().x + 0.5f) * m_PixelsPerUnit;

		// y좌표를 기준으로 정렬
		var yList = posList
			.OrderBy(item => item.y)
			.ToList();

		float yMin = (yList.First().y - 0.5f) * m_PixelsPerUnit;
		float yMax = (yList.Last().y + 0.5f) * m_PixelsPerUnit;

		m_Rect.xMin = xMin;
		m_Rect.yMin = yMin;
		m_Rect.xMax = xMax;
		m_Rect.yMax = yMax;

		m_LayerObjectMap[m_SelectedSortingLayerID].transform.localPosition = m_Rect.min / m_PixelsPerUnit;
	}
	private void UpdateRenderer()
	{
		int width = this.width;
		int height = this.height;

		// 투명 처리
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				m_TextureMap[m_SelectedSortingLayerID].SetPixel(x, y, m_AlphaColor);
			}
		}

		// 타일 그리기
		foreach (var item in m_SpriteDictionary[m_SelectedSortingLayerID])
		{
			Vector2 destPos = (item.Key - Vector2.one / 2) * m_PixelsPerUnit;

			Sprite src = item.Value;

			int src_offsetX = Mathf.RoundToInt(src.rect.x);
			int src_offsetY = Mathf.RoundToInt(src.rect.y);

			int src_width = Mathf.RoundToInt(src.rect.width);
			int src_height = Mathf.RoundToInt(src.rect.height);

			for (int y = 0; y < src_height; ++y)
			{
				for (int x = 0; x < src_width; ++x)
				{
					int tex_x = Mathf.RoundToInt(destPos.x + x - m_Rect.min.x);
					int tex_y = Mathf.RoundToInt(destPos.y + y - m_Rect.min.y);

					m_TextureMap[m_SelectedSortingLayerID].SetPixel(tex_x, tex_y, src.texture.GetPixel(src_offsetX + x, src_offsetY + y));
				}
			}
		}

		m_TextureMap[m_SelectedSortingLayerID].Apply();

		Sprite sprite = Sprite.Create(m_TextureMap[m_SelectedSortingLayerID], new Rect(0, 0, width, height), Vector2.zero, m_PixelsPerUnit);

		m_SpriteRendererMap[m_SelectedSortingLayerID].sprite = sprite;
	}
	private void UpdateRenderer(Vector2 pos, Sprite tile)
	{
		int width = this.width;
		int height = this.height;

		if (m_TextureMap[m_SelectedSortingLayerID].width != width ||
			m_TextureMap[m_SelectedSortingLayerID].height != height)
		{
			m_TextureMap[m_SelectedSortingLayerID].Reinitialize(width, height);

			// 투명 처리
			for (int y = 0; y < height; ++y)
			{
				for (int x = 0; x < width; ++x)
				{
					m_TextureMap[m_SelectedSortingLayerID].SetPixel(x, y, m_AlphaColor);
				}
			}

			// 타일 그리기
			foreach (var item in m_SpriteDictionary[m_SelectedSortingLayerID])
			{
				Vector2 tempPos = (item.Key - Vector2.one / 2) * m_PixelsPerUnit;
				Sprite tempTile = item.Value;

				int tempWidth = Mathf.RoundToInt(tempTile.rect.width);
				int tempHeight = Mathf.RoundToInt(tempTile.rect.height);

				if (m_Rect.xMin <= tempPos.x && tempPos.x + tempWidth <= m_Rect.xMax &&
					m_Rect.yMin <= tempPos.y && tempPos.y + tempHeight <= m_Rect.yMax)
				{
					int tempOffsetX = Mathf.RoundToInt(tempTile.rect.x);
					int tempOffsetY = Mathf.RoundToInt(tempTile.rect.y);

					for (int y = 0; y < tempHeight; ++y)
					{
						for (int x = 0; x < tempWidth; ++x)
						{
							int tex_x = Mathf.RoundToInt(tempPos.x + x - m_Rect.min.x);
							int tex_y = Mathf.RoundToInt(tempPos.y + y - m_Rect.min.y);

							m_TextureMap[m_SelectedSortingLayerID].SetPixel(tex_x, tex_y, tempTile.texture.GetPixel(tempOffsetX + x, tempOffsetY + y));
						}
					}
				}
			}
		}
		else
		{
			Vector2 destPos = (pos - Vector2.one / 2) * m_PixelsPerUnit;

			int src_width = Mathf.RoundToInt(tile == null ?
												m_PixelsPerUnit :
												tile.rect.width);
			int src_height = Mathf.RoundToInt(tile == null ?
												m_PixelsPerUnit :
												tile.rect.height);

			if (m_Rect.xMin <= destPos.x + 0 && destPos.x + src_width <= m_Rect.xMax &&
				m_Rect.yMin <= destPos.y + 0 && destPos.y + src_height <= m_Rect.yMax)
			{
				int src_offsetX = tile == null ?
									16 :
									Mathf.RoundToInt(tile.rect.x);
				int src_offsetY = tile == null ?
									16 :
									Mathf.RoundToInt(tile.rect.y);

				for (int y = 0; y < src_height; ++y)
				{
					for (int x = 0; x < src_width; ++x)
					{
						int tex_x = Mathf.RoundToInt(destPos.x + x - m_Rect.min.x);
						int tex_y = Mathf.RoundToInt(destPos.y + y - m_Rect.min.y);

						Color color = tile == null ?
										m_AlphaColor :
										tile.texture.GetPixel(src_offsetX + x, src_offsetY + y);

						m_TextureMap[m_SelectedSortingLayerID].SetPixel(tex_x, tex_y, color);
					}
				}
			}
		}

		m_TextureMap[m_SelectedSortingLayerID].Apply();

		Sprite sprite = Sprite.Create(m_TextureMap[m_SelectedSortingLayerID], new Rect(0, 0, width, height), Vector2.zero, m_PixelsPerUnit);

		m_SpriteRendererMap[m_SelectedSortingLayerID].sprite = sprite;
	}
}