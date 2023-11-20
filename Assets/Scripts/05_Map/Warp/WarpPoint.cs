using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enum;

public class WarpPoint : MonoBehaviour
{
	private static readonly Color colliderColor = new Color(145 / 255f, 244 / 255f, 139 / 255f, 192 / 255f);

	#region 변수
	private Room m_Room;

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

		Collider2D[] hits = Physics2D.OverlapBoxAll(origin, size, 0f, M_Warp.layerMask);

		foreach (var item in hits)
		{
			MoveCollisionObject(item.gameObject);
		}
	}
	private void MoveCollisionObject(GameObject collisionObject)
	{
		Debug.Log(collisionObject.name);

		if (M_Warp.CheckLayerMask(collisionObject) == false)
			return;

		Room nearRoom = m_Room.GetNearRoom(m_Direction);

		Dictionary<int, WarpPoint> index_WarpPointMap = nearRoom.GetIndexWarpPointMap(DirEnumUtil.GetOtherDir(m_Direction));
		WarpPoint warpPoint = index_WarpPointMap[m_Index];

		Vector3 offset = transform.position - collisionObject.transform.position;

		collisionObject.transform.position = warpPoint.transform.position + offset;

		M_Stage.currentStage.MoveRoom(DirEnumUtil.ConvertToVector2Int(m_Direction));
	}

	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;

		Gizmos.color = colliderColor;

		Gizmos.DrawWireCube(transform.position + (Vector3)m_Offset, m_Size);

		Gizmos.color = color;
	}
}