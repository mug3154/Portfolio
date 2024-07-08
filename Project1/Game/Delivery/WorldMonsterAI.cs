using UnityEngine;

public abstract class WorldMonsterAI
{
    protected WorldMonster character;

    protected enum E_DEFAULT_BEHAVIOR
    {
        NONE,
        IDLE,
        MOVE
    }
    protected E_DEFAULT_BEHAVIOR _STATE;
    protected float behaviorCooltime;

    public virtual void Init(WorldMonster character)
    {
        this.character = character;
    }


    public abstract void SetRandomDefaultBehavior();
    public abstract void DefaultBehavior();

    public abstract void SetFollowPlayer();

    public abstract void FollowPlayer();

    public void InitDeath()
    {
        behaviorCooltime = 0;

        //사망시 연출 변수 계산
    }

    public abstract void Death();

}
