using System;
using System.Collections.Generic;
using UnityEngine;

public static class UtilClass
{
	public static void NullCheckGetComponent<T>(this Component com, ref T obj) where T : Component
	{
		if (obj == null)
		{
			obj = com.GetComponent<T>();

			if (obj == null)
				Debug.LogError("없는 컴포넌트를 GetComponent함");
		}
	}
	public static void NullCheckGetComponentInParent<T>(this Component com, ref T obj) where T : Component
	{
		if (obj == null)
		{
			obj = com.GetComponentInParent<T>();

			if (obj == null)
				Debug.LogError("없는 컴포넌트를 GetComponentInParent함");
		}
	}
	public static void NullCheckGetComponentInChilderen<T>(this Component com, ref T obj) where T : Component
	{
		if (obj == null)
		{
			obj = com.GetComponentInChildren<T>();

			if (obj == null)
				Debug.LogError("없는 컴포넌트를 GetComponentInChildren함");
		}
	}

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

		private bool m_IsSimulating;

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
		public float time => m_Time;

		public Timer()
		{
			m_Time = m_Interval = 0f;
			m_IsSimulating = true;
		}
		public Timer(float interval, bool filled = false)
		{
			m_Interval = interval;

			if (filled)
				m_Time = interval;
			else
				m_Time = 0f;

			m_IsSimulating = true;
		}

		public bool TimeCheck(bool autoClear = false)
		{
			if (m_Time >= m_Interval)
			{
				if (autoClear)
					Clear();

				return true;
			}

			return false;
		}
		public void Update()
		{
			if (m_IsSimulating == false)
				return;

			if (m_Time >= m_Interval)
				return;

			m_Time += Time.deltaTime;
		}
		public void Update(float timeScale)
		{
			if (m_IsSimulating == false)
				return;

			if (m_Time >= m_Interval)
				return;

			m_Time += Time.deltaTime * timeScale;
		}
		public void Clear()
		{
			m_Time = 0f;
		}

		public void Start()
		{
			m_IsSimulating = true;
		}
		public void Stop()
		{
			m_IsSimulating = false;
		}
	}
}