using System.Collections;
using System.Collections.Generic;
using SpreadSheet;
using UnityEngine;

namespace BuffDebuff
{
	public class DataSheetManager : SpreadSheetManager<DataSheetManager>
	{
		private void Awake()
		{
			Initialize();
		}
	}
}