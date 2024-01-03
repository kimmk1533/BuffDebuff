using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public abstract class SceneReceiver : MonoBehaviour
{
	private void Awake()
	{
		Initialize();
	}
	private void OnDestroy()
	{
		Finallize();
	}
	protected abstract void Initialize();
	protected abstract void Finallize();
}