using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisualRange : MonoBehaviour
{
	[Space(10)]
	[SerializeField, ReadOnly]
	protected BoxCollider2D m_Collider;

	[SerializeField, ReadOnly]
	protected GameObject m_Target;
	public GameObject target
	{
		get { return m_Target; }
		protected set { m_Target = value; }
	}

	protected bool m_isLostTarget;
	[SerializeField]
	protected float m_FindTargetTimeLimit;
	protected float m_FindTargetTimer;

	private void Awake()
	{
		Init();
	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			target = collision.gameObject;

			if (m_isLostTarget)
				Debug.Log("타겟 다시 찾음!");

			m_isLostTarget = false;

			Debug.Log("타겟 찾음!");
		}
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			m_isLostTarget = true;
			m_FindTargetTimer = 0.0f;

			Debug.Log("타겟 놓침!");
		}
	}
	private void Update()
	{
		if (m_isLostTarget)
		{
			FindTarget();
		}
	}

	protected virtual void Init()
	{
		if (this.AddOneComponent<BoxCollider2D>(out m_Collider))
		{
			m_Collider.isTrigger = true;
			m_Collider.offset = new Vector2(2.1875f, 0.5f);
			m_Collider.size = new Vector2(5.0f, 1.0f);
		}

		m_isLostTarget = false;

		if (m_FindTargetTimeLimit == 0.0f)
			m_FindTargetTimeLimit = 3.0f;
		m_FindTargetTimer = 0.0f;
	}
	protected virtual void FindTarget()
	{
		Debug.Log("타겟 다시 찾는 중!");
		m_FindTargetTimer += Time.deltaTime;

		if (m_FindTargetTimer >= m_FindTargetTimeLimit)
		{
			Debug.Log("타겟 잃어버림!");
			m_Target = null;
			m_isLostTarget = false;
		}
	}
}