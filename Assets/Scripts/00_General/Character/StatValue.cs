using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	[System.Serializable]
	public struct StatValue<T>
	{
		[field:SerializeField]
		public T max { get; set; }
		[field:SerializeField]
		public T current { get; set; }

		public StatValue(T max)
		{
			current = this.max = max;
		}
		public StatValue(T current, T max)
		{
			this.max = max;
			this.current = current;
		}
	}
}