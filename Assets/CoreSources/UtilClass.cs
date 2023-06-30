using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class UtilClass
{
	public static Vector3 GetMouseWorldPosition()
	{
		Vector3 vector = GetMouseWorldPositionZ(Input.mousePosition, Camera.main);
		vector.z = 0f;
		return vector;
	}
	public static Vector3 GetMouseWorldPositionZ()
	{
		return GetMouseWorldPositionZ(Input.mousePosition, Camera.main);
	}
	public static Vector3 GetMouseWorldPositionZ(Camera worldCamera)
	{
		return GetMouseWorldPositionZ(Input.mousePosition, worldCamera);
	}
	public static Vector3 GetMouseWorldPositionZ(Vector3 screenPosition, Camera worldCamera)
	{
		Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
		return worldPosition;
	}

	public static TextMesh CreateWorldText(object text, Transform parent = null, Vector3 localPosition = default(Vector3), float characterSize = 0.1f, int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.LowerLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 5000)
	{
		if (color == null)
			color = Color.white;

		return CreateWorldText(parent, text.ToString(), localPosition, characterSize, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
	}
	public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), float characterSize = 0.1f, int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.LowerLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 5000)
	{
		if (color == null)
			color = Color.white;

		return CreateWorldText(parent, text, localPosition, characterSize, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
	}
	public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, float characterSize, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
	{
		GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
		Transform transform = gameObject.transform;
		transform.SetParent(parent, false);
		transform.localPosition = localPosition;
		TextMesh textMesh = gameObject.GetComponent<TextMesh>();
		textMesh.anchor = textAnchor;
		textMesh.alignment = textAlignment;
		textMesh.text = text;
		textMesh.characterSize = characterSize;
		textMesh.fontSize = fontSize;
		textMesh.color = color;
		textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
		return textMesh;
	}

	[System.Serializable]
	public class Timer
	{
		[SerializeField]
		private float m_Interval;
		[SerializeField, ReadOnly]
		private float m_Time;

		public float interval
		{
			get { return m_Interval; }
			set
			{
				m_Interval = value;
				if (m_Interval < m_Time)
					m_Time = m_Interval;
			}
		}
		public float time
		{
			get { return m_Time; }
		}
		public bool timeIsUp
		{
			get
			{
				return time >= m_Interval;
			}
		}

		public Timer(float interval)
		{
			m_Interval = interval;
			m_Time = 0f;
		}

		public bool Update(bool autoUse = false, float timeScale = 1.0f)
		{
			if (timeIsUp)
			{
				if (autoUse)
					Use();

				return true;
			}

			m_Time += Time.deltaTime * timeScale;

			return false;
		}
		public void Use()
		{
			m_Time = 0f;
		}
		public void Clear()
		{
			m_Time = 0f;
		}
	}
}