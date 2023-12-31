using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
	#region 변수
	private Camera m_Camera;

	[Space(10)]
	[SerializeField]
	private PlayerController2D m_PlayerController;

	[Space(10)]
	[SerializeField]
	private float m_VerticalOffset;
	[SerializeField]
	private float m_LookAheadDstX;
	[SerializeField]
	private float m_LookSmoothTimeX;
	[SerializeField]
	private float m_VerticalSmoothTime;
	[SerializeField]
	private Vector2 m_FocusAreaSize;

	[Space(10)]
	[SerializeField]
	private Vector2 m_ClampOffset;
	[SerializeField]
	private Vector2 m_ClampAreaSize;

	private FocusArea m_FocusArea;

	private float m_CurrentLookAheadX;
	private float m_TargetLookAheadX;
	private float m_LookAheadDirX;
	private float m_SmoothLookVelocityX;
	private float m_SmoothVelocityY;

	private bool m_LookAheadStopped;

	private Vector2 m_Screen;
	private Vector2 m_MapSize;
	#endregion

	#region 프로퍼티
	public float verticalOffset
	{
		get => m_VerticalOffset;
		set => m_VerticalOffset = value;
	}
	public float lookAheadDstX
	{
		get => m_LookAheadDstX;
		set => m_LookAheadDstX = value;
	}
	public float lookSmoothTimeX
	{
		get => m_LookSmoothTimeX;
		set => m_LookSmoothTimeX = value;
	}
	public float verticalSmoothTime
	{
		get => m_VerticalSmoothTime;
		set => m_VerticalSmoothTime = value;
	}
	public Vector2 focusAreaSize
	{
		get => m_FocusAreaSize;
		set => m_FocusAreaSize = value;
	}

	public Vector2 clampOffset
	{
		get => m_ClampOffset;
		set => m_ClampOffset = value;
	}
	public Vector2 clampAreaSize
	{
		get => m_ClampAreaSize;
		set => m_ClampAreaSize = value;
	}
	#endregion

	#region 매니저
	private static PlayerManager M_Player => PlayerManager.Instance;
	#endregion

	public void Initialize()
	{
		this.Safe_GetComponentInChilderen<Camera>(ref m_Camera);
		m_PlayerController = M_Player.player.character.controller;
		transform.position = m_PlayerController.transform.position + transform.forward * -10f;

		m_FocusArea = new FocusArea(m_PlayerController.collider.bounds, m_FocusAreaSize);

		float height = m_Camera.orthographicSize;
		float width = height * Screen.width / Screen.height;
		m_Screen = new Vector2(width, height);
		m_MapSize = m_ClampAreaSize / 2;
	}
	//private void Awake()
	//{
	//	Initialize();
	//}

	private void LateUpdate()
	{
		if (m_PlayerController == null)
			return;

		m_FocusArea.Update(m_PlayerController.collider.bounds);

		Vector2 focusPosition = m_FocusArea.center + Vector2.up * m_VerticalOffset;

		CameraMove(ref focusPosition);
		Clamp(ref focusPosition);

		transform.position = (Vector3)focusPosition + Vector3.forward * -10;
	}

	public void UpdateClamp(Vector2 offset, Vector2 size)
	{
		m_ClampOffset = offset;
		m_ClampAreaSize = size;
		m_MapSize = m_ClampAreaSize / 2;

		m_FocusArea.Update(m_PlayerController.collider.bounds);

		Vector2 focusPosition = m_FocusArea.center + Vector2.up * m_VerticalOffset;

		transform.position = (Vector3)focusPosition + Vector3.forward * -10;
	}

	private void CameraMove(ref Vector2 focusPosition)
	{
		if (m_FocusArea.velocity.x != 0f)
		{
			m_LookAheadDirX = System.MathF.Sign(m_FocusArea.velocity.x);

			//if (Mathf.Sign(m_Player.playerInput.x) == Mathf.Sign(m_FocusArea.velocity.x) && m_Player.playerInput.x != 0)

			if (System.MathF.Sign(m_PlayerController.playerInput.x) == System.MathF.Sign(m_FocusArea.velocity.x))
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
	private void Clamp(ref Vector2 focusPosition)
	{
		float lx = m_MapSize.x - m_Screen.x;
		float ly = m_MapSize.y - m_Screen.y;

		focusPosition.x = Mathf.Clamp(focusPosition.x, m_ClampOffset.x - lx, m_ClampOffset.x + lx);
		focusPosition.y = Mathf.Clamp(focusPosition.y, m_ClampOffset.y - ly, m_ClampOffset.y + ly);
	}

	private void OnDrawGizmos()
	{
		if (m_PlayerController == null)
			return;

		Color color = Gizmos.color;

		Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
		Gizmos.DrawCube((Application.isPlaying) ? m_FocusArea.center : m_PlayerController.transform.position + Vector3.up * (m_FocusAreaSize.y / 2), m_FocusAreaSize);

		Gizmos.color = new Color(0.0f, 1.0f, 0.5f, 0.1f);
		Gizmos.DrawCube(m_ClampOffset, m_ClampAreaSize);

		Gizmos.color = color;
	}

	private struct FocusArea
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