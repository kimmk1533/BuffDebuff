using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[DefaultExecutionOrder(-98)]
public abstract class Singleton<TSelf> : MonoBehaviour where TSelf : Singleton<TSelf>
{
	#region 변수
	[SerializeField]
	protected bool m_IsMainScene;
	[SerializeField]
	protected bool m_DontDestroyOnLoad;

	private static TSelf m_Instance;
	#endregion

	#region 프로퍼티
	public static TSelf Instance
	{
		get
		{
			if (m_Instance != null)
				return m_Instance;

			Singleton<TSelf>[] objs = FindObjectsByType<Singleton<TSelf>>(FindObjectsInactive.Include, FindObjectsSortMode.None);

			Singleton<TSelf> obj = objs
				.Where(item => item.m_IsMainScene == true)
				.FirstOrDefault(); //GameObject.Find(typeof(T).Name);

			if (obj == null)
			{
				if (objs.Length > 0)
					m_Instance = objs[0].GetComponent<TSelf>();
				else
				{
					GameObject t = new GameObject(typeof(TSelf).Name + "_New");
					obj = m_Instance = t.AddComponent<TSelf>();
					obj.m_IsMainScene = true;
					obj.m_DontDestroyOnLoad = true;
				}
			}
			else
			{
				m_Instance = obj.GetComponent<TSelf>();
			}

			if (Application.isPlaying)
			{
				foreach (var item in objs)
				{
					if (obj == item)
						continue;

					GameObject.Destroy(item.gameObject);
				}
			}

			if (Application.isPlaying == true &&
				obj.m_DontDestroyOnLoad)
				DontDestroyOnLoad(obj.gameObject);

			return m_Instance;
		}
	}
	#endregion

	private void Awake()
	{
		if (Application.isPlaying == true &&
			m_DontDestroyOnLoad)
			DontDestroyOnLoad(gameObject);
	}
}