using System.Collections;

public class WorldMonster : MonoBehaviour
{
    public WorldMonsterAI ai;

    public MonsterData monsterData;

    Coroutine _stateCoroutine;
    bool _isPause;
    bool _isCoroutineBreak;
    System.Action _coroutineCallback;

    public bool isDeath { get; private set; } = false;

    public void SetData(MonsterData monsterData, ushort monsterLv, int limitFollowRange)
    {
        this.monsterData = monsterData;

        SetAI();
    }

    public void SetAI()
    {
        if (monsterData.encounter_type == 0)
            ai = new WorldMonsterAI_FollowDistanceLimit();
        else
            ai = new WorldMonsterAI_FollowToEnd();

        ai.Init(this);
    }


    public void StartBehavior()
    {
        DisposeCoroutine();

        _isCoroutineBreak = false;
        _coroutineCallback = null;

        _stateCoroutine = StartCoroutine(BehaviorCoroutine());

        Default();
    }

    public void Default()
    {
        if (isDeath)
            return;

        _coroutineCallback = ai.DefaultBehavior;
    }

    public void DectectedPlayer()
    {
        if (isDeath)
            return;

        //유저 캐릭터 쫓아가기 코드
    }

    public void Death()
    {
        if (isDeath)
            return;

        isDeath = true;

        AnimationStateIdle();

        ai.InitDeath();

        _coroutineCallback = ai.Death;

        //몬스터 충돌 시 몬스터 사망 & PLAYER_HP_MINUS 이벤트 송출
    }

    private IEnumerator BehaviorCoroutine()
    {
        while (!_isCoroutineBreak)
        {
            if (!_isPause)
                _coroutineCallback?.Invoke();

            yield return null;
        }
    }

    //스파인 애니메이션 관련 코드
    //.........
}
