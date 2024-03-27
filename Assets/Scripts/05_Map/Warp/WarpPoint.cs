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
	public bool isSimulating
	{
		get => m_CollisionChecker2D.isSimulating;
		set => m_CollisionChecker2D.isSimulating = value;
	}
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

		this.Safe_GetComponent<BoxCollisionChecker2D>(ref m_CollisionChecker2D);
		m_CollisionChecker2D.Initialize();

		m_CollisionChecker2D["Player"].onEnter2D += MoveCollisionObject;
		m_CollisionChecker2D.isSimulating = false;

		M_Warp.AddWarpPoint(room, this);
	}
	public void Finallize()
	{
		m_CollisionChecker2D.Finallize();

		onWarp = null;
	}

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

		M_Stage.currentStage.MoveRoom(dirVec);

		warpPoint.m_CollisionChecker2D.isSimulating = false;
		warpPoint.m_CollisionChecker2D.Clear();
		warpPoint.StartCoroutine(warpPoint.TurnOnCollisionChecker());

		WarpArg warpArg = new WarpArg()
		{
			prevRoom = m_Room,
			currRoom = nearRoom,
			direction = m_Direction,
			warpObject = collisionObject,
		};

		warpPoint.onWarp?.Invoke(warpArg);
	}
	private IEnumerator TurnOnCollisionChecker()
	{
		yield return new WaitForSeconds(1.0f);

		m_CollisionChecker2D.isSimulating = true;
	}
}