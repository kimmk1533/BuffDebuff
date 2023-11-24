using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enum;
using static UnityEditor.Progress;

public class WarpPoint : MonoBehaviour
{
	private static readonly Color colliderColor = new Color(145 / 255f, 244 / 255f, 139 / 255f, 192 / 255f);

	#region 변수
	private Room m_Room;

	[SerializeField]
	private List<Collider2D> m_CollisionObjectList;
	[SerializeField]
	private List<Collider2D> m_OldCollisionObjectList;
	[SerializeField]
	private List<Collider2D> m_WarpedObjectList;

	[SerializeField]
	private int m_Index;
	[SerializeField]
	private E_Direction m_Direction;

	[Space(10)]
	[SerializeField]
	private Vector2 m_Offset;
	[SerializeField, Min(0.0001f)]
	private Vector2 m_Size = Vector2.one;
	#endregion

	#region 프로퍼티
	public int index => m_Index;
	public E_Direction direction => m_Direction;
	#endregion

	#region 매니저
	private WarpManager M_Warp => WarpManager.Instance;
	private StageManager M_Stage => StageManager.Instance;
	#endregion

	public void Initialize(Room room)
	{
		m_Room = room;

		m_CollisionObjectList = new List<Collider2D>();
		m_OldCollisionObjectList = new List<Collider2D>();
		m_WarpedObjectList = new List<Collider2D>();

		M_Warp.AddWarpPoint(room, this);
	}
	private void Update()
	{
		CheckCollision();
	}

	private void CheckCollision()
	{
		// 현재 방의 워프포인트만 확인하도록 예외처리
		if (m_Room != M_Stage.currentStage.currentRoom)
			return;

		Vector2 origin = (Vector2)transform.position + m_Offset;
		Vector2 size = m_Size;

		m_OldCollisionObjectList.Clear();
		m_OldCollisionObjectList.AddRange(m_CollisionObjectList);
		m_CollisionObjectList.Clear();
		m_CollisionObjectList.AddRange(Physics2D.OverlapBoxAll(origin, size, 0f, M_Warp.layerMask));

		foreach (var item in m_OldCollisionObjectList)
		{
			if (m_WarpedObjectList.Contains(item) == true &&
				m_CollisionObjectList.Contains(item) == false)
			{
				m_WarpedObjectList.Remove(item);
			}
		}

		foreach (var item in m_CollisionObjectList)
		{
			if (m_WarpedObjectList.Contains(item) == true)
				continue;

			MoveCollisionObject(item);
		}
	}
	private void MoveCollisionObject(Collider2D collisionObject)
	{
		Room nearRoom = m_Room.GetNearRoom(m_Direction);
		E_Direction otherDir = DirEnumUtil.GetOtherDir(m_Direction);
		Vector2Int dirVec = DirEnumUtil.ConvertToVector2Int(m_Direction);

		Dictionary<int, WarpPoint> index_WarpPointMap = nearRoom.GetIndexWarpPointMap(otherDir);
		WarpPoint warpPoint = index_WarpPointMap[m_Index];

		Vector3 offset = (transform.position + (Vector3)m_Offset) - collisionObject.transform.position;

		switch (m_Direction)
		{
			case E_Direction.Up:
			case E_Direction.Down:
				offset.y = Mathf.Sign(offset.y) * Mathf.Min(Mathf.Abs(offset.y), collisionObject.bounds.extents.y);
				offset.y = dirVec.y * Mathf.Max(Mathf.Abs(offset.y), m_Size.y * 0.5f);
				offset.y *= -1f;
				break;
			case E_Direction.Left:
			case E_Direction.Right:
				offset.x = Mathf.Sign(offset.x) * Mathf.Max(Mathf.Abs(offset.x), m_Size.x * 0.5f);
				offset.x *= -1f;
				break;
		}

		collisionObject.transform.position = (warpPoint.transform.position + (Vector3)warpPoint.m_Offset) - offset;

		warpPoint.m_WarpedObjectList.Add(collisionObject);
		warpPoint.m_CollisionObjectList.RemoveAll((obj) => true);
		warpPoint.m_OldCollisionObjectList.RemoveAll((obj) => true);

		StartCoroutine(warpPoint.ClearWarpedObject());

		M_Stage.currentStage.MoveRoom(dirVec);
	}
	IEnumerator ClearWarpedObject()
	{
		yield return new WaitForSeconds(1.0f);

		m_WarpedObjectList.Clear();
	}

	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;

		Gizmos.color = colliderColor;

		Gizmos.DrawWireCube(transform.position + (Vector3)m_Offset, m_Size);

		Gizmos.color = color;
	}
}