using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class Methods
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

	public static Transform[] GetChilderen(this Transform transform, string n)
	{
		int count = transform.childCount;

		List<Transform> ret_list = new List<Transform>();
		if (transform.name == n)
		{
			ret_list.Add(transform);
		}
		else if (count == 0)
			return null;

		for (int i = 0; i < count; i++)
		{
			Transform[] arr = transform.GetChild(i).GetChilderen(n);
			if (arr != null)
				ret_list.AddRange(arr);
		}

		return ret_list.ToArray();
	}

	public static T Find<T>(this Transform transform, string n) where T : Component
	{
		Transform tf = transform.Find(n);

		if (tf == null)
			return null;

		return tf.GetComponent<T>();
	}
	public static Transform FindInChilderen(this Transform transform, string n)
	{
		int count = transform.childCount;

		Transform childTransform;

		for (int i = 0; i < count; i++)
		{
			childTransform = transform.GetChild(i);
			if (childTransform.name == n)
			{
				return childTransform;
			}
			else if (childTransform.childCount > 0)
			{
				childTransform = FindInChilderen(childTransform, n);
				if (childTransform != null)
				{
					return childTransform;
				}
			}
		}
		return null;
	}
	public static T GetChild<T>(this Transform transform, int index)
	{
		Transform child = transform.GetChild(index);

		if (child == null)
			return default(T);

		return child.GetComponent<T>();
	}

	public static string CombinePath(char split_word = '/', params string[] path)
	{
		StringBuilder sb = new StringBuilder();
		int Length = path.Length;

		for (int i = 0; i < Length - 1; i++)
		{
			sb.Append(path[i] + split_word);
		}
		sb.Append(path[Length - 1]);

		return sb.ToString();
	}

	// 0 ~ 360 도 리턴
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

public static class String_Extension
{
	public static string[] mySplit(this string str, char seperator)
	{
		int length = str.Length;
		int seperator_pos = -1;
		for (int i = 0; i < length; i++)
		{
			if (str[i] == seperator)
			{
				seperator_pos = i;
				break;
			}
		}

		if (seperator_pos == -1)
			return null;

		string[] ret = new string[2];
		ret[0] = str.Substring(0, seperator_pos);
		ret[1] = str.Substring(seperator_pos + 1, length - seperator_pos - 1);

		return ret;
	}
}