using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public abstract class CollisionChecker2D : MonoBehaviour
{
	protected static readonly Color c_ColliderColor = new Color(145 / 255f, 244 / 255f, 139 / 255f, 192 / 255f);

	#region Enum
	public enum E_ColliderType
	{
		BoxCollider2D,
		CircleCollider2D,

		Max
	}
	#endregion

	#region 변수
	[SerializeField, ReadOnly]
	protected E_ColliderType m_ColliderType;

	[SerializeField]
	protected bool m_IsSimulating = true;

	[SerializeField, ReadOnly]
	protected List<Collider2D> m_CollisionObjectList;
	[SerializeField, ReadOnly]
	protected List<Collider2D> m_OldCollisionObjectList;

	[SerializeField, ReadOnly]
	protected LayerMask m_LayerMask;
	protected Dictionary<int, Trigger> m_TriggerMap;

	[Space(10)]
	[SerializeField]
	protected Vector2 m_Offset;
	#endregion

	#region 프로퍼티
	public E_ColliderType colliderType => m_ColliderType;

	public bool isSimulating
	{
		get => m_IsSimulating;
		set => m_IsSimulating = value;
	}
	public Vector2 offset
	{
		get => m_Offset;
		set => m_Offset = value;
	}

	#region 인덱서
	public Trigger this[int layer]
	{
		get
		{
			if (m_TriggerMap.ContainsKey(layer) == false)
			{
				m_LayerMask |= 1 << layer;
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
		if (m_CollisionObjectList != null)
			m_CollisionObjectList.Clear();
		else
			m_CollisionObjectList = new List<Collider2D>();

		if (m_OldCollisionObjectList != null)
			m_OldCollisionObjectList.Clear();
		else
			m_OldCollisionObjectList = new List<Collider2D>();

		if (m_TriggerMap == null)
			m_TriggerMap = new Dictionary<int, Trigger>();
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

		m_LayerMask = LayerMask.GetMask();
	}

	protected virtual void Update()
	{
		if (m_IsSimulating == false)
			return;

		if (m_TriggerMap.Count <= 0)
			return;

		CheckCollision();

		ResolveCollision();
	}

	public void Clear()
	{
		m_CollisionObjectList.Clear();
		m_OldCollisionObjectList.Clear();
	}

	protected virtual void CheckCollision()
	{
		m_OldCollisionObjectList.Clear();
		m_OldCollisionObjectList.AddRange(m_CollisionObjectList);

		m_CollisionObjectList.Clear();
		m_CollisionObjectList.AddRange(GetCollisionColliders());
	}
	protected abstract Collider2D[] GetCollisionColliders();
	protected virtual void ResolveCollision()
	{
		foreach (var item in m_CollisionObjectList)
		{
			if (m_OldCollisionObjectList.Contains(item) == false)
				this[item.gameObject.layer].onEnter2D?.Invoke(item);
			else
				this[item.gameObject.layer].onStay2D?.Invoke(item);
		}

		foreach (var item in m_OldCollisionObjectList)
		{
			if (m_CollisionObjectList.Contains(item) == false)
				this[item.gameObject.layer].onExit2D?.Invoke(item);
		}
	}

	protected abstract void OnDrawGizmosSelected();
}