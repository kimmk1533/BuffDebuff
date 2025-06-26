using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[DefaultExecutionOrder(-98)]
public abstract class SerializedSingleton<TSelf> : SerializedMonoBehaviour where TSelf : SerializedSingleton<TSelf>
{
	#region 변수
	[SerializeField]
	private bool m_IsInitScene = false;
	[SerializeField]
	private bool m_DontDestroyOnLoad = false;

	private static TSelf m_Instance = null;
	#endregion

	#region 프로퍼티
	public static TSelf Instance
	{
		get
		{
			if (m_Instance != null)
				return m_Instance;

			SerializedSingleton<TSelf>[] objs = FindObjectsByType<SerializedSingleton<TSelf>>(FindObjectsInactive.Include, FindObjectsSortMode.None);

			SerializedSingleton<TSelf> obj = objs
				.Where(item => item.m_IsInitScene == true)
				.FirstOrDefault(); //GameObject.Find(typeof(T).Name);

			if (obj == null)
			{
				if (objs.Length > 0)
					obj = m_Instance = objs[0].GetComponent<TSelf>();
				else
				{
					GameObject t = new GameObject(typeof(TSelf).Name + "_New");
					obj = m_Instance = t.AddComponent<TSelf>();
					obj.m_IsInitScene = false;
					obj.m_DontDestroyOnLoad = false;
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

	#region 유니티 콜백 함수
	protected virtual void Awake()
	{
		if (Application.isPlaying == true &&
			m_DontDestroyOnLoad)
			DontDestroyOnLoad(gameObject);
	}
	#endregion

	#region 초기화 & 마무리화 함수
	/// <summary>
	/// 초기화 함수 (Init Scene 진입 시, 즉 게임 실행 시 호출)
	/// </summary>
	public virtual void Initialize()
	{
		gameObject.SetActive(false);
	}
	/// <summary>
	/// 마무리화 함수 (게임 종료 시 호출)
	/// </summary>
	public virtual void Finallize()
	{
	}

	/// <summary>
	/// 메인 초기화 함수 (본인 Main Scene 진입 시 호출)
	/// </summary>
	public virtual void InitializeMain()
	{
		gameObject.SetActive(true);
	}
	/// <summary>
	/// 메인 마무리화 함수 (본인 Main Scene 나갈 시 호출)
	/// </summary>
	public virtual void FinallizeMain()
	{
		gameObject.SetActive(false);
	}
	#endregion
}