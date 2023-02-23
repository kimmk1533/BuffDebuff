using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tilemap : MonoBehaviour
{
	private SpriteRenderer m_SpriteRenderer;
	public SpriteRenderer spriteRenderer => m_SpriteRenderer;

	[Min(0.001f)]
	public float m_PixelsPerUnit = 16f;

	private Rect m_Rect;
	public Rect rect
	{
		get
		{
			return m_Rect;
		}
	}
	public int width => Mathf.RoundToInt(m_Rect.width);
	public int height => Mathf.RoundToInt(m_Rect.height);

	private Color m_AlphaColor = Color.clear;
	private GameObject m_Tilemap = null;

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
		UpdateRenderer();
	}
	public void DestroyTile(Vector2 pos)
	{
		// 예외처리
		if (m_SpriteDictionary == null)
			return;

		// 정보 제거
		m_SpriteDictionary.Remove(pos);

		// 새로운 정보를 바탕으로 크기 계산
		ReSize();

		// 저장된 정보를 바탕으로 렌더링
		UpdateRenderer();
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

		Texture2D texture = new Texture2D(width, height)
		{
			alphaIsTransparency = true,
			filterMode = FilterMode.Point,
		};

		// 투명 처리
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				texture.SetPixel(x, y, m_AlphaColor);
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

					texture.SetPixel(tex_x, tex_y, src.texture.GetPixel(src_offsetX + x, src_offsetY + y));
				}
			}
		}

		texture.Apply();

		Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero, m_PixelsPerUnit);

		m_SpriteRenderer.sprite = sprite;
	}
}