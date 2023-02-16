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

	private Color[] m_AlphaColor = new Color[]
	{
		new Color(1f, 1f, 1f, 1f)
	};


	// 추후 Value는 Scriptable Object로 수정하여 메모리를 절약할 것
	private Dictionary<Vector2, Sprite> m_SpriteDictionary;

	private void Reset()
	{
		Init();
	}

	public void Init()
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		m_SpriteRenderer.sprite = null;
		m_Rect = new Rect();
		m_SpriteDictionary = new Dictionary<Vector2, Sprite>();
	}
	public void DrawTile(Vector2 pos, Sprite tile)
	{
		if (m_SpriteDictionary == null)
			return;

		m_SpriteDictionary[pos] = tile;

		Vector2 halfSize = tile.rect.size / 2;

		m_Rect.xMin = Mathf.Min(pos.x * m_PixelsPerUnit - halfSize.x, m_Rect.xMin);
		m_Rect.xMax = Mathf.Max(pos.x * m_PixelsPerUnit + halfSize.x, m_Rect.xMax);
		m_Rect.yMin = Mathf.Min(pos.y * m_PixelsPerUnit - halfSize.y, m_Rect.yMin);
		m_Rect.yMax = Mathf.Max(pos.y * m_PixelsPerUnit + halfSize.y, m_Rect.yMax);
	}
	public void Render()
	{
		int width = this.width;
		int height = this.height;

		Texture2D texture = new Texture2D(width, height)
		{
			filterMode = FilterMode.Point
		};

		//texture.SetPixels(m_AlphaColor);

		Sprite sprite = Sprite.Create(texture, new Rect(m_Rect.min, m_Rect.size), Vector2.zero);

		m_SpriteRenderer.sprite = sprite;
	}
}