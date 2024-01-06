using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
	#region 변수
	protected static string m_PrevScene = null;
	protected static string m_NextScene = null;

	[SerializeField]
	private TextMeshProUGUI m_Percent = null;
	[SerializeField]
	private Image m_ProgressBar = null;
	#endregion

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		StartCoroutine(LoadScene());
	}
	private void Finallize()
	{

	}

	public static void LoadScene(string sceneName)
	{
		m_PrevScene = SceneManager.GetActiveScene().name;
		m_NextScene = sceneName;

		LoadSceneParameters sceneParameters = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None);
		SceneManager.LoadScene("Loading Scene", sceneParameters);
	}
	private IEnumerator LoadScene()
	{
		yield return null;

		LoadSceneParameters sceneParameters = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None);
		AsyncOperation op = SceneManager.LoadSceneAsync(m_NextScene, sceneParameters);
		op.allowSceneActivation = false;
		op.completed += OnSceneLoadCompleted;

		while (!op.isDone)
		{
			yield return null;

			if (op.progress < 0.9f)
			{
				m_ProgressBar.fillAmount = Mathf.Lerp(m_ProgressBar.fillAmount, op.progress, op.progress / 0.9f);
				m_Percent.text = string.Format("{0:##0.00}%", (m_ProgressBar.fillAmount * 100f));
			}
			else
			{
				m_ProgressBar.fillAmount = Mathf.Lerp(m_ProgressBar.fillAmount, 1f, op.progress / 0.9f);
				m_Percent.text = string.Format("{0:##0.00}%", (m_ProgressBar.fillAmount * 100f));

				if (m_ProgressBar.fillAmount >= 1.0f)
				{
					op.allowSceneActivation = true;
					break;
				}
			}
		}
	}
	private void OnSceneLoadCompleted(AsyncOperation op)
	{
		SceneManager.UnloadSceneAsync(m_PrevScene);

		m_PrevScene = m_NextScene;
		m_NextScene = "";
	}
}