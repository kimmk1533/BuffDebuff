using System.Collections;
using System.Collections.Generic;
using SpreadSheet;
using UnityEngine;

namespace BuffDebuff
{
	public class BuffSheetManager : SpreadSheetManager<BuffSheetManager>
	{
		[ContextMenu("Load Sheet Data")]
		public override void LoadSpreadSheetData()
		{
			base.LoadSpreadSheetData();
		}
	}
}