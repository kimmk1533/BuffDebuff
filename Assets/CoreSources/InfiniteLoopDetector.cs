using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 출처: https://rito15.github.io/posts/unity-memo-prevent-infinite-loop/
public static class InfiniteLoopDetector
{
	private static string m_PrevPoint = "";
	private static int m_DetectionCount = 0;
	private const int m_DetectionThreshold = 1000000;

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void Run(
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0
	)
	{
#if !UNITY_EDITOR
		return;
#endif

		string currentPoint = $"{filePath}:{lineNumber}, {memberName}()";

		if (m_PrevPoint == currentPoint)
			++m_DetectionCount;
		else
			m_DetectionCount = 0;

		if (m_DetectionCount > m_DetectionThreshold)
			throw new Exception($"Infinite Loop Detected: \n{currentPoint}\n\n");

		m_PrevPoint = currentPoint;
	}

#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoadMethod]
	private static void Init()
	{
		UnityEditor.EditorApplication.update += () =>
		{
			m_DetectionCount = 0;
		};
	}
#endif
}