using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMap : MonoBehaviour
{
	private SpriteRenderer m_SpriteRenderer;
	public SpriteRenderer spriteRenderer => m_SpriteRenderer;

	[Min(0.001f)]
	public float m_PixelsPerUnit = 16f;

	private Rect m_Rect;
	public int width => Mathf.RoundToInt(m_Rect.width);
	public int height => Mathf.RoundToInt(m_Rect.height);

	private Color m_AlphaColor = Color.clear;
	private GameObject m_Tilemap = null;
	private Texture2D m_Texture = null;
	//private int m_SortingLayer;
	//private Dictionary<int, GameObject> m_SortingLayerTilemapDictionary = new Dictionary<int, GameObject>();
	//public int sortingLayer
	//{
	//	get { return m_SortingLayer; }
	//	set { m_SortingLayer = value; }
	//}

	// 추후 Value는 Scriptable Object로 수정하여 메모리를 절약할 것
	// 또는 유니티의 스프라이트처럼 한 장의 전체 텍스쳐와 범위만 가지고 있는 것은 어떨까?
	private Dictionary<Vector2, Sprite> m_SpriteDictionary = new Dictionary<Vector2, Sprite>();

	public void DrawTile(Vector2 pos, Sprite tile)
	{
		// 예외처리
		if (m_SpriteDictionary == null)
			return;

		// 정보 저장
		m_SpriteDictionary[pos] = tile;

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
		if (m_SpriteDictionary.Remove(pos) == false)
			return;

		// 새로운 정보를 바탕으로 크기 계산
		ReSize();

		// 저장된 정보를 바탕으로 렌더링
		UpdateRenderer(pos, null);
	}

	[ContextMenu("Init")]
	private void Init()
	{
		if (m_Rect == null) m_Rect = new Rect();
		else m_Rect.Set(0f, 0f, 0f, 0f);

		if (m_SpriteDictionary == null) m_SpriteDictionary = new Dictionary<Vector2, Sprite>();
		else m_SpriteDictionary.Clear();

		if (m_Tilemap == null)
		{
			int childCount = transform.childCount;

			if (childCount == 0)
			{
				m_Tilemap = new GameObject("Layer 0", typeof(SpriteRenderer));
				m_Tilemap.transform.SetParent(transform);
				m_Tilemap.transform.position = Vector3.zero;
				//m_Tilemap.hideFlags = HideFlags.HideInHierarchy;

				m_SpriteRenderer = m_Tilemap.GetComponent<SpriteRenderer>();
				m_SpriteRenderer.sprite = null;
			}
			else
			{
				for (int i = 1; i < childCount; ++i)
				{
					GameObject.DestroyImmediate(transform.GetChild(1));
				}

				m_Tilemap = transform.GetChild(0).gameObject;

				m_SpriteRenderer = m_Tilemap.GetComponent<SpriteRenderer>();
				m_SpriteRenderer.sprite = null;
			}
		}

		if (m_Texture == null)
		{
			int size = Mathf.RoundToInt(m_PixelsPerUnit);

			m_Texture = new Texture2D(size, size)
			{
				alphaIsTransparency = true,
				filterMode = FilterMode.Point,
			};
		}
	}
	private void ReSize()
	{
		// 예외처리
		if (m_SpriteDictionary.Count == 0)
		{
			m_Rect.width = 0;
			m_Rect.height = 0;
			return;
		}

		var posList = m_SpriteDictionary
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

		m_Rect.xMin = xMin; m_Rect.yMin = yMin;
		m_Rect.xMax = xMax; m_Rect.yMax = yMax;

		m_Tilemap.transform.localPosition = m_Rect.min / m_PixelsPerUnit;
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
				m_Texture.SetPixel(x, y, m_AlphaColor);
			}
		}

		// 타일 그리기
		foreach (var item in m_SpriteDictionary)
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

					m_Texture.SetPixel(tex_x, tex_y, src.texture.GetPixel(src_offsetX + x, src_offsetY + y));
				}
			}
		}

		m_Texture.Apply();

		Sprite sprite = Sprite.Create(m_Texture, new Rect(0, 0, width, height), Vector2.zero, m_PixelsPerUnit);

		m_SpriteRenderer.sprite = sprite;
	}
	private void UpdateRenderer(Vector2 pos, Sprite tile)
	{
		int width = this.width;
		int height = this.height;

		if (m_Texture.width != width ||
			m_Texture.height != height)
		{
			m_Texture.Reinitialize(width, height);

			// 투명 처리
			for (int y = 0; y < height; ++y)
			{
				for (int x = 0; x < width; ++x)
				{
					m_Texture.SetPixel(x, y, m_AlphaColor);
				}
			}

			// 타일 그리기
			foreach (var item in m_SpriteDictionary)
			{
				Vector2 tempPos = (item.Key - Vector2.one / 2) * m_PixelsPerUnit;
				Sprite tempTile = item.Value;

				int tempWidth = Mathf.RoundToInt(tempTile.rect.width);
				int tempHeight = Mathf.RoundToInt(tempTile.rect.height);

				if (m_Rect.xMin <= tempPos.x + 0 && tempPos.x + tempWidth <= m_Rect.xMax &&
					m_Rect.yMin <= tempPos.y + 0 && tempPos.y + tempHeight <= m_Rect.yMax)
				{
					int tempOffsetX = Mathf.RoundToInt(tempTile.rect.x);
					int tempOffsetY = Mathf.RoundToInt(tempTile.rect.y);

					for (int y = 0; y < tempHeight; ++y)
					{
						for (int x = 0; x < tempWidth; ++x)
						{
							int tex_x = Mathf.RoundToInt(tempPos.x + x - m_Rect.min.x);
							int tex_y = Mathf.RoundToInt(tempPos.y + y - m_Rect.min.y);

							m_Texture.SetPixel(tex_x, tex_y, tempTile.texture.GetPixel(tempOffsetX + x, tempOffsetY + y));
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

			if (m_Rect.xMin <= destPos.x + 0 && destPos.x + src_width < m_Rect.xMax &&
				m_Rect.yMin <= destPos.y + 0 && destPos.y + src_height < m_Rect.yMax)
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

						m_Texture.SetPixel(tex_x, tex_y, color);
					}
				}
			}
		}

		m_Texture.Apply();

		Sprite sprite = Sprite.Create(m_Texture, new Rect(0, 0, width, height), Vector2.zero, m_PixelsPerUnit);

		m_SpriteRenderer.sprite = sprite;
	}
}