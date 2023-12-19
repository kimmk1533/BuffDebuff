using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-98)]
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	#region 변수
	[SerializeField]
	protected bool flag;

	private static T instance;
	#endregion

	#region 프로퍼티
	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				Singleton<T>[] objs = FindObjectsOfType<Singleton<T>>(true);

				GameObject obj = objs
					.Where(item => item.flag == true)
					.FirstOrDefault()?.gameObject; //GameObject.Find(typeof(T).Name);

				if (obj == null)
				{
					if (objs.Length > 0)
						return objs[0].GetComponent<T>();
					obj = new GameObject(typeof(T).Name);
					instance = obj.AddComponent<T>();
				}
				else
				{
					instance = obj.GetComponent<T>();
				}
			}

			return instance;
		}
	}
	#endregion
}

public abstract class SingletonBasic<T> where T : new()
{
	#region 변수
	private static T instance;
	#endregion

	#region 프로퍼티
	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new T();
			}

			return instance;
		}
	} 
	#endregion
}