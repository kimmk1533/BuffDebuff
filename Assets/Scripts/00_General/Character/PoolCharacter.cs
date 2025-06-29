using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	public abstract class PoolCharacter<T, TSelf, TStat, TController, TAnimator> : ObjectPoolItem<TSelf> where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack
	{
		#region 변수
		#endregion

		#region 프로퍼티
		#endregion



	}
}