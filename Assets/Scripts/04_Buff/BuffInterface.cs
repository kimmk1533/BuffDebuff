// 버프를 얻었을 때
public interface IOnBuffInitialize
{
	public void OnBuffInitialize(Character character);
}
// 버프를 잃었을 때
public interface IOnBuffFinalize
{
	public void OnBuffFinalize(Character character);
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
public interface IOnBuffCondition : IOnBuffInitialize, IOnBuffFinalize, IOnBuffUpdate, IOnBuffJump, IOnBuffDash, IOnBuffGetDamage, IOnBuffAttackStart, IOnBuffGiveDamage, IOnBuffAttackEnd
{

}