using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpManager : Singleton<WarpManager>
{
	[System.Flags]
	public enum E_WarpPointPos
	{
		None = 0,

		Left = 1 << 0,
		CenterX = 1 << 1,
		Right = 1 << 2,

		Top = 1 << 3,
		CenterY = 1 << 4,
		Bottom = 1 << 5,
	}

	private static Dictionary<(int x, int y), E_WarpPointPos> m_WarpPointLookupTable_Flag;
	private static Dictionary<(int x, int y), WarpPoint.E_WarpPointPos> m_WarpPointLookupTable_Enum;

	public static E_WarpPointPos WarpPointPos_Enum2Flag(WarpPoint.E_WarpPointPos warpPointPos)
	{
		switch (warpPointPos)
		{
			case WarpPoint.E_WarpPointPos.TopLeft:
				return E_WarpPointPos.Left | E_WarpPointPos.Top;
			case WarpPoint.E_WarpPointPos.Top:
				return E_WarpPointPos.CenterX | E_WarpPointPos.Top;
			case WarpPoint.E_WarpPointPos.TopRight:
				return E_WarpPointPos.Right | E_WarpPointPos.Top;
			case WarpPoint.E_WarpPointPos.Left:
				return E_WarpPointPos.Left | E_WarpPointPos.CenterY;
			case WarpPoint.E_WarpPointPos.Center:
				return E_WarpPointPos.CenterX | E_WarpPointPos.CenterY;
			case WarpPoint.E_WarpPointPos.Right:
				return E_WarpPointPos.Right | E_WarpPointPos.CenterY;
			case WarpPoint.E_WarpPointPos.BottomLeft:
				return E_WarpPointPos.Left | E_WarpPointPos.Bottom;
			case WarpPoint.E_WarpPointPos.Bottom:
				return E_WarpPointPos.CenterX | E_WarpPointPos.Bottom;
			case WarpPoint.E_WarpPointPos.BottomRight:
				return E_WarpPointPos.Right | E_WarpPointPos.Bottom;
		}

		return E_WarpPointPos.None;
	}
	public static WarpPoint.E_WarpPointPos WarpPointPos_Flag2Enum(E_WarpPointPos warpPointPos)
	{
		int x = 10;
		int y = 10;

		if (warpPointPos.HasFlag(E_WarpPointPos.Left))
		{
			x = -1;
		}
		if (warpPointPos.HasFlag(E_WarpPointPos.CenterX))
		{
			if (x != 10)
				return WarpPoint.E_WarpPointPos.None;

			x = 0;
		}
		if (warpPointPos.HasFlag(E_WarpPointPos.Right))
		{
			if (x != 10)
				return WarpPoint.E_WarpPointPos.None;

			x = 1;
		}

		if (warpPointPos.HasFlag(E_WarpPointPos.Top))
		{
			y = 1;
		}
		if (warpPointPos.HasFlag(E_WarpPointPos.CenterY))
		{
			if (y != 10)
				return WarpPoint.E_WarpPointPos.None;

			y = 0;
		}
		if (warpPointPos.HasFlag(E_WarpPointPos.Bottom))
		{
			if (y != 10)
				return WarpPoint.E_WarpPointPos.None;

			y = -1;
		}

		return m_WarpPointLookupTable_Enum[(x, y)];
	}

	public static E_WarpPointPos GetWarpPointPos(E_WarpPointPos warpPointPos, WarpPoint.E_Direction dir)
	{
		int x = 10;
		int y = 10;
		int dirX = (dir == WarpPoint.E_Direction.Left ? -1 : (dir == WarpPoint.E_Direction.Right ? 1 : 0));
		int dirY = (dir == WarpPoint.E_Direction.Down ? -1 : (dir == WarpPoint.E_Direction.Up ? 1 : 0));

		switch (warpPointPos)
		{
			case WarpPoint.E_WarpPointPos.TopLeft:
				x = -1;
				y = 1;
				break;
			case WarpPoint.E_WarpPointPos.Top:

				break;
			case WarpPoint.E_WarpPointPos.TopRight:
				break;
			case WarpPoint.E_WarpPointPos.Left:
				break;
			case WarpPoint.E_WarpPointPos.Center:
				break;
			case WarpPoint.E_WarpPointPos.Right:
				break;
			case WarpPoint.E_WarpPointPos.BottomLeft:
				break;
			case WarpPoint.E_WarpPointPos.Bottom:
				break;
			case WarpPoint.E_WarpPointPos.BottomRight:
				break;
		}

		if (warpPointPos.HasFlag(E_WarpPointPos.Left))
		{
			x = -1;
		}
		if (warpPointPos.HasFlag(E_WarpPointPos.CenterX))
		{
			if (x != 10)
				return E_WarpPointPos.None;

			x = 0;
		}
		if (warpPointPos.HasFlag(E_WarpPointPos.Right))
		{
			if (x != 10)
				return E_WarpPointPos.None;

			x = 1;
		}

		if (warpPointPos.HasFlag(E_WarpPointPos.Top))
		{
			y = 1;
		}
		if (warpPointPos.HasFlag(E_WarpPointPos.CenterY))
		{
			if (y != 10)
				return E_WarpPointPos.None;

			y = 0;
		}
		if (warpPointPos.HasFlag(E_WarpPointPos.Bottom))
		{
			if (y != 10)
				return E_WarpPointPos.None;

			y = -1;
		}

		return E_WarpPointPos.None;
	}

	public void Initialize()
	{
		m_WarpPointLookupTable_Flag = new Dictionary<(int, int), E_WarpPointPos>();

		for (int y = 1; y >= -1; --y)
		{
			for (int x = -1; x <= 1; ++x)
			{
				m_WarpPointLookupTable_Flag.Add((x, y), E_WarpPointPos.None);
			}
		}

		for (int x = -1; x <= 1; ++x)
		{
			m_WarpPointLookupTable_Flag[(x, 1)] |= E_WarpPointPos.Top;
			m_WarpPointLookupTable_Flag[(x, 0)] |= E_WarpPointPos.CenterY;
			m_WarpPointLookupTable_Flag[(x, -1)] |= E_WarpPointPos.Bottom;
		}
		for (int y = -1; y <= 1; ++y)
		{
			m_WarpPointLookupTable_Flag[(-1, y)] |= E_WarpPointPos.Left;
			m_WarpPointLookupTable_Flag[(0, y)] |= E_WarpPointPos.CenterX;
			m_WarpPointLookupTable_Flag[(1, y)] |= E_WarpPointPos.Right;
		}

		m_WarpPointLookupTable_Enum = new Dictionary<(int x, int y), WarpPoint.E_WarpPointPos>();

		m_WarpPointLookupTable_Enum[(-1, 1)] = WarpPoint.E_WarpPointPos.TopLeft;
		m_WarpPointLookupTable_Enum[(0, 1)] = WarpPoint.E_WarpPointPos.Top;
		m_WarpPointLookupTable_Enum[(1, 1)] = WarpPoint.E_WarpPointPos.TopRight;

		m_WarpPointLookupTable_Enum[(-1, 0)] = WarpPoint.E_WarpPointPos.Left;
		m_WarpPointLookupTable_Enum[(0, 0)] = WarpPoint.E_WarpPointPos.Center;
		m_WarpPointLookupTable_Enum[(1, 0)] = WarpPoint.E_WarpPointPos.Right;

		m_WarpPointLookupTable_Enum[(-1, -1)] = WarpPoint.E_WarpPointPos.BottomLeft;
		m_WarpPointLookupTable_Enum[(0, -1)] = WarpPoint.E_WarpPointPos.Bottom;
		m_WarpPointLookupTable_Enum[(1, -1)] = WarpPoint.E_WarpPointPos.BottomRight;
	}
}