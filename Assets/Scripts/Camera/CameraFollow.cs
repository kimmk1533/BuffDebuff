using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public PlayerController2D m_Player;
	public float m_VerticalOffset;
	public float m_LookAheadDstX;
	public float m_LookSmoothTimeX;
	public float m_VerticalSmoothTime;
	public Vector2 m_FocusAreaSize;

	FocusArea m_FocusArea;

	float m_CurrentLookAheadX;
	float m_TargetLookAheadX;
	float m_LookAheadDirX;
	float m_SmoothLookVelocityX;
	float m_SmoothVelocityY;

	bool m_LookAheadStopped;

	private void Start()
	{
		m_FocusArea = new FocusArea(m_Player.collider.bounds, m_FocusAreaSize);
	}
	private void LateUpdate()
	{
		m_FocusArea.Update(m_Player.collider.bounds);

		Vector2 focusPosition = m_FocusArea.center + Vector2.up * m_VerticalOffset;

		if (m_FocusArea.velocity.x != 0)
		{
			m_LookAheadDirX = Mathf.Sign(m_FocusArea.velocity.x);

			if (Mathf.Sign(m_Player.playerInput.x) == Mathf.Sign(m_FocusArea.velocity.x) && m_Player.playerInput.x != 0)
			{
				m_LookAheadStopped = false;
				m_TargetLookAheadX = m_LookAheadDirX * m_LookAheadDstX;
			}
			else
			{
				if (!m_LookAheadStopped)
				{
					m_LookAheadStopped = true;
					m_TargetLookAheadX = m_CurrentLookAheadX + (m_LookAheadDirX * m_LookAheadDstX - m_CurrentLookAheadX) / 4f;
				}
			}
		}

		m_CurrentLookAheadX = Mathf.SmoothDamp(m_CurrentLookAheadX, m_TargetLookAheadX, ref m_SmoothLookVelocityX, m_LookSmoothTimeX);

		focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref m_SmoothVelocityY, m_VerticalSmoothTime);
		focusPosition += Vector2.right * m_CurrentLookAheadX;
		transform.position = (Vector3)focusPosition + Vector3.forward * -10;
	}
	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawCube(m_FocusArea.center, m_FocusAreaSize);
	}

	struct FocusArea
	{
		public Vector2 center;
		public Vector2 velocity;
		float left, right;
		float top, bottom;

		public FocusArea(Bounds targetBounds, Vector2 size)
		{
			left = targetBounds.center.x - size.x / 2;
			right = targetBounds.center.x + size.x / 2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			velocity = Vector2.zero;
			center = new Vector2((left + right) / 2, (top + bottom) / 2);
		}

		public void Update(Bounds targetBounds)
		{
			float shiftX = 0;
			if (targetBounds.min.x < left)
			{
				shiftX = targetBounds.min.x - left;
			}
			else if (targetBounds.max.x > right)
			{
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if (targetBounds.min.y < bottom)
			{
				shiftY = targetBounds.min.y - bottom;
			}
			else if (targetBounds.max.y > top)
			{
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;

			center = new Vector2((left + right) / 2, (top + bottom) / 2);
			velocity = new Vector2(shiftX, shiftY);
		}
	}
}