using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class Methods
{
	private static System.DateTime timer;
	public static void CheckTimeIN()
	{
		timer = System.DateTime.Now;
	}
	public static System.TimeSpan CheckTimeStop()
	{
		System.TimeSpan time = System.DateTime.Now - timer;
		return time;
	}

	public static Transform[] GetChilderen(this Transform tf, string name)
	{
		int count = tf.childCount;

		List<Transform> ret_list = new List<Transform>();
		if (tf.name == name)
		{
			ret_list.Add(tf);
		}
		else if (count == 0)
			return null;

		for (int i = 0; i < count; i++)
		{
			Transform[] arr = tf.GetChild(i).GetChilderen(name);
			if (arr != null)
				ret_list.AddRange(arr);
		}

		return ret_list.ToArray();
	}

	public static Transform GetChild(this Transform _transform, string p_name)
	{
		int count = _transform.childCount;

		Transform temp_child_transforms;

		for (int i = 0; i < count; i++)
		{
			temp_child_transforms = _transform.GetChild(i);
			if (temp_child_transforms.name == p_name)
			{
				return temp_child_transforms;
			}
			else if (temp_child_transforms.childCount > 0)
			{
				temp_child_transforms = GetChild(temp_child_transforms, p_name);
				if (temp_child_transforms != null)
				{
					return temp_child_transforms;
				}
			}
		}
		return null;
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
		return transform.gameObject.AddComponent<T>();
	}
	public static Component GetOrAddComponent(this Component origin, System.Type type)
	{
		Component result = origin.GetComponent(type);
		if (result == null)
			result = origin.gameObject.AddComponent(type);

		return result;
	}
	public static T GetOrAddComponent<T>(this Component origin) where T : Component
	{
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