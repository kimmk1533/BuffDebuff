// 버프를 얻었을 때
public interface IOnBuffAdded
{
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new();
}
// 버프를 잃었을 때
public interface IOnBuffRemoved
{
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new();
}
// 매 프레임마다
public interface IOnBuffUpdate
{
	public void OnBuffUpdate<T>(Character<T> character) where T : CharacterStat, new();
}
// 일정 시간마다
public interface IOnBuffTimer
{
	public void OnBuffTimer<T>(Character<T> character) where T : CharacterStat, new();
}
// 점프했을 때
public interface IOnBuffJump
{
	public void OnBuffJump<T>(Character<T> character) where T : CharacterStat, new();
}
// 대쉬했을 때
public interface IOnBuffDash
{
	public void OnBuffDash<T>(Character<T> character) where T : CharacterStat, new();
}
// 타격시
public interface IOnBuffGiveDamage
{
	public void OnBuffGiveDamage<T>(Character<T> character) where T : CharacterStat, new();
}
// 피격 시
public interface IOnBuffTakeDamage
{
	public void OnBuffTakeDamage<T>(Character<T> character) where T : CharacterStat, new();
}
// 공격 시작할 때
public interface IOnBuffAttackStart
{
	public void OnBuffAttackStart<T>(Character<T> character) where T : CharacterStat, new();
}
// 공격 시
public interface IOnBuffAttack
{
	public void OnBuffAttack<T>(Character<T> character) where T : CharacterStat, new();
}
// 공격을 끝낼 때
public interface IOnBuffAttackEnd
{
	public void OnBuffAttackEnd<T>(Character<T> character) where T : CharacterStat, new();
}
// 적 처치 시
public interface IOnBuffKillEnemy
{
	public void OnBuffKillEnemy<T>(Character<T> character) where T : CharacterStat, new();
}
// 사망 시
public interface IOnBuffDeath
{
	public void OnBuffDeath<T>(Character<T> character) where T : CharacterStat, new();
}
// 스테이지를 넘어갈 시
public interface IOnBuffNextStage
{
	public void OnBuffNextStage<T>(Character<T> character) where T : CharacterStat, new();
}