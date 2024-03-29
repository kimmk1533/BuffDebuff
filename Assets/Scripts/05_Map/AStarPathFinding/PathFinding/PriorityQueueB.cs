//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Algorithms
{
	public interface IPriorityQueue<T>
	{
		int Push(T item);
		T Pop();
		T Peek();
		void Update(int i);
	}

	public class PriorityQueueB<T> : IPriorityQueue<T>
	{
		protected List<T> m_InnerList = new List<T>();
		protected IComparer<T> m_Comparer;

		public PriorityQueueB()
		{
			m_Comparer = Comparer<T>.Default;
		}
		public PriorityQueueB(IComparer<T> comparer)
		{
			m_Comparer = comparer;
		}
		public PriorityQueueB(IComparer<T> comparer, int capacity)
		{
			m_Comparer = comparer;
			m_InnerList.Capacity = capacity;
		}

		#region Methods
		protected void SwitchElements(int i, int j)
		{
			var h = m_InnerList[i];
			m_InnerList[i] = m_InnerList[j];
			m_InnerList[j] = h;
		}

		protected virtual int OnCompare(int i, int j)
		{
			return m_Comparer.Compare(m_InnerList[i], m_InnerList[j]);
		}

		/// <summary>
		/// Push an object onto the PQ
		/// </summary>
		/// <param name="O">The new object</param>
		/// <returns>The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ.</returns>
		public int Push(T item)
		{
			int p = m_InnerList.Count, p2;
			m_InnerList.Add(item); // E[p] = O
			do
			{
				if (p == 0)
					break;
				p2 = (p - 1) / 2;
				if (OnCompare(p, p2) < 0)
				{
					SwitchElements(p, p2);
					p = p2;
				}
				else
					break;
			} while (true);
			return p;
		}

		/// <summary>
		/// Get the smallest object and remove it.
		/// </summary>
		/// <returns>The smallest object</returns>
		public T Pop()
		{
			var result = m_InnerList[0];
			int p = 0, p1, p2, pn;
			m_InnerList[0] = m_InnerList[m_InnerList.Count - 1];
			m_InnerList.RemoveAt(m_InnerList.Count - 1);
			do
			{
				pn = p;
				p1 = 2 * p + 1;
				p2 = 2 * p + 2;
				if (m_InnerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
					p = p1;
				if (m_InnerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
					p = p2;

				if (p == pn)
					break;
				SwitchElements(p, pn);
			} while (true);

			return result;
		}

		/// <summary>
		/// Notify the PQ that the object at position i has changed
		/// and the PQ needs to restore order.
		/// Since you dont have access to any indexes (except by using the
		/// explicit IList.this) you should not call this function without knowing exactly
		/// what you do.
		/// </summary>
		/// <param name="i">The index of the changed object.</param>
		public void Update(int i)
		{
			int p = i, pn;
			int p1, p2;
			do  // aufsteigen
			{
				if (p == 0)
					break;
				p2 = (p - 1) / 2;
				if (OnCompare(p, p2) < 0)
				{
					SwitchElements(p, p2);
					p = p2;
				}
				else
					break;
			} while (true);
			if (p < i)
				return;
			do     // absteigen
			{
				pn = p;
				p1 = 2 * p + 1;
				p2 = 2 * p + 2;
				if (m_InnerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
					p = p1;
				if (m_InnerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
					p = p2;

				if (p == pn)
					break;
				SwitchElements(p, pn);
			} while (true);
		}

		/// <summary>
		/// Get the smallest object without removing it.
		/// </summary>
		/// <returns>The smallest object</returns>
		public T Peek()
		{
			if (m_InnerList.Count > 0)
				return m_InnerList[0];
			return default(T);
		}

		public void Clear()
		{
			m_InnerList.Clear();
		}

		public int Count
		{
			get { return m_InnerList.Count; }
		}

		public void RemoveLocation(T item)
		{
			var index = -1;
			for (var i = 0; i < m_InnerList.Count; i++)
			{

				if (m_Comparer.Compare(m_InnerList[i], item) == 0)
					index = i;
			}

			if (index != -1)
				m_InnerList.RemoveAt(index);
		}

		public T this[int index]
		{
			get { return m_InnerList[index]; }
			set
			{
				m_InnerList[index] = value;
				Update(index);
			}
		}
		#endregion
	}
}