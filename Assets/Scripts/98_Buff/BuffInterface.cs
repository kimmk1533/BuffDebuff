namespace BuffDebuff
{
	// 버프를 얻었을 때
	public interface IOnBuffAdded
	{
		public void OnBuffAdded<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 버프를 잃었을 때
	public interface IOnBuffRemoved
	{
		public void OnBuffRemoved<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 매 프레임마다
	public interface IOnBuffUpdate
	{
		public void OnBuffUpdate<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 일정 시간마다
	public interface IOnBuffTimer
	{
		public void OnBuffTimer<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 점프했을 때
	public interface IOnBuffJump
	{
		public void OnBuffJump<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 대쉬했을 때
	public interface IOnBuffDash
	{
		public void OnBuffDash<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 타격시
	public interface IOnBuffGiveDamage
	{
		public void OnBuffGiveDamage<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 피격 시
	public interface IOnBuffTakeDamage
	{
		public void OnBuffTakeDamage<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 공격 시작할 때
	public interface IOnBuffAttackStart
	{
		public void OnBuffAttackStart<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 공격 시
	public interface IOnBuffAttack
	{
		public void OnBuffAttack<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 공격을 끝낼 때
	public interface IOnBuffAttackEnd
	{
		public void OnBuffAttackEnd<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 적 처치 시
	public interface IOnBuffKillEnemy
	{
		public void OnBuffKillEnemy<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 사망 시
	public interface IOnBuffDeath
	{
		public void OnBuffDeath<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 스테이지를 넘어갈 시
	public interface IOnBuffNextStage
	{
		public void OnBuffNextStage<TSelf, TStat, TController, TAnimator>(Character<TSelf, TStat, TController, TAnimator> character) where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
}