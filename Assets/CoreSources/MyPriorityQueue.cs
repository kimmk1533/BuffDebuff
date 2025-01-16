using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Collections.Generic
{
	public interface IMyPriorityQueue<T> : IEnumerable<T>
	{
		int Push(T item);
		T Pop();
		T Peek();
	}

	// 참고: https://yoongrammer.tistory.com/81
	public class MyPriorityQueue<T> : IMyPriorityQueue<T>, IEnumerable<T>
	{
		private List<T> m_ElementList;
		private IComparer<T> m_Comparer;

		public IComparer<T> Comparer
		{
			get
			{
				return m_Comparer;
			}
		}
		public int Count
		{
			get
			{
				return m_ElementList.Count;
			}
		}
		private T root
		{
			get
			{
				if (m_ElementList == null ||
					m_ElementList.Count == 0)
					return default(T);

				return m_ElementList[0];
			}
		}

		public MyPriorityQueue()
		{
			m_ElementList = new List<T>();

			m_Comparer = Comparer<T>.Default;
		}
#nullable enable
		public MyPriorityQueue(IComparer<T>? comparer)
		{
			m_ElementList = new List<T>();

			m_Comparer = comparer;
		}
#nullable disable
		public MyPriorityQueue(IEnumerable<T> items)
		{
			m_ElementList = new List<T>(items);

			m_Comparer = Comparer<T>.Default;
		}
		public MyPriorityQueue(int initialCapacity)
		{
			m_ElementList = new List<T>(initialCapacity);

			m_Comparer = Comparer<T>.Default;
		}

		public void Clear()
		{
			m_ElementList.Clear();
		}
		public bool Contains(T element)
		{
			return m_ElementList.Contains(element);
		}
		public T Pop()
		{
			// 1. 루트 노드 반환하기 위해 저장
			T item = root;

			// 2. 최하단 노드를 루트 노드로 올림
			m_ElementList[0] = m_ElementList[m_ElementList.Count - 1];
			m_ElementList.RemoveAt(m_ElementList.Count - 1);

			// 3. 루트 노드와 자식 노트들과 값을 비교
			Heapify();

			// 4. 기존 루트 노드 반환
			return item;
		}
		public int Push(T element)
		{
			// 1. 최하단 노드에 새로운 원소 추가
			m_ElementList.Add(element);

			// 2. 추가한 원소와 부모 노드와 우선순위 비교
			int index = m_ElementList.Count - 1;
			T parent = GetParent(index);
			while (index > 0 &&
				Comparer.Compare(parent, element) > 0)
			{
				// 3. 추가한 원소의 우선순위가 더 높다면 부모와 자리교환
				m_ElementList[index] = parent;
				index = GetParentIndex(index);
				parent = GetParent(index);
			}

			// 3-1. 최적화로 자식은 마지막에 1번만 옮김
			m_ElementList[index] = element;

			return index;
		}
		public void Push(IEnumerable<T> items)
		{
			if (null == items)
				throw new ArgumentNullException(items.ToString());

			foreach (var item in items)
			{
				Push(item);
			}
		}
		public T Peek()
		{
			return root;
		}
		public void TrimExcess()
		{
			m_ElementList.TrimExcess();
		}
		public bool TryPop(out T element)
		{
			if (m_ElementList.Count == 0 ||
				root == null)
			{
				element = default(T);

				return false;
			}

			element = Pop();

			return true;
		}
		public bool TryPeek(out T element)
		{
			if (m_ElementList.Count == 0 ||
				root == null)
			{
				element = default(T);

				return false;
			}

			element = root;

			return true;
		}

		/// <summary>
		/// Heap 속성을 유지하는 작업
		/// </summary>
		/// <param name="index"></param>
		protected void Heapify(int index = 0)
		{
			if (index < 0)
				return;

			int tempIndex = index;
			int leftIndex = GetLeftIndex(index);
			int rightIndex = GetRightIndex(index);

			if (leftIndex <= m_ElementList.Count - 1 &&
				m_Comparer.Compare(GetLeft(index), m_ElementList[index]) < 0)
				tempIndex = leftIndex;
			if (rightIndex <= m_ElementList.Count - 1 &&
				m_Comparer.Compare(GetRight(index), m_ElementList[tempIndex]) < 0)
				tempIndex = rightIndex;

			if (tempIndex != index)
			{
				T temp = m_ElementList[tempIndex];
				m_ElementList[tempIndex] = m_ElementList[index];
				m_ElementList[index] = temp;

				Heapify(tempIndex);
			}
		}

		protected T GetLeft(int index)
		{
			return m_ElementList[GetLeftIndex(index)];
		}
		protected T GetRight(int index)
		{
			return m_ElementList[GetRightIndex(index)];
		}
		protected T GetParent(int index)
		{
			return m_ElementList[GetParentIndex(index)];
		}
		protected int GetLeftIndex(int index)
		{
			return index * 2 + 1;
		}
		protected int GetRightIndex(int index)
		{
			return index * 2 + 2;
		}
		protected int GetParentIndex(int index)
		{
			return (index - 1) / 2;
		}

		public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			List<T> elementList;
			int index;

			T IEnumerator<T>.Current
			{
				get
				{
					try
					{
						return elementList[index];
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
			}
			object IEnumerator.Current
			{
				get
				{
					try
					{
						return elementList[index];
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
			}

			public Enumerator(List<T> elements)
			{
				elementList = elements;
				index = 0;
			}
			public void Dispose()
			{
				Reset();
			}

			public bool MoveNext()
			{
				return (++index < elementList.Count);
			}
			public void Reset()
			{
				index = 0;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(m_ElementList);
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new Enumerator(m_ElementList);
		}
	}
}