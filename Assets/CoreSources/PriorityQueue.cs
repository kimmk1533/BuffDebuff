using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Collections.Generic
{
	public class PriorityQueue<TElement> : IEnumerable<TElement> where TElement : IComparer<TElement>, new()
	{
		List<TElement> m_ElementList;
		IComparer<TElement> m_Comparer;

		public IComparer<TElement> Comparer
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
				return m_ElementList.Count - 1;
			}
		}

		private TElement root
		{
			get
			{
				return m_ElementList[1];
			}
		}

		public void Clear()
		{
			m_ElementList.Clear();
			m_ElementList.Add(default(TElement));
		}
		public TElement Dequeue()
		{
			// 1. 루트 노드 반환하기 위해 저장
			TElement item = root;

			// 2. 최하단 노드를 루트 노드로 올림
			m_ElementList[1] = m_ElementList[Count];
			m_ElementList.RemoveAt(Count);

			// 3. 루트 노드와 자식 노트들과 값을 비교
			Heapify();

			// 4. 기존 루트 노드 반환
			return item;
		}
		public void Enqueue(TElement element)
		{
			// 1. 최하단 노드에 새로운 원소 추가
			m_ElementList.Add(element);

			// 2. 추가한 원소와 부모 노드와 우선순위 비교
			int index = Count;
			TElement parent = GetParent(index);
			while (index > 1 &&
				Comparer.Compare(parent, element) > 0)
			{
				// 3. 추가한 원소의 우선순위가 더 높다면 부모와 자리교환
				m_ElementList[index] = parent;
				index = GetParentIndex(index);
				parent = GetParent(index);
			}

			// 3-1. 최적화로 자식은 마지막에 1번만 옮김
			m_ElementList[index] = element;

			Heapify();
		}
		public TElement EnqueueDequeue(TElement element)
		{
			Enqueue(element);
			return Dequeue();
		}
		public void EnqueueRange(IEnumerable<TElement> items)
		{
			if (null == items)
				throw new ArgumentNullException(items.ToString());

			foreach (var item in items)
			{
				Enqueue(item);
			}
		}
		public TElement Peek()
		{
			return root;
		}
		public void TrimExcess()
		{
			m_ElementList.TrimExcess();
		}
		public bool TryDequeue(out TElement element)
		{
			if (Count == 0 ||
				root == null)
			{
				element = default(TElement);

				return false;
			}

			element = Dequeue();

			return true;
		}
		public bool TryPeek(out TElement element)
		{
			if (Count == 0 ||
				root == null)
			{
				element = default(TElement);

				return false;
			}

			element = root;

			return true;
		}

		private int GetParentIndex(int index)
		{
			return index / 2;
		}
		private TElement GetParent(int index)
		{
			return m_ElementList[GetParentIndex(index)];
		}
		private int GetLeftIndex(int index)
		{
			return index * 2;
		}
		private TElement GetLeft(int index)
		{
			return m_ElementList[GetLeftIndex(index)];
		}
		private int GetRightIndex(int index)
		{
			return index * 2 + 1;
		}
		private TElement GetRight(int index)
		{
			return m_ElementList[GetRightIndex(index)];
		}

		/// <summary>
		/// Heap 속성을 유지하는 작업
		/// </summary>
		/// <param name="index"></param>
		private void Heapify(int index = 1)
		{
			if (index <= 0)
				return;

			int tempIndex = index;
			int leftIndex = GetLeftIndex(index);
			int rightIndex = GetRightIndex(index);

			if (leftIndex <= Count &&
				m_Comparer.Compare(GetLeft(index), m_ElementList[index]) < 0)
				tempIndex = leftIndex;
			if (rightIndex <= Count &&
				m_Comparer.Compare(GetRight(index), m_ElementList[tempIndex]) < 0)
				tempIndex = rightIndex;

			if (tempIndex != index)
			{
				TElement temp = m_ElementList[tempIndex];
				m_ElementList[tempIndex] = m_ElementList[index];
				m_ElementList[index] = temp;

				Heapify(tempIndex);
			}
		}

		public PriorityQueue()
		{
			m_ElementList = new List<TElement>();
			m_ElementList.Add(default(TElement));

			m_Comparer = new TElement();
		}
		public PriorityQueue(IComparer<TElement>? comparer)
		{
			m_ElementList = new List<TElement>();
			m_ElementList.Add(default(TElement));

			m_Comparer = comparer;
		}
		public PriorityQueue(IEnumerable<TElement> items)
		{
			m_ElementList = new List<TElement>(items);
			m_ElementList.Add(default(TElement));

			m_Comparer = new TElement();
		}
		public PriorityQueue(int initialCapacity)
		{
			m_ElementList = new List<TElement>(initialCapacity);
			m_ElementList.Add(default(TElement));

			m_Comparer = new TElement();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(m_ElementList);
		}
		IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
		{
			return new Enumerator(m_ElementList);
		}

		public class Enumerator : IEnumerator<TElement>
		{
			List<TElement> elementList;
			int index = -1;

			TElement IEnumerator<TElement>.Current
			{
				get
				{
					try
					{
						return elementList[index + 1];
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
						return elementList[index + 1];
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
			}

			public Enumerator(List<TElement> elements)
			{
				elementList = elements;
			}
			public void Dispose()
			{
			}
			public bool MoveNext()
			{
				return (++index < elementList.Count - 1);
			}
			public void Reset()
			{
				index = -1;
			}
		}
	}
	public class PriorityQueue<TElement, TPriority> where TPriority : IComparer<TPriority>, new()
	{
		List<Tuple<TElement, TPriority>> m_ElementList;
		IComparer<TPriority> m_Comparer;

		public IComparer<TPriority> Comparer
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
				return m_ElementList.Count - 1;
			}
		}

		private Tuple<TElement, TPriority> root
		{
			get
			{
				return m_ElementList[1];
			}
		}

		public void Clear()
		{
			m_ElementList.Clear();
			m_ElementList.Add(null);
		}
		public TElement Dequeue()
		{
			// 1. 루트 노드 반환하기 위해 저장
			Tuple<TElement, TPriority> item = root;

			// 2. 최하단 노드를 루트 노드로 올림
			m_ElementList[1] = m_ElementList[Count];
			m_ElementList.RemoveAt(Count);

			// 3. 루트 노드와 자식 노트들과 값을 비교
			Heapify();

			// 4. 기존 루트 노드 반환
			return item.Item1;
		}
		public void Enqueue(TElement element, TPriority priority)
		{
			// 1. 최하단 노드에 새로운 원소 추가
			Tuple<TElement, TPriority> item = new Tuple<TElement, TPriority>(element, priority);
			m_ElementList.Add(item);

			// 2. 추가한 원소와 부모 노드와 우선순위 비교
			int index = Count;
			Tuple<TElement, TPriority> parent = GetParent(index);
			while (index != 1 &&
				Comparer.Compare(parent.Item2, priority) > 0)
			{
				// 3. 추가한 원소의 우선순위가 더 높다면 부모와 자리교환
				m_ElementList[index] = parent;
				index = GetParentIndex(index);
				parent = GetParent(index);
			}

			// 3-1. 최적화로 자식은 마지막에 1번만 옮김
			m_ElementList[index] = item;
		}
		public TElement EnqueueDequeue(TElement element, TPriority priority)
		{
			Enqueue(element, priority);
			return Dequeue();
		}
		public void EnqueueRange(IEnumerable<Tuple<TElement, TPriority>> items)
		{
			if (null == items)
				throw new ArgumentNullException(items.ToString());

			foreach (var item in items)
			{
				Enqueue(item.Item1, item.Item2);
			}
		}
		public void EnqueueRange(IEnumerable<TElement> elements, TPriority priority)
		{
			if (null == elements)
				throw new ArgumentNullException(elements.ToString());

			foreach (var item in elements)
			{
				Enqueue(item, priority);
			}
		}
		public TElement Peek()
		{
			return root.Item1;
		}
		public void TrimExcess()
		{
			m_ElementList.TrimExcess();
		}
		public bool TryDequeue(out TElement element, out TPriority priority)
		{
			if (Count == 0 ||
				root == null)
			{
				element = default(TElement);
				priority = default(TPriority);

				return false;
			}

			Tuple<TElement, TPriority> item = root;

			element = item.Item1;
			priority = item.Item2;

			Dequeue();

			return true;
		}
		public bool TryPeek(out TElement element, out TPriority priority)
		{
			if (Count == 0 ||
				root == null)
			{
				element = default(TElement);
				priority = default(TPriority);

				return false;
			}

			element = root.Item1;
			priority = root.Item2;

			return true;
		}

		private int GetParentIndex(int index)
		{
			return index / 2;
		}
		private Tuple<TElement, TPriority> GetParent(int index)
		{
			return m_ElementList[GetParentIndex(index)];
		}
		private int GetLeftIndex(int index)
		{
			return index * 2;
		}
		private Tuple<TElement, TPriority> GetLeft(int index)
		{
			return m_ElementList[GetLeftIndex(index)];
		}
		private int GetRightIndex(int index)
		{
			return index * 2 + 1;
		}
		private Tuple<TElement, TPriority> GetRight(int index)
		{
			return m_ElementList[GetRightIndex(index)];
		}

		/// <summary>
		/// Heap 속성을 유지하는 작업
		/// </summary>
		/// <param name="index"></param>
		private void Heapify(int index = 1)
		{
			int tempIndex = index;
			int leftIndex = GetLeftIndex(index);
			int rightIndex = GetRightIndex(index);

			if (leftIndex <= Count &&
				m_Comparer.Compare(GetLeft(index).Item2, m_ElementList[index].Item2) < 0)
				tempIndex = leftIndex;
			if (rightIndex <= Count &&
				m_Comparer.Compare(GetRight(index).Item2, m_ElementList[tempIndex].Item2) < 0)
				tempIndex = rightIndex;

			if (tempIndex != index)
			{
				Tuple<TElement, TPriority> temp = m_ElementList[tempIndex];
				m_ElementList[tempIndex] = m_ElementList[index];
				m_ElementList[index] = temp;

				Heapify(tempIndex);
			}
		}

		public PriorityQueue()
		{
			m_ElementList = new List<Tuple<TElement, TPriority>>();
			m_ElementList.Add(null);

			m_Comparer = new TPriority();
		}
		public PriorityQueue(IComparer<TPriority>? comparer)
		{
			m_ElementList = new List<Tuple<TElement, TPriority>>();
			m_ElementList.Add(null);

			m_Comparer = comparer;
		}
		public PriorityQueue(IEnumerable<Tuple<TElement, TPriority>> items)
		{
			m_ElementList = new List<Tuple<TElement, TPriority>>(items);
			m_ElementList.Add(null);

			m_Comparer = new TPriority();
		}
		public PriorityQueue(int initialCapacity)
		{
			m_ElementList = new List<Tuple<TElement, TPriority>>(initialCapacity);
			m_ElementList.Add(null);

			m_Comparer = new TPriority();
		}
	}
}