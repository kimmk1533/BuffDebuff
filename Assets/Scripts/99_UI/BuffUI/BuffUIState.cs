using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	public partial class BuffUI : ObjectPoolItem<BuffUI>
	{
		#region Enum
		public enum E_BuffUIState
		{
			None = -1,

			BuffRewards,
			BuffInventory,
			BuffCombine,
			BuffCombineInventory,
		}
		#endregion

		#region 인터페이스
		private interface IBuffUIState
		{
			public abstract void OnClicked(BuffUI buffUI);
		}
		#endregion

		#region 내부 클래스
		private class RewardState : IBuffUIState
		{
			private static BuffUIManager M_BuffUI => BuffUIManager.Instance;

			public void OnClicked(BuffUI buffUI)
			{
				M_BuffUI.AddBuff(buffUI.buffData);
				M_BuffUI.rewardsPanel.active = false;
			}
		}
		private class InventoryState : IBuffUIState
		{
			private static BuffUIManager M_BuffUI => BuffUIManager.Instance;

			public void OnClicked(BuffUI buffUI)
			{
				Debug.Log(buffUI.buffData.title + ": " + buffUI.buffCount.ToString());
			}
		}
		private class CombineState : IBuffUIState
		{
			private static BuffUIManager M_BuffUI => BuffUIManager.Instance;

			public void OnClicked(BuffUI buffUI)
			{
				BuffData buffData = buffUI.buffData;

				if (M_BuffUI.RemoveCombineBuff(buffData) == false)
					return;

				M_BuffUI.AddBuff(buffData);
			}
		}
		private class CombineInventoryState : IBuffUIState
		{
			private static BuffUIManager M_BuffUI => BuffUIManager.Instance;

			public void OnClicked(BuffUI buffUI)
			{
				BuffData buffData = buffUI.buffData;

				if (M_BuffUI.AddCombineBuff(buffData) == false)
					return;

				M_BuffUI.RemoveBuff(buffData);
			}
		}
		#endregion
	}
}