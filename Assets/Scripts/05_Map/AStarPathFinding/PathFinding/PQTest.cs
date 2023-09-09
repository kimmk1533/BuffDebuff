using System.Collections;
using System.Collections.Generic;
using Algorithms;
using UnityEngine;

// 1,000,000개 테스트 결과
// 
// PQB (Push) : 0.05249023
// MyPQ (Push): 0.08105469
// 
// PQB (Pop)  : 1.760498
// MyPQ (Pop) : 2.255859
// 
// 로 약 20% 정도의 성능 차이가 있음.
// 추후 수정할 필요가 보임.
public class PQTest : MonoBehaviour
{
	PriorityQueueB<int> m_PQB;
	PriorityQueue<int> m_MyPQ;

	[ContextMenu("Test")]
	void Test()
	{
		m_MyPQ = new PriorityQueue<int>();
		m_PQB = new PriorityQueueB<int>();

		float start, end;

		start = Time.realtimeSinceStartup;
		for (int i = 0; i < 1000; ++i)
		{
			for (int j = 0; j < 1000; ++j)
			{
				m_PQB.Push(i * 1000 + j);
			}
		}
		end = Time.realtimeSinceStartup;
		Debug.Log("PQ (Push): " + (end - start));

		start = Time.realtimeSinceStartup;
		for (int i = 0; i < 1000; ++i)
		{
			for (int j = 0; j < 1000; ++j)
			{
				m_MyPQ.Push(i * 1000 + j);
			}
		}
		end = Time.realtimeSinceStartup;
		Debug.Log("MyPQ (Push): " + (end - start));

		start = Time.realtimeSinceStartup;
		for (int i = 0; i < 1000; ++i)
		{
			for (int j = 0; j < 1000; ++j)
			{
				m_PQB.Pop();
			}
		}
		end = Time.realtimeSinceStartup;
		Debug.Log("PQ (Pop): " + (end - start));

		start = Time.realtimeSinceStartup;
		for (int i = 0; i < 1000; ++i)
		{
			for (int j = 0; j < 1000; ++j)
			{
				m_MyPQ.Pop();
			}
		}
		end = Time.realtimeSinceStartup;
		Debug.Log("MyPQ (Pop): " + (end - start));
	}
}