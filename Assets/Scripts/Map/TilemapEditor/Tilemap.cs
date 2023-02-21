using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
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
	public int width => (int)Mathf.RoundToInt(m_Rect.width);
	public int height => (int)Mathf.RoundToInt(m_Rect.height);

	private Color m_AlphaColor = new Color(1f, 0f, 0f, 1f);

	// 추후 Value는 Scriptable Object로 수정하여 메모리를 절약할 것
	private Dictionary<Vector2, Sprite> m_SpriteDictionary;

	private void Reset()
	{
		Init();

		int count = transform.childCount;
		for (int i = 0; i < count; ++i)
		{
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
	}

	public void Init()
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		m_SpriteRenderer.sprite = null;
		m_Rect = new Rect();
		m_SpriteDictionary = new Dictionary<Vector2, Sprite>();

		m_AlphaColor = new Color(0f, 0f, 0f, 0f);
	}
	public void DrawTile(Vector2 pos, Sprite tile)
	{
		if (m_SpriteDictionary == null)
			return;

		m_SpriteDictionary[pos] = tile;

		Debug.Log(pos);

		Vector2 halfSize = tile.rect.size / 2;

		Rect tileRect = new Rect(pos * m_PixelsPerUnit, tile.rect.size);

		//m_Rect.xMin = Mathf.Min(pos.x * m_PixelsPerUnit - halfSize.x, m_Rect.xMin);
		//m_Rect.xMax = Mathf.Max(pos.x * m_PixelsPerUnit + halfSize.x, m_Rect.xMax);
		//m_Rect.yMin = Mathf.Min(pos.y * m_PixelsPerUnit - halfSize.y, m_Rect.yMin);
		//m_Rect.yMax = Mathf.Max(pos.y * m_PixelsPerUnit + halfSize.y, m_Rect.yMax);

		if (m_Rect.xMin == m_Rect.xMax)
		{
			m_Rect.xMin = tileRect.xMin;
			m_Rect.xMax = tileRect.xMax;
		}
		else
		{
			m_Rect.xMin = Mathf.Min(tileRect.xMin, m_Rect.xMin);
			m_Rect.xMax = Mathf.Max(tileRect.xMax, m_Rect.xMax);
		}

		if (m_Rect.yMin == m_Rect.yMax)
		{
			m_Rect.yMin = tileRect.yMin;
			m_Rect.yMax = tileRect.yMax;
		}
		else
		{
			m_Rect.yMin = Mathf.Min(tileRect.yMin, m_Rect.yMin);
			m_Rect.yMax = Mathf.Max(tileRect.yMax, m_Rect.yMax);
		}
	}
	public void UpdateRenderer()
	{
		int width = this.width;
		int height = this.height;

		Debug.Log(new Vector2(width, height));

		Texture2D texture = new Texture2D(width, height)
		{
			alphaIsTransparency = true,
			filterMode = FilterMode.Point,
		};

		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				texture.SetPixel(x, y, m_AlphaColor);
			}
		}

		foreach (var item in m_SpriteDictionary)
		{
			Vector2 pos = (item.Key - Vector2.one * 0.5f) * m_PixelsPerUnit;
			Sprite spr = item.Value;

			int offsetX = (int)spr.rect.x;
			int offsetY = (int)spr.rect.y;

			int tex_width = Mathf.RoundToInt(spr.rect.width);
			int tex_height = Mathf.RoundToInt(spr.rect.height);

			for (int y = offsetY; y < offsetY + tex_height; ++y)
			{
				for (int x = offsetX; x < offsetX + tex_width; ++x)
				{
					texture.SetPixel((int)(x - offsetX + pos.x), (int)(y - offsetY + pos.y), spr.texture.GetPixel(x, y));
				}
			}
		}

		texture.Apply();

		Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero, m_PixelsPerUnit);

		m_SpriteRenderer.sprite = sprite;
	}
}