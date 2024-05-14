namespace BuffDebuff
{
	// 버프를 얻었을 때
	public interface IOnBuffAdded
	{
		public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 버프를 잃었을 때
	public interface IOnBuffRemoved
	{
		public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 매 프레임마다
	public interface IOnBuffUpdate
	{
		public void OnBuffUpdate<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 일정 시간마다
	public interface IOnBuffTimer
	{
		public void OnBuffTimer<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 점프했을 때
	public interface IOnBuffJump
	{
		public void OnBuffJump<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 대쉬했을 때
	public interface IOnBuffDash
	{
		public void OnBuffDash<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 타격시
	public interface IOnBuffGiveDamage
	{
		public void OnBuffGiveDamage<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 피격 시
	public interface IOnBuffTakeDamage
	{
		public void OnBuffTakeDamage<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 공격 시작할 때
	public interface IOnBuffAttackStart
	{
		public void OnBuffAttackStart<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 공격 시
	public interface IOnBuffAttack
	{
		public void OnBuffAttack<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 공격을 끝낼 때
	public interface IOnBuffAttackEnd
	{
		public void OnBuffAttackEnd<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 적 처치 시
	public interface IOnBuffKillEnemy
	{
		public void OnBuffKillEnemy<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 사망 시
	public interface IOnBuffDeath
	{
		public void OnBuffDeath<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
	// 스테이지를 넘어갈 시
	public interface IOnBuffNextStage
	{
		public void OnBuffNextStage<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack;
	}
}