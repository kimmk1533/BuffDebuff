using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{
	public LayerMask m_PassengerMask;

	public Vector3[] m_LocalWaypoints;
	Vector3[] m_GlobalWaypoints;

	public float m_Speed;
	public bool m_Cyclic;
	public float m_WaitTime;
	[Range(0, 2)]
	public float m_EaseAmount;

	int m_FromWaypointIndex;
	float m_PercentBetweenWaypoints;
	float m_NextMoveTime;

	List<PassengerMovement> m_PassengerMovement;
	Dictionary<Transform, Controller2D> m_PassengerDictionary = new Dictionary<Transform, Controller2D>();

	protected override void Start()
	{
		base.Start();

		m_GlobalWaypoints = new Vector3[m_LocalWaypoints.Length];
		for (int i = 0; i < m_LocalWaypoints.Length; ++i)
		{
			m_GlobalWaypoints[i] = m_LocalWaypoints[i] + transform.position;
		}
	}
	private void Update()
	{
		UpdateRaycastOrigins();

		Vector3 velocity = CalculatePlatformMovement();

		CalculatePassengerMovement(velocity);

		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
	}

	float Ease(float x)
	{
		float a = m_EaseAmount + 1;

		return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
	}

	Vector3 CalculatePlatformMovement()
	{
		if (Time.time < m_NextMoveTime)
		{
			return Vector3.zero;
		}

		m_FromWaypointIndex %= m_GlobalWaypoints.Length;
		int toWaypointIndex = (m_FromWaypointIndex + 1) % m_GlobalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance(m_GlobalWaypoints[m_FromWaypointIndex], m_GlobalWaypoints[toWaypointIndex]);

		m_PercentBetweenWaypoints += Time.deltaTime * m_Speed / distanceBetweenWaypoints;
		m_PercentBetweenWaypoints = Mathf.Clamp01(m_PercentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Ease(m_PercentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp(m_GlobalWaypoints[m_FromWaypointIndex], m_GlobalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

		if (m_PercentBetweenWaypoints >= 1)
		{
			m_PercentBetweenWaypoints = 0;
			++m_FromWaypointIndex;

			if (!m_Cyclic)
			{
				if (m_FromWaypointIndex >= m_GlobalWaypoints.Length - 1)
				{
					m_FromWaypointIndex = 0;
					System.Array.Reverse(m_GlobalWaypoints);
				}
			}
			m_NextMoveTime = Time.time + m_WaitTime;
		}

		return newPos - transform.position;
	}
	void MovePassengers(bool beforeMovePlatform)
	{
		foreach (PassengerMovement passenger in m_PassengerMovement)
		{
			if (!m_PassengerDictionary.ContainsKey(passenger.transform))
			{
				m_PassengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
			}
			if (passenger.moveBeforePlatform == beforeMovePlatform)
			{
				m_PassengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
			}
		}
	}
	void CalculatePassengerMovement(Vector3 velocity)
	{
		HashSet<Transform> movedPassengers = new HashSet<Transform>();
		m_PassengerMovement = new List<PassengerMovement>();

		float directionX = Mathf.Sign(velocity.x);
		float directionY = Mathf.Sign(velocity.y);

		// Vertically moving platform
		if (velocity.y != 0)
		{
			float rayLength = Mathf.Abs(velocity.y) + skinWidth;

			for (int i = 0; i < m_VerticalRayCount; ++i)
			{
				Vector2 rayOrigin = (directionY == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.topLeft;
				rayOrigin += Vector2.right * (m_VerticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_PassengerMask);

				if (hit && hit.distance != 0)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);

						float pushX = (directionY == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

						m_PassengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
					}
				}
			}
		}

		// Horizontally moving platform
		if (velocity.x != 0)
		{
			float rayLength = Mathf.Abs(velocity.x) + skinWidth;

			for (int i = 0; i < m_HorizontalRayCount; ++i)
			{
				Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (m_HorizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_PassengerMask);

				if (hit && hit.distance != 0)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);

						float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
						float pushY = -skinWidth;

						m_PassengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
					}
				}
			}
		}

		// Passenger on top of a horizontally or downward moving platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
		{
			float rayLength = skinWidth * 2;

			for (int i = 0; i < m_VerticalRayCount; ++i)
			{
				Vector2 rayOrigin = m_RaycastOrigins.topLeft + Vector2.right * (m_VerticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, m_PassengerMask);

				if (hit && hit.distance != 0)
				{
					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);

						float pushX = velocity.x;
						float pushY = velocity.y;

						m_PassengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
				}
			}
		}
	}

	struct PassengerMovement
	{
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
		{
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}

	private void OnDrawGizmos()
	{
		if (m_LocalWaypoints != null)
		{
			Gizmos.color = Color.red;
			float size = 0.3f;

			for (int i = 0; i < m_LocalWaypoints.Length; ++i)
			{
				Vector3 globalWaypointPos = (Application.isPlaying) ? m_GlobalWaypoints[i] : m_LocalWaypoints[i] + transform.position;
				Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
				Gizmos.DrawLine(globalWaypointPos - Vector3.right * size, globalWaypointPos + Vector3.right * size);
			}
		}
	}
}