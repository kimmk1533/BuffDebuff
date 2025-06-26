using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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

	public static Vector2 GetMouseWorldPosition2D()
	{
		Camera worldCamera = Camera.main;

		return GetMouseWorldPosition2D(worldCamera);
	}
	public static Vector2 GetMouseWorldPosition2D(Camera worldCamera)
	{
		Vector2 mousePosition = Input.mousePosition;

		return GetMouseWorldPosition2D(mousePosition, worldCamera);
	}
	public static Vector2 GetMouseWorldPosition2D(Vector3 screenPosition, Camera worldCamera)
	{
		Vector2 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);

		return worldPosition;
	}
	public static Vector3 GetMouseWorldPosition3D()
	{
		Camera worldCamera = Camera.main;

		return GetMouseWorldPosition3D(worldCamera);
	}
	public static Vector3 GetMouseWorldPosition3D(Camera worldCamera)
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = -worldCamera.transform.position.z;

		return GetMouseWorldPosition3D(mousePosition, worldCamera);
	}
	public static Vector3 GetMouseWorldPosition3D(Vector3 screenPosition, Camera worldCamera)
	{
		Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);

		return worldPosition;
	}

	public static bool IsPointerOnUI()
	{
		return EventSystem.current.IsPointerOverGameObject();
	}

	public static TextMesh CreateWorldText(object text, Transform parent = null, Vector3 localPosition = default, float characterSize = 0.1f, Font font = null, int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.LowerLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 5000, float duration = -1f)
	{
		return CreateWorldText(text.ToString(), parent, localPosition, characterSize, font, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder, duration);
	}
	public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default, float characterSize = 0.1f, Font font = null, int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.LowerLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 5000, float duration = -1f)
	{
		if (color == null)
			color = Color.white;

		WorldTextOption option = new WorldTextOption()
		{
			localPosition = localPosition,
			characterSize = characterSize,
			font = font,
			fontSize = fontSize,
			color = color.Value,
			textAnchor = textAnchor,
			textAlignment = textAlignment,
			sortingOrder = sortingOrder,
			duration = duration
		};

		return CreateWorldText(parent, text, option);
	}
	public static TextMesh CreateWorldText(Transform parent, string text, WorldTextOption option)
	{
		GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));

		Transform transform = gameObject.transform;
		transform.SetParent(parent, false);
		transform.localPosition = option.localPosition;

		TextMesh textMesh = gameObject.GetComponent<TextMesh>();
		textMesh.anchor = option.textAnchor;
		textMesh.alignment = option.textAlignment;
		textMesh.text = text;
		textMesh.characterSize = option.characterSize;
		textMesh.font = option.font;
		textMesh.fontSize = option.fontSize;
		textMesh.color = option.color;
		textMesh.GetComponent<MeshRenderer>().sortingOrder = option.sortingOrder;
		if (option.duration >= 0f)
			GameObject.Destroy(gameObject, option.duration);

		return textMesh;
	}

	public static TextMeshPro CreateWorldText(object text, Transform parent = null, Vector3 localPosition = default, TMP_FontAsset font = null, int fontSize = 40, Color? color = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.Left, int sortingOrder = 5000, float duration = -1f)
	{
		return CreateWorldText(text.ToString(), parent, localPosition, font, fontSize, color, textAlignment, sortingOrder, duration);
	}
	public static TextMeshPro CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default, TMP_FontAsset font = null, int fontSize = 40, Color? color = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.Left, int sortingOrder = 5000, float duration = -1f)
	{
		if (color == null)
			color = Color.white;

		WorldTMP_TextOption option = new WorldTMP_TextOption()
		{
			localPosition = localPosition,
			tmpFont = font,
			fontSize = fontSize,
			color = color.Value,
			textAlignment = textAlignment,
			sortingOrder = sortingOrder,
			duration = duration,
		};

		return CreateWorldText(parent, text, option);
	}
	public static TextMeshPro CreateWorldText(Transform parent, string text, WorldTMP_TextOption option)
	{
		GameObject gameObject = new GameObject("World_TMP_Text", typeof(TextMeshPro));

		Transform transform = gameObject.transform;
		transform.SetParent(parent, false);
		transform.localPosition = option.localPosition;

		TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
		textMesh.alignment = option.textAlignment;
		textMesh.text = text;
		textMesh.font = option.tmpFont;
		textMesh.fontSize = option.fontSize;
		textMesh.color = option.color;
		textMesh.sortingOrder = option.sortingOrder;
		if (option.duration >= 0f)
			GameObject.Destroy(gameObject, option.duration);

		return textMesh;
	}

	public class WorldTextOption
	{
		public Vector3 localPosition = default;
		public float characterSize = 0.1f;
		public Font font = null;
		public int fontSize = 40;
		public Color color = Color.white;
		public TextAnchor textAnchor = TextAnchor.LowerLeft;
		public TextAlignment textAlignment = TextAlignment.Left;
		public int sortingOrder = 5000;
		public float duration = -1f;
	}
	public class WorldTMP_TextOption
	{
		public Vector3 localPosition = default;
		public TMP_FontAsset tmpFont = null;
		public float fontSize = 10f;
		public Color color = Color.white;
		public TextAlignmentOptions textAlignment = TextAlignmentOptions.Left;
		public int sortingOrder = 5000;
		public float duration = -1f;
	}

	[System.Serializable]
	public class Timer
	{
		#region 변수
		[SerializeField]
		private float m_Interval = 0f;
		[SerializeField, ReadOnly]
		private float m_Time = 0f;

		private bool m_IsSimulating = true;
		#endregion

		#region 프로퍼티
		public float interval
		{
			get => m_Interval;
			set
			{
				m_Interval = value;
				if (m_Interval < m_Time)
					m_Time = m_Interval;
			}
		}
		public float time
		{
			get => m_Time;
			set => m_Time = value;
		}
		public float progress => m_Time / m_Interval;
		public bool isPaused => m_IsSimulating == false;
		#endregion

		#region 이벤트
		public event Action onTime = null;
		#endregion

		#region 생성자
		public Timer()
		{
			m_Time = m_Interval = 0f;
			m_IsSimulating = true;

			onTime = null;
		}
		public Timer(float interval, bool filled = false)
		{
			m_Interval = interval;

			if (filled)
				m_Time = interval;
			else
				m_Time = 0f;

			m_IsSimulating = true;

			onTime = null;
		}
		public Timer(in Timer timer)
		{
			m_Interval = timer.interval;
			m_Time = timer.m_Time;
			m_IsSimulating = timer.m_IsSimulating;
			onTime = timer.onTime;
		}
		#endregion

		/// <summary>
		/// 설정한 시간이 되었는 지 확인하는 함수
		/// </summary>
		/// <param name="autoClear">자동으로 다시 시작 여부</param>
		/// <returns>설정한 시간이 되었는 지</returns>
		public bool TimeCheck(bool autoClear = false)
		{
			if (m_Time >= m_Interval)
			{
				if (autoClear)
					Clear();

				onTime?.Invoke();

				return true;
			}

			return false;
		}

		/// <summary>
		/// 시간 경과
		/// </summary>
		public void Update()
		{
			Update(1f);
		}
		/// <summary>
		/// 시간 경과
		/// </summary>
		/// <param name="timeScale">시간 배율</param>
		public void Update(float timeScale)
		{
			if (m_IsSimulating == false)
				return;

			if (m_Time >= m_Interval)
				return;

			m_Time += Time.deltaTime * timeScale;
		}

		/// <summary>
		/// 시간 초기화
		/// </summary>
		public void Clear()
		{
			m_Time = 0f;
		}

		/// <summary>
		/// 일시정지
		/// </summary>
		public void Pause()
		{
			m_IsSimulating = false;
		}
		/// <summary>
		/// 다시 시작
		/// </summary>
		public void Resume()
		{
			m_IsSimulating = true;
		}
	}
}

public static class ExtensionMethods
{
	public static void Swap<T>(this List<T> list, int index1, int index2)
	{
		if (index1 == index2)
			return;

		if (list.Count <= index1 ||
			list.Count <= index2)
			return;

		T temp = list[index1];
		list[index1] = list[index2];
		list[index2] = temp;
	}
	public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> collection)
	{
		foreach (var item in collection)
		{
			queue.Enqueue(item);
		}
	}
	public static void PushRange<T>(this Stack<T> stack, IEnumerable<T> collection)
	{
		foreach (var item in collection)
		{
			stack.Push(item);
		}
	}
	public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> collection)
	{
		foreach (var item in collection)
		{
			dictionary.Add(item.Key, item.Value);
		}
	}

	public static T GetChild<T>(this Transform transform, int index) where T : Component
	{
		Transform child = transform.GetChild(index);

		if (child == null)
			return default(T);

		return child.GetComponent<T>();
	}
	public static Transform[] GetChildren(this Transform transform, string name)
	{
		int count = transform.childCount;

		List<Transform> ret_list = new List<Transform>();
		if (transform.name == name)
		{
			ret_list.Add(transform);
		}
		else if (count == 0)
			return null;

		for (int i = 0; i < count; i++)
		{
			Transform[] arr = transform.GetChild(i).GetChildren(name);
			if (arr != null)
				ret_list.AddRange(arr);
		}

		return ret_list.ToArray();
	}

	public static T Find<T>(this Transform transform, string name) where T : Component
	{
		Transform tf = transform.Find(name);

		if (tf == null)
			return null;

		return tf.GetComponent<T>();
	}
	public static Transform FindInChildren(this Transform transform, string name)
	{
		int count = transform.childCount;

		Transform childTransform;

		for (int i = 0; i < count; i++)
		{
			childTransform = transform.GetChild(i);
			if (childTransform.name == name)
			{
				return childTransform;
			}
			else if (childTransform.childCount > 0)
			{
				childTransform = FindInChildren(childTransform, name);
				if (childTransform != null)
				{
					return childTransform;
				}
			}
		}
		return null;
	}
	public static T FindInChildren<T>(this Transform transform, string name) where T : Component
	{
		return transform.FindInChildren(name)?.GetComponent<T>();
	}

	// 좌표 평면 기준 -180 ~ 180 도 리턴
	public static float GetAngle(this Vector2 vStart, Vector2 vEnd)
	{
		// return Quaternion.FromToRotation(vStart, vEnd).eulerAngles.z;
		Vector3 v = vEnd - vStart;
		return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
	}
	public static float GetAngle(this Vector3 vStart, Vector3 vEnd)
	{
		// return Quaternion.FromToRotation(vStart, vEnd).eulerAngles.z;
		Vector3 v = vEnd - vStart;
		return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
	}

	public static T AddComponent<T>(this Transform transform) where T : Component
	{
		if (transform == null)
			return null;

		return transform.gameObject.AddComponent<T>();
	}
	public static Component GetOrAddComponent(this Component origin, System.Type type)
	{
		if (origin == null)
			return null;

		Component result = origin.GetComponent(type);
		if (result == null)
			result = origin.gameObject.AddComponent(type);

		return result;
	}
	public static T GetOrAddComponent<T>(this Component origin) where T : Component
	{
		if (origin == null)
			return null;

		T result = origin.GetComponent<T>();
		if (result == null)
			result = origin.gameObject.AddComponent<T>();

		return result;
	}
	/// <summary>
	/// 기존 컴포넌트를 검사 후 없으면 추가하는 함수
	/// </summary>
	/// <typeparam name="T">추가할 컴포넌트</typeparam>
	/// <param name="result">결과 컴포넌트</param>
	/// <returns>추가 여부</returns>
	public static bool GetOrAddComponent<T>(this Component origin, out T result) where T : Component
	{
		result = origin.GetComponent<T>();
		if (result == null)
		{
			result = origin.gameObject.AddComponent<T>();
			return true;
		}

		return false;
	}
	public static T CopyComponent<T>(this Component original) where T : Component
	{
		Type type = original.GetType();
		GameObject tempObj = new GameObject("temp");
		T copy = tempObj.AddComponent<T>();

		FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (FieldInfo field in fields)
		{
			field.SetValue(copy, field.GetValue(original));
		}

		//PropertyInfo[] properties = type.GetProperties();
		//foreach (PropertyInfo property in properties)
		//{
		//    if (property.CanWrite)
		//    {
		//        property.SetValue(copy, property.GetValue(original));
		//    }
		//}

		GameObject.DestroyImmediate(tempObj);
		return copy;
	}
	public static T CopyComponent<T>(this Component original, GameObject dest) where T : Component
	{
		Type type = original.GetType();
		Component copy = dest.GetComponent(type);
		if (null == copy)
		{
			copy = dest.AddComponent(type);
		}

		FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (FieldInfo field in fields)
		{
			field.SetValue(copy, field.GetValue(original));
		}

		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo property in properties)
		{
			if (property.CanWrite)
			{
				property.SetValue(copy, property.GetValue(original));
			}
		}
		return copy as T;
	}
}