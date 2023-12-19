using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enum;

[RequireComponent(typeof(BoxCollisionChecker2D))]
public class WarpPoint : MonoBehaviour
{
	#region 변수
	private Room m_Room;

	[SerializeField]
	private int m_Index;
	[SerializeField]
	private E_Direction m_Direction;

	private BoxCollisionChecker2D m_CollisionChecker2D;
	#endregion

	#region 프로퍼티
	public int index => m_Index;
	public E_Direction direction => m_Direction;
	#endregion

	#region 이벤트
	public struct WarpArg
	{
		public Room prevRoom;
		public Room currRoom;
		public E_Direction direction;

		public Collider2D warpObject;
	}
	public delegate void OnWarpHandler(WarpArg arg);

	public event OnWarpHandler onWarp;
	#endregion

	#region 매니저
	private static WarpManager M_Warp => WarpManager.Instance;
	private static StageManager M_Stage => StageManager.Instance;
	#endregion

	public void Initialize(Room room)
	{
		m_Room = room;

		#region SAFE_INIT
		this.Safe_GetComponent<BoxCollisionChecker2D>(ref m_CollisionChecker2D);
		m_CollisionChecker2D.Initialize();
		m_CollisionChecker2D["Player"].onEnter2D += MoveCollisionObject;
		#endregion

		M_Warp.AddWarpPoint(room, this);
	}
	public void Finallize()
	{
		onWarp = null;
	}

	//private void Update()
	//{
	//	// 현재 방의 워프포인트만 확인하도록 예외처리
	//	if (m_Room != M_Stage.currentStage.currentRoom)
	//		return;

	//	CheckCollision();
	//}

	//private void CheckCollision()
	//{
	//	Vector2 origin = (Vector2)transform.position + m_Offset;
	//	Vector2 size = m_Size;

	//	m_OldCollisionObjectList.Clear();
	//	m_OldCollisionObjectList.AddRange(m_CollisionObjectList);
	//	m_CollisionObjectList.Clear();
	//	m_CollisionObjectList.AddRange(Physics2D.OverlapBoxAll(origin, size, 0f, M_Warp.layerMask));

	//	foreach (var item in m_OldCollisionObjectList)
	//	{
	//		if (m_WarpedObjectList.Contains(item) == true &&
	//			m_CollisionObjectList.Contains(item) == false)
	//		{
	//			m_WarpedObjectList.Remove(item);
	//		}
	//	}

	//	foreach (var item in m_CollisionObjectList)
	//	{
	//		if (m_WarpedObjectList.Contains(item) == true)
	//			continue;

	//		MoveCollisionObject(item);
	//	}
	//}
	private void MoveCollisionObject(Collider2D collisionObject)
	{
		Room nearRoom = m_Room.GetNearRoom(m_Direction);
		E_Direction otherDir = DirEnumUtil.GetOtherDir(m_Direction);
		Vector2Int dirVec = DirEnumUtil.ConvertToVector2Int(m_Direction);

		Dictionary<int, WarpPoint> index_WarpPointMap = nearRoom.GetIndexWarpPointMap(otherDir);
		WarpPoint warpPoint = index_WarpPointMap[m_Index];

		Vector3 offset = (transform.position + (Vector3)m_CollisionChecker2D.offset) - collisionObject.transform.position;

		switch (m_Direction)
		{
			case E_Direction.Up:
			case E_Direction.Down:
				offset.y = Mathf.Sign(offset.y) * Mathf.Min(Mathf.Abs(offset.y), collisionObject.bounds.extents.y);
				offset.y = dirVec.y * Mathf.Max(Mathf.Abs(offset.y), m_CollisionChecker2D.size.y * 0.5f);
				offset.y *= -1f;
				break;
			case E_Direction.Left:
			case E_Direction.Right:
				offset.x = Mathf.Sign(offset.x) * Mathf.Max(Mathf.Abs(offset.x), m_CollisionChecker2D.size.x * 0.5f);
				offset.x *= -1f;
				break;
		}

		collisionObject.transform.position = (warpPoint.transform.position + (Vector3)warpPoint.m_CollisionChecker2D.offset) - offset;

		warpPoint.m_CollisionChecker2D.isSimulating = false;
		warpPoint.m_CollisionChecker2D.Clear();

		StartCoroutine(warpPoint.TurnOnCollisionChecker());

		M_Stage.currentStage.MoveRoom(dirVec);

		WarpArg warpArg = new WarpArg()
		{
			prevRoom = m_Room,
			currRoom = nearRoom,
			direction = m_Direction,
			warpObject = collisionObject,
		};

		warpPoint.onWarp?.Invoke(warpArg);
	}
	IEnumerator TurnOnCollisionChecker()
	{
		yield return new WaitForSeconds(1.0f);

		m_CollisionChecker2D.isSimulating = true;
	}
}