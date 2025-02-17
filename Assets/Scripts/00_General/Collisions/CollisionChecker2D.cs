using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace BuffDebuff
{
	[RequireComponent(typeof(Collider2D))]
	public class CollisionChecker2D : SerializedMonoBehaviour
	{
		#region 변수
		protected Collider2D m_Collider2D = null;

		[OdinSerialize]
		protected Dictionary<int, Trigger> m_TriggerMap = null;

		[SerializeField]
		protected bool m_IsSimulating = true;

		#endregion

		#region 프로퍼티
		public bool isSimulating
		{
			get => m_IsSimulating;
			set => m_IsSimulating = value;
		}
		public Vector2 offset
		{
			get => m_Collider2D.offset;
			set => m_Collider2D.offset = value;
		}
		public Vector2 size
		{
			get => m_Collider2D.bounds.size;
			set
			{
				Bounds bounds = m_Collider2D.bounds;
				bounds.size = value;
				m_Collider2D.bounds.SetMinMax(bounds.min, bounds.max);
			}
		}

		#region 인덱서
		public Trigger this[int layer]
		{
			get
			{
				if (m_TriggerMap.ContainsKey(layer) == false)
				{
					m_TriggerMap.Add(layer, new Trigger());
				}

				return m_TriggerMap[layer];
			}
		}
		public Trigger this[string layer]
		{
			get
			{
				return this[LayerMask.NameToLayer(layer)];
			}
		}
		#endregion

		#endregion

		#region 이벤트
		public delegate void OnTriggerHandler(Collider2D collider);
		public class Trigger
		{
			public OnTriggerHandler onEnter2D;
			public OnTriggerHandler onStay2D;
			public OnTriggerHandler onExit2D;
		}
		#endregion

		public virtual void Initialize()
		{
			this.NullCheckGetComponent<Collider2D>(ref m_Collider2D);
			m_Collider2D.isTrigger = true;

			if (m_TriggerMap == null)
				m_TriggerMap = new Dictionary<int, Trigger>();

			m_IsSimulating = true;
		}
		public virtual void Finallize()
		{
			foreach (var item in m_TriggerMap)
			{
				item.Value.onEnter2D = null;
				item.Value.onStay2D = null;
				item.Value.onExit2D = null;
			}
			m_TriggerMap.Clear();

			m_IsSimulating = false;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (m_IsSimulating == false)
				return;

			if (m_TriggerMap.ContainsKey(collision.gameObject.layer) == false)
				return;

			m_TriggerMap[collision.gameObject.layer].onEnter2D?.Invoke(collision);
		}
		private void OnTriggerStay2D(Collider2D collision)
		{
			if (m_IsSimulating == false)
				return;

			if (m_TriggerMap.ContainsKey(collision.gameObject.layer) == false)
				return;

			m_TriggerMap[collision.gameObject.layer].onStay2D?.Invoke(collision);
		}
		private void OnTriggerExit2D(Collider2D collision)
		{
			if (m_IsSimulating == false)
				return;

			if (m_TriggerMap.ContainsKey(collision.gameObject.layer) == false)
				return;

			m_TriggerMap[collision.gameObject.layer].onExit2D?.Invoke(collision);
		}
	}
}