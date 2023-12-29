using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-99)]
public class LoadingSceneManager : MonoBehaviour
{
	#region 변수
	private static string m_NextScene;

	[SerializeField]
	private TextMeshProUGUI m_Percent;
	[SerializeField]
	private Image m_ProgressBar;
	#endregion

	#region 이벤트
	public static event System.Action onLoadSceneCompleted;
	#endregion

	private void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		StartCoroutine(LoadScene());
	}
	public void Finallize()
	{

	}

	public static void LoadScene(string sceneName)
	{
		m_NextScene = sceneName;
		UnityEngine.SceneManagement.SceneManager.LoadScene("Loading Scene");
	}
	private IEnumerator LoadScene()
	{
		yield return null;

		AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(m_NextScene);
		op.allowSceneActivation = false;
		op.completed += OnLoadCompleted;

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

					yield break;
				}
			}
		}
	}
	private void OnLoadCompleted(AsyncOperation op)
	{
		onLoadSceneCompleted?.Invoke();
		onLoadSceneCompleted = null;
	}
}