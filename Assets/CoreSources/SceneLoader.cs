using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class SceneLoader : SerializedMonoBehaviour
{
	#region 변수
	protected static string m_PrevScene = null;
	protected static string m_CurrScene = "Init Scene";

	[SerializeField, FoldoutGroup("Use Flags")]
	private bool m_UseFadeBG = true;
	[SerializeField, FoldoutGroup("Use Flags")]
	private bool m_UsePercent = true;
	[SerializeField, FoldoutGroup("Use Flags")]
	private bool m_UseProgressBar = true;

	[SerializeField, ShowIf("@m_UseFadeBG")]
	private Image m_FadeBG = null;
	[SerializeField, ShowIf("@m_UseFadeBG")]
	private float m_FadeDuration = 1f;
	[SerializeField, ShowIf("@m_UsePercent")]
	private TextMeshProUGUI m_Percent = null;
	[SerializeField, ShowIf("@m_UseProgressBar")]
	private Image m_ProgressBar = null;
	#endregion

	#region 프로퍼티
	public static string prevSceneName => m_PrevScene;
	#endregion

	#region 유니티 콜백 함수
	private void Awake()
	{
		Initialize();
	}
	private void OnDestroy()
	{
		Finallize();
	}
	#endregion

	#region 초기화 & 마무리화 함수
	/// <summary>
	/// 초기화 함수
	/// </summary>
	private void Initialize()
	{
		StartCoroutine(LoadScene());
	}
	/// <summary>
	/// 마무리화 함수
	/// </summary>
	private void Finallize()
	{

	}
	#endregion

	public static void LoadScene(string sceneName)
	{
		m_PrevScene = m_CurrScene;
		m_CurrScene = sceneName;

		LoadSceneParameters sceneParameters = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None);
		SceneManager.LoadScene("Loading Scene", sceneParameters);
	}

	// UnloadSceneAsync 참고: https://stackoverflow.com/questions/44727881/how-to-use-scenemanager-unloadsceneasync
	private IEnumerator LoadScene()
	{
		if (m_UseFadeBG == true)
			yield return FadeOutBG();

		yield return null;

		List<GameObject> rootGameObjectList = new List<GameObject>();
		SceneManager.GetSceneByName(m_PrevScene).GetRootGameObjects(rootGameObjectList);

		for (int i = 0; i < rootGameObjectList.Count; ++i)
		{
			SceneEventController eventController = rootGameObjectList[i].GetComponent<SceneEventController>();

			if (eventController == null)
				continue;

			eventController.OnBeforeSceneSwitching(m_CurrScene);
		}

		LoadSceneParameters sceneParameters = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None);
		AsyncOperation op = SceneManager.LoadSceneAsync(m_CurrScene, sceneParameters);
		op.allowSceneActivation = false;
		op.completed += OnSceneLoadCompleted;

		float fillAmount = 0f;

		while (!op.isDone)
		{
			fillAmount = Mathf.Lerp(0f, 1f, op.progress / 0.9f);

			if (m_UsePercent == true)
			{
				m_Percent.text = string.Format("{0:##0.00}%", fillAmount * 100f);

				if (fillAmount >= 0.99999f)
					m_Percent.text = string.Format("{0:##0}%", fillAmount * 100f);
			}
			if (m_UseProgressBar == true)
				m_ProgressBar.fillAmount = fillAmount;

			if (op.progress >= 0.9f)
			{
				if (m_UseFadeBG)
					yield return FadeInBG();

				break;
			}

			yield return null;
		}

		op.allowSceneActivation = true;

		while (!op.isDone)
			yield return null;
	}

	private IEnumerator FadeOutBG()
	{
		float t = 0f;
		Color colorA = m_FadeBG.color;
		Color colorB = Color.black;

		for (float time = 0f; time <= m_FadeDuration; time += Time.deltaTime)
		{
			t = time / m_FadeDuration;

			m_FadeBG.color = Color.Lerp(colorA, colorB, t);

			yield return null;
		}
	}
	private IEnumerator FadeInBG()
	{
		float t = 0f;
		Color colorA = m_FadeBG.color;
		Color colorB = m_FadeBG.color;
		colorB.a = 0f;

		for (float time = 0f; time <= m_FadeDuration; time += Time.deltaTime)
		{
			t = time / m_FadeDuration;

			m_FadeBG.color = Color.Lerp(colorA, colorB, t);
			yield return null;

		}
	}

	private void OnSceneLoadCompleted(AsyncOperation op)
	{
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(m_CurrScene));

		List<GameObject> rootGameObjectList = new List<GameObject>();
		SceneManager.GetSceneByName(m_PrevScene).GetRootGameObjects(rootGameObjectList);

		for (int i = 0; i < rootGameObjectList.Count; ++i)
		{
			SceneEventController eventController = rootGameObjectList[i].GetComponent<SceneEventController>();

			if (eventController == null)
				continue;

			eventController.OnAfterSceneSwitching(m_CurrScene);
		}

		TurnOffCamera();

		AsyncOperation opLoading = SceneManager.UnloadSceneAsync("Loading Scene");
		AsyncOperation opPrev = SceneManager.UnloadSceneAsync(m_PrevScene);
	}
	private void TurnOffCamera()
	{
		List<GameObject> rootGameObjectList = new List<GameObject>();

		for (int i = 0; i < SceneManager.sceneCount; ++i)
		{
			Scene scene = SceneManager.GetSceneAt(i);

			if (scene.isLoaded == false ||
				scene == SceneManager.GetActiveScene())
				continue;

			SceneManager.GetSceneByName(scene.name).GetRootGameObjects(rootGameObjectList);
			for (int j = 0; j < rootGameObjectList.Count; ++j)
			{
				Camera camera = rootGameObjectList[j].GetComponent<Camera>();

				if (camera == null)
					continue;

				camera.gameObject.SetActive(false);
			}
		}
	}
}