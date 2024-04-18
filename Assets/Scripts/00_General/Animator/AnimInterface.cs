using UnityEngine;

namespace BuffDebuff
{
	public interface IAnim_Movable
	{
		public void Anim_SetVelocity(float x, float y);
		public void Anim_SetVelocity(Vector2 speed);
	}
	public interface IAnim_IsGround
	{
		public void Anim_SetIsGround(bool isGround);
	}
	public interface IAnim_Attack
	{
		public void Anim_SetAttackSpeed(float attackSpeed);
		public void Anim_Attack(int patternIndex);
	}
	public interface IAnim_Jump
	{
		public void Anim_Jump();
	}
	public interface IAnim_Death
	{
		public void Anim_Death();
	}
}