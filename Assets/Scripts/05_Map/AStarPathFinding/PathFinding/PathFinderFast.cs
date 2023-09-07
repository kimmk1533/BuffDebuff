//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Algorithms
{
	#region Enum
	public enum E_HeuristicFormula
	{
		Manhattan = 1,
		MaxDXDY = 2,
		DiagonalShortCut = 3,
		Euclidean = 4,
		EuclideanNoSQR = 5,
		Custom1 = 6
	}
	#endregion

	public struct Location
	{
		public Location(int xy, int z)
		{
			this.xy = xy;
			this.z = z;
		}

		public int xy;
		public int z;
	}

	public class PathFinderFast
	{
		#region 구조체
		internal struct PathFinderNodeFast
		{
			#region 변수 선언
			public int F;            // F = 이동한 값 + Heuristic
			public int G;
			public ushort PX;        // 부모 x
			public ushort PY;        // 부모 y
			public byte Status;
			public byte PZ;          // 부모 z
			public short JumpLength; // 점프 값
			#endregion

			public PathFinderNodeFast UpdateStatus(byte newStatus)
			{
				PathFinderNodeFast newNode = this;
				newNode.Status = newStatus;
				return newNode;
			}
		}
		#endregion

		#region 변수 선언
		// lock key
		private readonly object _lockObject = new object();

		private List<PathFinderNodeFast>[] m_Nodes;
		// 지나간 위치들 기억할 컨테이너(초기화 시 전체가 아닌 지나간 위치들만 초기화 하면 됨)
		private Stack<int> m_TouchedLocations;

		// Heap variables are initializated to default, but I like to do it anyway
		private byte[,] m_Grid = null;
		private PriorityQueue<Location> m_Open = null;
		private List<Vector2Int> m_Close = null;

		private bool m_Stop = false;
		private bool m_Stopped = true;
		private E_HeuristicFormula m_Formula = E_HeuristicFormula.Manhattan;
		private bool m_Diagonals = true; // 대각선 이동 가능 여부
		private int m_HEstimate = 2;
		private bool m_PunishChangeDirection = false;
		private bool m_TieBreaker = false;
		private bool m_HeavyDiagonals = false;
		private int m_SearchLimit = 2000;
		private double m_CompletedTime = 0;
		private bool m_DebugProgress = false;
		private bool m_DebugFoundPath = false;
		private byte m_OpenNodeValue = 1;
		private byte m_CloseNodeValue = 2;

		// Promoted local variables to member variables to avoid recreation between calls
		private int m_H = 0;
		private Location m_Location;
		private int m_NewLocation = 0;
		private PathFinderNodeFast m_Node;
		private ushort m_LocationX = 0;
		private ushort m_LocationY = 0;
		private ushort m_NewLocationX = 0;
		private ushort m_NewLocationY = 0;
		private int m_CloseNodeCounter = 0;
		private ushort m_GridX = 0;
		private ushort m_GridY = 0;
		private ushort m_GridXLog2 = 0;
		private bool m_Found = false;
		private sbyte[,] m_Direction = new sbyte[8, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
		private int m_EndLocation = 0;
		private int m_NewG = 0;

		public Map m_Map;
		#endregion

		#region 생성자
		public PathFinderFast(byte[,] grid, Map map)
		{
			if (map == null)
				throw new Exception("Map can`t be null");
			if (grid == null)
				throw new Exception("Grid can`t be null");

			m_Map = map;
			m_Grid = grid;
			m_GridX = (ushort)(m_Grid.GetUpperBound(1) + 1);
			m_GridY = (ushort)(m_Grid.GetUpperBound(0) + 1);
			m_GridXLog2 = (ushort)Math.Log(m_GridX, 2);

			if (Math.Log(m_GridX, 2) != (int)Math.Log(m_GridX, 2) ||
				Math.Log(m_GridY, 2) != (int)Math.Log(m_GridY, 2))
				throw new Exception("Invalid Grid, size in X and Y must be power of 2");

			if (m_Nodes == null || m_Nodes.Length != (m_GridX * m_GridY))
			{
				m_Nodes = new List<PathFinderNodeFast>[m_GridX * m_GridY];
				m_TouchedLocations = new Stack<int>(m_GridX * m_GridY);
				m_Close = new List<Vector2Int>(m_GridX * m_GridY);
			}

			for (var i = 0; i < m_Nodes.Length; ++i)
				m_Nodes[i] = new List<PathFinderNodeFast>(1);

			m_Open = new PriorityQueue<Location>(new ComparePFNodeMatrix(m_Nodes));
		}
		#endregion

		#region 프로퍼티
		public bool Stopped
		{
			get { return m_Stopped; }
		}
		public E_HeuristicFormula Formula
		{
			get { return m_Formula; }
			set { m_Formula = value; }
		}
		public bool Diagonals
		{
			get { return m_Diagonals; }
			set
			{
				m_Diagonals = value;
				if (m_Diagonals)
					m_Direction = new sbyte[8, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
				else
					m_Direction = new sbyte[4, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
			}
		}
		public bool HeavyDiagonals
		{
			get { return m_HeavyDiagonals; }
			set { m_HeavyDiagonals = value; }
		}
		public int HeuristicEstimate
		{
			get { return m_HEstimate; }
			set { m_HEstimate = value; }
		}
		public bool PunishChangeDirection
		{
			get { return m_PunishChangeDirection; }
			set { m_PunishChangeDirection = value; }
		}
		public bool TieBreaker
		{
			get { return m_TieBreaker; }
			set { m_TieBreaker = value; }
		}
		public int SearchLimit
		{
			get { return m_SearchLimit; }
			set { m_SearchLimit = value; }
		}
		public double CompletedTime
		{
			get { return m_CompletedTime; }
			set { m_CompletedTime = value; }
		}
		public bool DebugProgress
		{
			get { return m_DebugProgress; }
			set { m_DebugProgress = value; }
		}
		public bool DebugFoundPath
		{
			get { return m_DebugFoundPath; }
			set { m_DebugFoundPath = value; }
		}
		#endregion

		#region 메서드
		private void CalculateH(Vector2Int end)
		{
			switch (m_Formula)
			{
				default:
				case E_HeuristicFormula.Manhattan:
					m_H = m_HEstimate * (Math.Abs(m_NewLocationX - end.x) + Math.Abs(m_NewLocationY - end.y));
					break;
				case E_HeuristicFormula.MaxDXDY:
					m_H = m_HEstimate * (Math.Max(Math.Abs(m_NewLocationX - end.x), Math.Abs(m_NewLocationY - end.y)));
					break;
				case E_HeuristicFormula.DiagonalShortCut:
					int h_diagonal = Math.Min(Math.Abs(m_NewLocationX - end.x), Math.Abs(m_NewLocationY - end.y));
					int h_straight = (Math.Abs(m_NewLocationX - end.x) + Math.Abs(m_NewLocationY - end.y));
					m_H = (m_HEstimate * 2) * h_diagonal + m_HEstimate * (h_straight - 2 * h_diagonal);
					break;
				case E_HeuristicFormula.Euclidean:
					m_H = (int)(m_HEstimate * Math.Sqrt(Math.Pow((m_NewLocationY - end.x), 2) + Math.Pow((m_NewLocationY - end.y), 2)));
					break;
				case E_HeuristicFormula.EuclideanNoSQR:
					m_H = (int)(m_HEstimate * (Math.Pow((m_NewLocationX - end.x), 2) + Math.Pow((m_NewLocationY - end.y), 2)));
					break;
				case E_HeuristicFormula.Custom1:
					Vector2Int dxy = new Vector2Int(Math.Abs(end.x - m_NewLocationX), Math.Abs(end.y - m_NewLocationY));
					int Orthogonal = Math.Abs(dxy.x - dxy.y);
					int Diagonal = Math.Abs(((dxy.x + dxy.y) - Orthogonal) / 2);
					m_H = m_HEstimate * (Diagonal + Orthogonal + dxy.x + dxy.y);
					break;
			}
		}

		public void FindPathStop()
		{
			m_Stop = true;
		}
		public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, int characterWidth, int characterHeight, short maxCharacterJumpHeight)
		{
			Monitor.Enter(_lockObject);

			try
			{
				while (m_TouchedLocations.Count > 0)
					m_Nodes[m_TouchedLocations.Pop()].Clear();

				if (m_Grid[end.y, end.x] == 0)
					return null;

				m_Found = false;
				m_Stop = false;
				m_Stopped = false;
				m_CloseNodeCounter = 0;
				m_OpenNodeValue += 2;
				m_CloseNodeValue += 2;
				m_Open.Clear();

				m_Location.xy = (start.y << m_GridXLog2) + start.x;
				m_Location.z = 0;
				m_EndLocation = (end.y << m_GridXLog2) + end.x;

				PathFinderNodeFast firstNode = new PathFinderNodeFast();
				firstNode.G = 0;
				firstNode.F = m_HEstimate;
				firstNode.PX = (ushort)start.x;
				firstNode.PY = (ushort)start.y;
				firstNode.PZ = 0;
				firstNode.Status = m_OpenNodeValue;

				if (m_Map.IsGround(start.x, start.y - 1))
					firstNode.JumpLength = 0;
				else
					firstNode.JumpLength = (short)(maxCharacterJumpHeight * 2);

				m_Nodes[m_Location.xy].Add(firstNode);
				m_TouchedLocations.Push(m_Location.xy);

				m_Open.Push(m_Location);

				while (m_Open.Count > 0 && m_Stop == false)
				{
					m_Location = m_Open.Pop();

					// 닫힌 리스트에 포함되어있는 지 확인. 만약 그렇다면 해당 노드는 이미 처리 완료
					if (m_Nodes[m_Location.xy][m_Location.z].Status == m_CloseNodeValue)
						continue;

					m_LocationX = (ushort)(m_Location.xy & (m_GridX - 1));
					m_LocationY = (ushort)(m_Location.xy >> m_GridXLog2);

					if (m_Location.xy == m_EndLocation)
					{
						m_Nodes[m_Location.xy][m_Location.z] = m_Nodes[m_Location.xy][m_Location.z].UpdateStatus(m_CloseNodeValue);
						m_Found = true;
						break;
					}

					if (m_CloseNodeCounter > m_SearchLimit)
					{
						m_Stopped = true;
						return null;
					}

					// Lets calculate each successors
					for (int i = 0; i < (m_Diagonals ? 8 : 4); ++i)
					{
						m_NewLocationX = (ushort)(m_LocationX + m_Direction[i, 0]);
						m_NewLocationY = (ushort)(m_LocationY + m_Direction[i, 1]);
						m_NewLocation = (m_NewLocationY << m_GridXLog2) + m_NewLocationX;

						bool onGround = false;
						bool atCeiling = false;

						if (m_Grid[m_NewLocationY, m_NewLocationX] == 0)
							continue;

						if (m_Map.IsGround(m_NewLocationX, m_NewLocationY - 1))
							onGround = true;
						else if (m_Grid[m_NewLocationX, m_NewLocationY + characterHeight] == 0)
							atCeiling = true;

						// calculate a proper jumplength value for the successor
						var jumpLength = m_Nodes[m_Location.xy][m_Location.z].JumpLength;
						short newJumpLength = jumpLength;

						if (atCeiling)
						{
							if (m_NewLocationX != m_LocationX)
								newJumpLength = (short)Mathf.Max(maxCharacterJumpHeight * 2 + 1, jumpLength + 1);
							else
								newJumpLength = (short)Mathf.Max(maxCharacterJumpHeight * 2, jumpLength + 2);
						}
						else if (onGround)
							newJumpLength = 0;
						else if (m_NewLocationY > m_LocationY)
						{
							if (jumpLength < 2) //first jump is always two block up instead of one up and optionally one to either right or left
								newJumpLength = 3;
							else if (jumpLength % 2 == 0)
								newJumpLength = (short)(jumpLength + 2);
							else
								newJumpLength = (short)(jumpLength + 1);
						}
						else if (m_NewLocationY < m_LocationY)
						{
							if (jumpLength % 2 == 0)
								newJumpLength = (short)Mathf.Max(maxCharacterJumpHeight * 2, jumpLength + 2);
							else
								newJumpLength = (short)Mathf.Max(maxCharacterJumpHeight * 2, jumpLength + 1);
						}
						else if (!onGround && m_NewLocationX != m_LocationX)
							newJumpLength = (short)(jumpLength + 1);

						if (jumpLength >= 0 && jumpLength % 2 != 0 && m_LocationX != m_NewLocationX)
							continue;

						//if we're falling and succeor's height is bigger than ours, skip that successor
						if (jumpLength >= maxCharacterJumpHeight * 2 && m_NewLocationY > m_LocationY)
							continue;

						if (newJumpLength >= maxCharacterJumpHeight * 2 + 6 && m_NewLocationX != m_LocationX && (newJumpLength - (maxCharacterJumpHeight * 2 + 6)) % 8 != 3)
							continue;

						m_NewG = m_Nodes[m_Location.xy][m_Location.z].G + m_Grid[m_NewLocationX, m_NewLocationY] + newJumpLength / 4;

						if (m_Nodes[m_NewLocation].Count > 0)
						{
							int lowestJump = short.MaxValue;
							bool couldMoveSideways = false;
							for (int j = 0; j < m_Nodes[m_NewLocation].Count; ++j)
							{
								if (m_Nodes[m_NewLocation][j].JumpLength < lowestJump)
									lowestJump = m_Nodes[m_NewLocation][j].JumpLength;

								if (m_Nodes[m_NewLocation][j].JumpLength % 2 == 0 && m_Nodes[m_NewLocation][j].JumpLength < maxCharacterJumpHeight * 2 + 6)
									couldMoveSideways = true;
							}

							if (lowestJump <= newJumpLength && (newJumpLength % 2 != 0 || newJumpLength >= maxCharacterJumpHeight * 2 + 6 || couldMoveSideways))
								continue;
						}

						CalculateH(end);

						PathFinderNodeFast newNode = new PathFinderNodeFast();
						newNode.JumpLength = newJumpLength;
						newNode.PX = m_LocationX;
						newNode.PY = m_LocationY;
						newNode.PZ = (byte)m_Location.z;
						newNode.G = m_NewG;
						newNode.F = m_NewG + m_H;
						newNode.Status = m_OpenNodeValue;

						if (m_Nodes[m_NewLocation].Count == 0)
							m_TouchedLocations.Push(m_NewLocation);

						m_Nodes[m_NewLocation].Add(newNode);
						m_Open.Push(new Location(m_NewLocation, m_Nodes[m_NewLocation].Count - 1));
					}

					m_Nodes[m_Location.xy][m_Location.z] = m_Nodes[m_Location.xy][m_Location.z].UpdateStatus(m_CloseNodeValue);
					++m_CloseNodeCounter;
				}

				// 경로 탐색 종료. 필터링 시작
				if (m_Found)
				{
					// 닫힌 리스트 초기화
					m_Close.Clear();

					int posX = end.x;
					int posY = end.y;

					Vector2Int node = end;
					Vector2Int prevNode = end;

					PathFinderNodeFast prevNodeTmp = new PathFinderNodeFast();
					PathFinderNodeFast nodeTmp = m_Nodes[m_EndLocation][0];

					int loc = (nodeTmp.PY << m_GridXLog2) + nodeTmp.PX;

					// 길 담을 리스트에 노드 담기
					while (node.x != nodeTmp.PX || node.y != nodeTmp.PY)
					{
						PathFinderNodeFast nextNodeTmp = m_Nodes[loc][nodeTmp.PZ];

						// 필터링 조건
						/*
						 * 1. 시작 노드
						 * 2. 끝 노드
						 * 3. 점프 노드
						 * 4. 옆으로 이동하는 점프에서 첫 번째 공중 노드 (점프 값이 3 인 노드)
						 * 5. 착지 노드 (점프 값이 0이 되는 노드)
						 * 6. 점프의 최고점 (위로 이동하는 노드와 아래로 떨어지는 노드 사이의 노드)
						 * 7. 장애물을 우회하는 노드﻿
						 */
						bool filter =
							// 끝 노드 필터링
							(m_Close.Count == 0)
							// 
							|| (m_Map.IsOneWayPlatform(node.x, node.y - 1))
							|| (m_Grid[node.x, node.y - 1] == 0 && m_Map.IsOneWayPlatform(prevNode.x, prevNode.y - 1))
							// 점프 노드 필터링
							|| (nodeTmp.JumpLength == 0 && prevNodeTmp.JumpLength != 0)
							|| (nodeTmp.JumpLength == 3)
							// 착지 노드 필터링
							|| (nextNodeTmp.JumpLength != 0 && nodeTmp.JumpLength == 0)
							// 최고점 노드 필터링
							|| (node.y > m_Close[m_Close.Count - 1].y && node.y > nodeTmp.PY)
							// 
							|| (node.y < m_Close[m_Close.Count - 1].y && node.y < nodeTmp.PY)
							// 장애물 우회 노드 필터링
							|| ((m_Map.IsGround(node.x - 1, node.y) || m_Map.IsGround(node.x + 1, node.y))
								&& node.y != m_Close[m_Close.Count - 1].y && node.x != m_Close[m_Close.Count - 1].x);

						if (filter)
							m_Close.Add(node);

						prevNode = node;
						posX = nodeTmp.PX;
						posY = nodeTmp.PY;
						prevNodeTmp = nodeTmp;
						nodeTmp = nextNodeTmp;
						loc = (nodeTmp.PY << m_GridXLog2) + nodeTmp.PX;
						node = new Vector2Int(posX, posY);
					}

					// 시작 노드 필터링
					m_Close.Add(node);

					m_Stopped = true;

					return m_Close;
				}

				m_Stopped = true;

				return null;
			}
			finally
			{
				Monitor.Exit(_lockObject);
			}
		}
		#endregion

		#region 내부 클래스
		// 비교자
		internal class ComparePFNodeMatrix : IComparer<Location>
		{
			#region 변수 선언
			List<PathFinderNodeFast>[] m_Matrix;
			#endregion

			#region 생성자
			public ComparePFNodeMatrix(List<PathFinderNodeFast>[] matrix)
			{
				m_Matrix = matrix;
			}
			#endregion

			#region IComparer 멤버
			public int Compare(Location a, Location b)
			{
				if (m_Matrix[a.xy][a.z].F > m_Matrix[b.xy][b.z].F)
					return 1;
				else if (m_Matrix[a.xy][a.z].F < m_Matrix[b.xy][b.z].F)
					return -1;
				return 0;
			}
			#endregion
		}
		#endregion
	}
}