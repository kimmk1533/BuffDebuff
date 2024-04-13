using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public interface IMovingStrategy
	{
		public Vector2 CalculateVelocity(Projectile projectile);
	}

	public abstract class ProjectileMove : IMovingStrategy
	{
		#region 변수
		#endregion

		#region 프로퍼티
		#endregion

		#region 이벤트
		#endregion

		#region 매니저
		#endregion

		//public abstract void Initialize();
		//public abstract void Finallize();

		public abstract Vector2 CalculateVelocity(Projectile projectile);
	}
}