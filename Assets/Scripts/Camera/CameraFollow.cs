using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
	[Space(10)]
	public PlayerController2D m_Player;

	[Space(10)]
	public float m_VerticalOffset;
	public float m_LookAheadDstX;
	public float m_LookSmoothTimeX;
	public float m_VerticalSmoothTime;
	public Vector2 m_FocusAreaSize;

	[Space(10)]
	[SerializeField]
	Vector2 m_ClampOffset;
	[SerializeField]
	Vector2 m_ClampAreaSize;

	public Vector2 clampOffset
	{
		get { return m_ClampOffset; }
		set { m_ClampOffset = value; }
	}
	public Vector2 clampAreaSize
	{
		get { return m_ClampAreaSize; }
		set { m_ClampAreaSize = value; }
	}

	Camera m_Camera;

	FocusArea m_FocusArea;

	float m_CurrentLookAheadX;
	float m_TargetLookAheadX;
	float m_LookAheadDirX;
	float m_SmoothLookVelocityX;
	float m_SmoothVelocityY;

	bool m_LookAheadStopped;

	Vector2 m_Screen;
	Vector2 m_MapSize;

	private void Start()
	{
		m_Camera = GetComponent<Camera>();

		m_FocusArea = new FocusArea(m_Player.collider.bounds, m_FocusAreaSize);

		float height = m_Camera.orthographicSize;
		float width = height * Screen.width / Screen.height;
		m_Screen = new Vector2(width, height);
		m_MapSize = m_ClampAreaSize / 2;
	}
	private void LateUpdate()
	{
		m_FocusArea.Update(m_Player.collider.bounds);

		Vector2 focusPosition = m_FocusArea.center + Vector2.up * m_VerticalOffset;

		CameraMove(ref focusPosition);
		Clamp(ref focusPosition);

		transform.position = (Vector3)focusPosition + Vector3.forward * -10;
	}
	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawCube((Application.isPlaying) ? m_FocusArea.center : m_Player.transform.position + Vector3.up * (m_FocusAreaSize.y / 2), m_FocusAreaSize);

		Gizmos.color = new Color(0, 1, 0.5f, 0.1f);
		Gizmos.DrawCube(m_ClampOffset, m_ClampAreaSize);
	}

	void CameraMove(ref Vector2 focusPosition)
	{
		if (m_FocusArea.velocity.x != 0)
		{
			m_LookAheadDirX = Mathf.Sign(m_FocusArea.velocity.x);

			//if (Mathf.Sign(m_Player.playerInput.x) == Mathf.Sign(m_FocusArea.velocity.x) && m_Player.playerInput.x != 0)
			
			if (System.MathF.Sign(m_Player.playerInput.x) == Mathf.Sign(m_FocusArea.velocity.x))
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

	}
	void Clamp(ref Vector2 focusPosition)
	{
		float lx = m_MapSize.x - m_Screen.x;
		float ly = m_MapSize.y - m_Screen.y;

		focusPosition.x = Mathf.Clamp(focusPosition.x, m_ClampOffset.x - lx, m_ClampOffset.x + lx);
		focusPosition.y = Mathf.Clamp(focusPosition.y, m_ClampOffset.y - ly, m_ClampOffset.y + ly);
	}

	public void UpdateClampAreaSize(float x, float y)
	{
		//m_Cl
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