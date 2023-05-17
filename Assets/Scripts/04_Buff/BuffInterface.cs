// 버프를 얻었을 때 (첫 1번만)
public interface IOnBuffInitialize
{
	public void OnBuffInitialize(Character character);
}
// 버프를 잃었을 때 (갯수가 0이 될 때)
public interface IOnBuffFinalize
{
	public void OnBuffFinalize(Character character);
}
// 버프를 얻었을 때 (매번 추가될 때 마다)
public interface IOnBuffAdded
{
	public void OnBuffAdded(Character character);
}
// 버프를 잃었을 때 (매번 감소될 때 마다)
public interface IOnBuffRemoved
{
	public void OnBuffRemoved(Character character);
}
// 매 프레임마다
public interface IOnBuffUpdate
{
	public void OnBuffUpdate();
}
// 점프했을 때
public interface IOnBuffJump
{
	public void OnBuffJump();
}
// 대쉬했을 때
public interface IOnBuffDash
{
	public void OnBuffDash();
}
// 대미지를 받을 때
public interface IOnBuffGetDamage
{
	public void OnBuffGetDamage();
}
// 공격 시작할 때
public interface IOnBuffAttackStart
{
	public void OnBuffAttackStart();
}
// 대미지를 줄 때
public interface IOnBuffGiveDamage
{
	public void OnBuffGiveDamage();
}
// 공격을 끝낼 때
public interface IOnBuffAttackEnd
{
	public void OnBuffAttackEnd();
}
// 모든 버프 조건
public interface IOnBuffCondition : IOnBuffInitialize, IOnBuffFinalize, IOnBuffAdded, IOnBuffRemoved, IOnBuffUpdate, IOnBuffJump, IOnBuffDash, IOnBuffGetDamage, IOnBuffAttackStart, IOnBuffGiveDamage, IOnBuffAttackEnd
{

}