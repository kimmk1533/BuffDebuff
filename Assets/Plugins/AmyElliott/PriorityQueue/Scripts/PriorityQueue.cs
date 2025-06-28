// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Collections/src/System/Collections/Generic/PriorityQueue.cs

#nullable enable

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
	/// <summary>
	///  Represents a min priority queue.
	/// </summary>
	/// <typeparam name="TPriority">Specifies the type of priority associated with enqueued elements.</typeparam>
	/// <typeparam name="TElement">Specifies the type of elements in the queue.</typeparam>
	/// <remarks>
	///  Implements an array-backed quaternary min-heap. Each element is enqueued with an associated priority
	///  that determines the dequeue order: elements with the lowest priority get dequeued first.
	/// </remarks>
	[System.Serializable]
	public class PriorityQueue<TPriority, TElement>
	{
		#region 변수
		/// <summary>
		/// Represents an implicit heap-ordered complete d-ary tree, stored as an array.
		/// </summary>
		[UnityEngine.SerializeField]
		private (TPriority Priority, TElement Element)[] _nodes;

		/// <summary>
		/// Custom comparer used to order the heap.
		/// </summary>
		private readonly IComparer<TPriority>? _comparer;

		/// <summary>
		/// Lazily-initialized collection used to expose the contents of the queue.
		/// </summary>
		private UnorderedItemsCollection? _unorderedItems;

		/// <summary>
		/// The number of nodes in the heap.
		/// </summary>
		private int _size;

		/// <summary>
		/// Version updated on mutation to help validate enumerators operate on a consistent state.
		/// </summary>
		private int _version;

		/// <summary>
		/// Specifies the arity of the d-ary heap, which here is quaternary.
		/// It is assumed that this value is a power of 2.
		/// </summary>
		private const int Arity = 4;

		/// <summary>
		/// The binary logarithm of <see cref="Arity" />.
		/// </summary>
		private const int Log2Arity = 2;
		#endregion

		#region 프로퍼티
		/// <summary>
		///  Gets the number of elements contained in the <see cref="PriorityQueue{TPriority, TElement}"/>.
		/// </summary>
		public int Count => _size;

		/// <summary>
		///  Gets the priority comparer used by the <see cref="PriorityQueue{TPriority, TElement}"/>.
		/// </summary>
		public IComparer<TPriority> Comparer => _comparer ?? Comparer<TPriority>.Default;

		/// <summary>
		///  Gets a collection that enumerates the elements of the queue in an unordered manner.
		/// </summary>
		/// <remarks>
		///  The enumeration does not order items by priority, since that would require N * log(N) time and N space.
		///  Items are instead enumerated following the internal array heap layout.
		/// </remarks>
		public UnorderedItemsCollection UnorderedItems => _unorderedItems ??= new UnorderedItemsCollection(this);
		#endregion

		#region 생성자
#if DEBUG
		static PriorityQueue()
		{
			Debug.Assert(Log2Arity > 0 && Math.Pow(2, Log2Arity) == Arity);
		}
#endif

		/// <summary>
		///  Initializes a new instance of the <see cref="PriorityQueue{TPriority, TElement}"/> class.
		/// </summary>
		public PriorityQueue()
		{
			_nodes = Array.Empty<(TPriority, TElement)>();
			_comparer = InitializeComparer(null);
		}

		/// <summary>
		///  Initializes a new instance of the <see cref="PriorityQueue{TPriority, TElement}"/> class
		///  with the specified initial capacity.
		/// </summary>
		/// <param name="initialCapacity">Initial capacity to allocate in the underlying heap array.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///  The specified <paramref name="initialCapacity"/> was negative.
		/// </exception>
		public PriorityQueue(int initialCapacity)
			: this(initialCapacity, comparer: null)
		{
		}

		/// <summary>
		///  Initializes a new instance of the <see cref="PriorityQueue{TPriority, TElement}"/> class
		///  with the specified custom priority comparer.
		/// </summary>
		/// <param name="comparer">
		///  Custom comparer dictating the ordering of elements.
		///  Uses <see cref="Comparer{T}.Default" /> if the argument is <see langword="null"/>.
		/// </param>
		public PriorityQueue(IComparer<TPriority>? comparer)
		{
			_nodes = Array.Empty<(TPriority, TElement)>();
			_comparer = InitializeComparer(comparer);
		}

		/// <summary>
		///  Initializes a new instance of the <see cref="PriorityQueue{TPriority, TElement}"/> class
		///  with the specified initial capacity and custom priority comparer.
		/// </summary>
		/// <param name="initialCapacity">Initial capacity to allocate in the underlying heap array.</param>
		/// <param name="comparer">
		///  Custom comparer dictating the ordering of elements.
		///  Uses <see cref="Comparer{T}.Default" /> if the argument is <see langword="null"/>.
		/// </param>
		///  The specified <paramref name="initialCapacity"/> was negative.
		/// </exception>
		public PriorityQueue(int initialCapacity, IComparer<TPriority>? comparer)
		{
			_nodes = new (TPriority, TElement)[initialCapacity];
			_comparer = InitializeComparer(comparer);
		}
		#endregion

		/// <summary>
		/// Gets the index of an element's parent.
		/// </summary>
		private static int GetParentIndex(int index) => (index - 1) >> Log2Arity;

		/// <summary>
		/// Gets the index of the first child of an element.
		/// </summary>
		private static int GetFirstChildIndex(int index) => (index << Log2Arity) + 1;

		/// <summary>
		///  Adds the specified element with associated priority to the <see cref="PriorityQueue{TPriority, TElement}"/>.
		/// </summary>
		/// <param name="element">The element to add to the <see cref="PriorityQueue{TPriority, TElement}"/>.</param>
		/// <param name="priority">The priority with which to associate the new element.</param>
		public void Enqueue(TPriority Priority, TElement Element)
		{
			// Virtually add the node at the end of the underlying array.
			// Note that the node being enqueued does not need to be physically placed
			// there at this point, as such an assignment would be redundant.

			int currentSize = _size;
			_version++;

			if (_nodes.Length == currentSize)
			{
				Grow(currentSize + 1);
			}

			_size = currentSize + 1;

			if (_comparer == null)
			{
				MoveUpDefaultComparer((Priority, Element), currentSize);
			}
			else
			{
				MoveUpCustomComparer((Priority, Element), currentSize);
			}
		}

		/// <summary>
		///  Removes and returns the minimal element from the <see cref="PriorityQueue{TPriority, TElement}"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">The queue is empty.</exception>
		/// <returns>The minimal element of the <see cref="PriorityQueue{TPriority, TElement}"/>.</returns>
		public (TPriority Priorty, TElement Element) Dequeue()
		{
			if (_size == 0)
			{
				throw new System.Exception("The queue is empty");
			}

			(TPriority Priorty, TElement Element) element = _nodes[0];
			RemoveRootNode();
			return element;
		}

		public (TPriority Priorty, TElement Element) Peek()
		{
			if (_size == 0)
			{
				throw new System.Exception("The queue is empty");
			}

			(TPriority Priorty, TElement Element) element = _nodes[0];
			//RemoveRootNode();
			return element;
		}

		public void Clear()
		{
			int size = _size;
			for (int i = 0; i < size; ++i)
			{
				Dequeue();
			}
		}

		public bool IsEmpty()
		{
			return Count == 0;
		}

		/// <summary>
		/// Grows the priority queue to match the specified min capacity.
		/// </summary>
		private void Grow(int minCapacity)
		{
			Debug.Assert(_nodes.Length < minCapacity);

			const int GrowFactor = 2;
			const int MinimumGrow = 4;

			int newcapacity = GrowFactor * _nodes.Length;

			// Allow the queue to grow to maximum possible capacity (~2G elements) before encountering overflow.
			// Note that this check works even when _nodes.Length overflowed thanks to the (uint) cast
			//if ((uint)newcapacity > Array.MaxLength) newcapacity = Array.MaxLength;

			// Ensure minimum growth is respected.
			newcapacity = Math.Max(newcapacity, _nodes.Length + MinimumGrow);

			// If the computed capacity is still less than specified, set to the original argument.
			// Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
			if (newcapacity < minCapacity)
				newcapacity = minCapacity;

			Array.Resize(ref _nodes, newcapacity);
		}

		/// <summary>
		/// Removes the node from the root of the heap
		/// </summary>
		private void RemoveRootNode()
		{
			int lastNodeIndex = --_size;
			_version++;

			if (lastNodeIndex > 0)
			{
				(TPriority Priority, TElement Element) lastNode = _nodes[lastNodeIndex];
				if (_comparer == null)
				{
					MoveDownDefaultComparer(lastNode, 0);
				}
				else
				{
					MoveDownCustomComparer(lastNode, 0);
				}
			}

			if (RuntimeHelpers.IsReferenceOrContainsReferences<(TPriority, TElement)>())
			{
				_nodes[lastNodeIndex] = default;
			}
		}

		/// <summary>
		/// Moves a node up in the tree to restore heap order.
		/// </summary>
		private void MoveUpDefaultComparer((TPriority Priority, TElement Element) node, int nodeIndex)
		{
			// Instead of swapping items all the way to the root, we will perform
			// a similar optimization as in the insertion sort.

			Debug.Assert(_comparer is null);
			Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

			(TPriority Priority, TElement Element)[] nodes = _nodes;

			while (nodeIndex > 0)
			{
				int parentIndex = GetParentIndex(nodeIndex);
				(TPriority Priority, TElement Element) parent = nodes[parentIndex];

				if (Comparer<TPriority>.Default.Compare(node.Priority, parent.Priority) < 0)
				{
					nodes[nodeIndex] = parent;
					nodeIndex = parentIndex;
				}
				else
				{
					break;
				}
			}

			nodes[nodeIndex] = node;
		}

		/// <summary>
		/// Moves a node up in the tree to restore heap order.
		/// </summary>
		private void MoveUpCustomComparer((TPriority Priority, TElement Element) node, int nodeIndex)
		{
			// Instead of swapping items all the way to the root, we will perform
			// a similar optimization as in the insertion sort.

			Debug.Assert(_comparer is not null);
			Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

			IComparer<TPriority> comparer = _comparer;
			(TPriority Priority, TElement Element)[] nodes = _nodes;

			while (nodeIndex > 0)
			{
				int parentIndex = GetParentIndex(nodeIndex);
				(TPriority Priority, TElement Element) parent = nodes[parentIndex];

				if (comparer.Compare(node.Priority, parent.Priority) < 0)
				{
					nodes[nodeIndex] = parent;
					nodeIndex = parentIndex;
				}
				else
				{
					break;
				}
			}

			nodes[nodeIndex] = node;
		}

		/// <summary>
		/// Moves a node down in the tree to restore heap order.
		/// </summary>
		private void MoveDownDefaultComparer((TPriority Priority, TElement Element) node, int nodeIndex)
		{
			// The node to move down will not actually be swapped every time.
			// Rather, values on the affected path will be moved up, thus leaving a free spot
			// for this value to drop in. Similar optimization as in the insertion sort.

			Debug.Assert(_comparer is null);
			Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

			(TPriority Priority, TElement Element)[] nodes = _nodes;
			int size = _size;

			int i;
			while ((i = GetFirstChildIndex(nodeIndex)) < size)
			{
				// Find the child node with the minimal priority
				(TPriority Priority, TElement Element) minChild = nodes[i];
				int minChildIndex = i;

				int childIndexUpperBound = Math.Min(i + Arity, size);
				while (++i < childIndexUpperBound)
				{
					(TPriority Priority, TElement Element) nextChild = nodes[i];
					if (Comparer<TPriority>.Default.Compare(nextChild.Priority, minChild.Priority) < 0)
					{
						minChild = nextChild;
						minChildIndex = i;
					}
				}

				// Heap property is satisfied; insert node in this location.
				if (Comparer<TPriority>.Default.Compare(node.Priority, minChild.Priority) <= 0)
				{
					break;
				}

				// Move the minimal child up by one node and
				// continue recursively from its location.
				nodes[nodeIndex] = minChild;
				nodeIndex = minChildIndex;
			}

			nodes[nodeIndex] = node;
		}

		/// <summary>
		/// Moves a node down in the tree to restore heap order.
		/// </summary>
		private void MoveDownCustomComparer((TPriority Priority, TElement Element) node, int nodeIndex)
		{
			// The node to move down will not actually be swapped every time.
			// Rather, values on the affected path will be moved up, thus leaving a free spot
			// for this value to drop in. Similar optimization as in the insertion sort.

			Debug.Assert(_comparer is not null);
			Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

			IComparer<TPriority> comparer = _comparer;
			(TPriority Priority, TElement Element)[] nodes = _nodes;
			int size = _size;

			int i;
			while ((i = GetFirstChildIndex(nodeIndex)) < size)
			{
				// Find the child node with the minimal priority
				(TPriority Priority, TElement Element) minChild = nodes[i];
				int minChildIndex = i;

				int childIndexUpperBound = Math.Min(i + Arity, size);
				while (++i < childIndexUpperBound)
				{
					(TPriority Priority, TElement Element) nextChild = nodes[i];
					if (comparer.Compare(nextChild.Priority, minChild.Priority) < 0)
					{
						minChild = nextChild;
						minChildIndex = i;
					}
				}

				// Heap property is satisfied; insert node in this location.
				if (comparer.Compare(node.Priority, minChild.Priority) <= 0)
				{
					break;
				}

				// Move the minimal child up by one node and continue recursively from its location.
				nodes[nodeIndex] = minChild;
				nodeIndex = minChildIndex;
			}

			nodes[nodeIndex] = node;
		}

		/// <summary>
		/// Initializes the custom comparer to be used internally by the heap.
		/// </summary>
		private static IComparer<TPriority>? InitializeComparer(IComparer<TPriority>? comparer)
		{
			if (typeof(TPriority).IsValueType)
			{
				if (comparer == Comparer<TPriority>.Default)
				{
					// if the user manually specifies the default comparer,
					// revert to using the optimized path.
					return null;
				}

				return comparer;
			}
			else
			{
				// Currently the JIT doesn't optimize direct Comparer<T>.Default.Compare
				// calls for reference types, so we want to cache the comparer instance instead.
				// TODO https://github.com/dotnet/runtime/issues/10050: Update if this changes in the future.
				return comparer ?? Comparer<TPriority>.Default;
			}
		}

		/// <summary>
		///  Enumerates the contents of a <see cref="PriorityQueue{TPriority, TElement}"/>, without any ordering guarantees.
		/// </summary>
		public sealed class UnorderedItemsCollection : IReadOnlyCollection<(TPriority Priority, TElement Element)>, ICollection
		{
			internal readonly PriorityQueue<TPriority, TElement> _queue;

			internal UnorderedItemsCollection(PriorityQueue<TPriority, TElement> queue) => _queue = queue;

			public int Count => _queue._size;
			object ICollection.SyncRoot => this;
			bool ICollection.IsSynchronized => false;

			void ICollection.CopyTo(Array array, int index)
			{
				throw new System.Exception("Null Array");
			}

			/// <summary>
			///  Enumerates the element and priority pairs of a <see cref="PriorityQueue{TPriority, TElement}"/>,
			///  without any ordering guarantees.
			/// </summary>
			public struct Enumerator : IEnumerator<(TPriority Priority, TElement Element)>
			{
				private readonly PriorityQueue<TPriority, TElement> _queue;
				private readonly int _version;
				private int _index;
				private (TPriority, TElement) _current;

				internal Enumerator(PriorityQueue<TPriority, TElement> queue)
				{
					_queue = queue;
					_index = 0;
					_version = queue._version;
					_current = default;
				}

				/// <summary>
				/// Releases all resources used by the <see cref="Enumerator"/>.
				/// </summary>
				public void Dispose() { }

				/// <summary>
				/// Advances the enumerator to the next element of the <see cref="UnorderedItems"/>.
				/// </summary>
				/// <returns><see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the collection.</returns>
				public bool MoveNext()
				{
					PriorityQueue<TPriority, TElement> localQueue = _queue;

					if (_version == localQueue._version && ((uint)_index < (uint)localQueue._size))
					{
						_current = localQueue._nodes[_index];
						_index++;
						return true;
					}

					return MoveNextRare();
				}

				private bool MoveNextRare()
				{
					if (_version != _queue._version)
					{
						throw new System.Exception("SR.InvalidOperation_EnumFailedVersion");
					}

					_index = _queue._size + 1;
					_current = default;
					return false;
				}

				/// <summary>
				/// Gets the element at the current position of the enumerator.
				/// </summary>
				public (TPriority Priority, TElement Element) Current => _current;
				object IEnumerator.Current => _current;

				void IEnumerator.Reset()
				{
					if (_version != _queue._version)
					{
						throw new System.Exception("SR.InvalidOperation_EnumFailedVersion");
					}

					_index = 0;
					_current = default;
				}
			}

			/// <summary>
			/// Returns an enumerator that iterates through the <see cref="UnorderedItems"/>.
			/// </summary>
			/// <returns>An <see cref="Enumerator"/> for the <see cref="UnorderedItems"/>.</returns>
			public Enumerator GetEnumerator() => new Enumerator(_queue);

#pragma warning disable 8603
			IEnumerator<(TPriority Priority, TElement Element)> IEnumerable<(TPriority Priority, TElement Element)>.GetEnumerator() =>
				_queue.Count == 0 ? null :
				GetEnumerator();
#pragma warning restore 8603

			IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<(TPriority Priority, TElement Element)>)this).GetEnumerator();
		}
	}
}