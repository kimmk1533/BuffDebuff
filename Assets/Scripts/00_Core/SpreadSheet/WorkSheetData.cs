using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorkSheetData
{
	public bool enabled = true;

	public string sheetName;

	public Cell startCell;
	public Cell endCell;

	public string ToRange()
	{
		return string.Concat(sheetName, "!", startCell, ":", endCell);
	}

	[System.Serializable]
	public class Cell
	{
		public string column;
		public int row;

		public override string ToString()
		{
			if (row <= 0)
				return column;

			return column + row.ToString();
		}
	}
}