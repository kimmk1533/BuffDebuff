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

	public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 5000)
	{
		if (color == null) color = Color.white;

		return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
	}
	public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
	{
		GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
		Transform transform = gameObject.transform;
		transform.SetParent(parent, false);
		transform.localPosition = localPosition;
		TextMesh textMesh = gameObject.GetComponent<TextMesh>();
		textMesh.anchor = textAnchor;
		textMesh.alignment = textAlignment;
		textMesh.text = text;
		textMesh.fontSize = fontSize;
		textMesh.color = color;
		textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
		return textMesh;
	}

	public class Timer
	{
		private float m_Interval;
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

		public Timer(float interval)
		{
			m_Interval = interval;
			m_Time = 0f;
		}

		public bool Update(bool autoUse = false, float timeScale = 1.0f)
		{
			if (m_Time >= m_Interval)
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
			if (m_Time >= m_Interval)
			{
				m_Time -= m_Interval;
			}
		}
	}
}