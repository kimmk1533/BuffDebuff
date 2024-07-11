using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorkSheetData
{
	public bool enabled = true;

	[Tooltip("불러올 시트명")]
	public string sheetName;
	[Tooltip("저장할 파일명")]
	public string fileName;

	public Cell startCell;
	public Cell endCell;

	public Cell offsetCell;

	public string ToRange()
	{
		return string.Concat(sheetName, "!", startCell, ":", endCell);
	}

	[System.Serializable]
	public class Cell
	{
		[Tooltip("행")]
		public char column;
		[Tooltip("열 (0 입력시 전체)")]
		public int row;

		public override string ToString()
		{
			if (row <= 0)
				return column.ToString();

			return column + row.ToString();
		}
	}
}