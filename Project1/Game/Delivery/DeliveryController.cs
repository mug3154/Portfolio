
public partial class DeliveryController : ContentController
{
    [SerializeField] WorldController _world;
    [HideInInspector] public WorldController World { get => _world; }

    [SerializeField] BattleController _battle;
    [HideInInspector] public BattleController Battle { get => _battle; }

    public DeliveryQuestInfo currentQuest;

    public enum E_CONTROLLER
    {
        WORLD = 0,
        BATTLE,
        MAX
    }


    public enum E_DELIVERY_STATE
    {
        NORMAL,
        SELECT_QUEST_FROM_WORLD,
        ............,
        PLAYING_GAME,
        RETURN_TO_NORMAL,
    }
    public E_DELIVERY_STATE _DELIVERY_STATE;
    public E_DELIVERY_STATE DELIVERY_STATE
    {
        set
        {
            _DELIVERY_STATE = value;
            SetMainGameState(value);
        }

        get
        {
            return _DELIVERY_STATE;
        }
    }

    public void Init()
    {
        CreateTimerSystem();

        ShowController(E_CONTROLLER.WORLD);

        _world.Setting(this);
        _battle.Setting(this);
    }


    public override void EnterContent()
    {
        base.EnterContent();

        _world.UI.Show();

        DELIVERY_STATE = E_DELIVERY_STATE.NORMAL;
    }

    public override void ExitContent()
    {
        base.ExitContent();

        _world.ExitContent();
    }


    public void ShowController(E_CONTROLLER controller)
    {
        _battle.View.ClearGame();

        if (controller == E_CONTROLLER.WORLD)
        {
            _world.gameObject.SetActive(true);
            _battle.gameObject.SetActive(false);
        }
        else
        {
            _world.gameObject.SetActive(false);
            _battle.gameObject.SetActive(true);
        }
    }

    public bool IsShowingController(E_CONTROLLER controller)
    {
        if (controller == E_CONTROLLER.WORLD)
            return _world.gameObject.activeSelf;
        else if (controller == E_CONTROLLER.BATTLE)
            return _battle.gameObject.activeSelf;

        return false;
    }


    private void SetMainGameState(E_DELIVERY_STATE state)
    {
        switch (state)
        {
            case E_DELIVERY_STATE.NORMAL:
                State_Normal();
                break;
            case E_DELIVERY_STATE.SELECT_QUEST_FROM_WORLD:
                State_SelectMainQuest();
                break;
            //.................
            case E_DELIVERY_STATE.PLAYING_GAME:
                State_PlayGame();
                break;
            case E_DELIVERY_STATE.RETURN_TO_NORMAL:
                State_ReturnToNormal();
                break;
        }
    }

    private void State_Normal()
    {
        //퀘스트 데이터 세팅
        currentQuest ??= new DeliveryQuestInfo();
        RootScene.userInfo.GetMainQuest().CopyTo(currentQuest);

        ClearDeliveryEventRecivers();

        ShowController(E_CONTROLLER.WORLD);

        _world.DeliveryDispose();
        _world.State_Normal();
    }

    private void State_SelectMainQuest()
    {
        _world.State_SelectQuest(currentQuest);
    }

    private void State_PlayGame()
    {
        RootScene.uiSystem.PreLoadUI<PopupResult>(); //팝업 로딩 딜레이를 줄이기 위하여 선로드
        RootScene.uiSystem.PreLoadUI<PopupCommonReward>();

        InitDelivery();

        _world.State_PlayGame();
    }

    private void State_ReturnToNormal()
    {
        DELIVERY_STATE = E_DELIVERY_STATE.NORMAL;
    }


    public bool KeyDownEscape()
    {
        if (IsShowingController(E_CONTROLLER.WORLD))
        {
            if (_DELIVERY_STATE == E_DELIVERY_STATE.SELECT_QUEST_FROM_WORLD)
            {
                DELIVERY_STATE = E_DELIVERY_STATE.RETURN_TO_NORMAL;
                return true;
            }
            else if (_DELIVERY_STATE == E_DELIVERY_STATE.PLAYING_GAME)
            {
                _world.UI.Play.OnClickExitBtn();
                return true;
            }
        }
        else if(IsShowingController(E_CONTROLLER.BATTLE))
        {
            _battle.UI.HUDBottom.ShowExitPopup();
            return true;
        }

        return false;
    }
}
