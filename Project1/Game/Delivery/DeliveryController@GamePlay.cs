using System;
using System.Collections;
using System.Collections.Generic;
using Game.Model;
using Google.Protobuf;
using UniRx;
using static Config;

/********************************************************
 * = (비동기 이벤트)
 * 상태 변화 또는 이벤트 실행은 E_DELIVERY_EVENT을 통해서 진행한다.
 * AddDeliveryEventReciever로 이벤트를 등록하여 해당 이벤트를 등록한 클래스에서
 * 이벤트에 관련 된 처리를 하도록 함.
 * 
 * = (동기 이벤트)
 * 간혹 가다가 이벤트 실행 시에 처리해야하는 작업이 여러개인 경우에는
 * (예: 타일 중앙에 도착 시 돌발상황 이벤트와 보물상자 이벤트가 동시에 뜨는 경우)
 * AddDeliveryEventCallback에 함수를 등록하여 실행해야한다.
 * 
 ********************************************************/

public partial class DeliveryController : ContentController
{
    public enum E_DELIVERY_EVENT
    {
        NONE, //달리기 진행

        GAME_PAUSE,
        GAME_RESUME,
        GAME_FAIL,
        GAME_SUCCESS,

        DELIVERY_TIME_OUT,

        PLAYER_ENTER_TILE,
        PLAYER_CENTER_IN_TILE,
        PLAYER_OUT_TILE,

        ARRIVE_TO_CUSTOMER,

        //플레이어 상태용 
        CHANGE_PLAYER_SPEED,
        CHANGE_PLAYER_DIRECTION,

        //피 증감 차감 이벤트
        PLAYER_HP_PLUS,
        PLAYER_HP_MINUS,

        //배틀
        BATTLE_MONSTER_ALL_KILL,
        PLAYER_HP_ZERO,
    }

    #region DELIVERY RECIEVER

    public record DelveryEventReciever
    {
        public E_DELIVERY_EVENT eventType { private set; get; }
        public Action onCallback { private set; get; }
        public Action<double> onCallbackDouble { private set; get; }
        //public Action<long> onCallbackLong { private set; get; }
        public Action<Vector2Int> onCallbackVector2Int { private set; get; }
        public Action<WorldTile> onCallbackTile { private set; get; }

        public DelveryEventReciever SetEvent(E_DELIVERY_EVENT eventType)
        {
            this.eventType = eventType;
            return this;
        }

        public DelveryEventReciever SetCallback(Action onCallback)
        {
            this.onCallback = onCallback;
            return this;
        }

        public DelveryEventReciever SetCallback(Action<double> onCallback)
        {
            this.onCallbackDouble = onCallback;
            return this;
        }

        public DelveryEventReciever SetCallback(Action<WorldTile> onCallback)
        {
            this.onCallbackTile = onCallback;
            return this;
        }

        public DelveryEventReciever SetCallback(Action<Vector2Int> onCallback)
        {
            this.onCallbackVector2Int = onCallback;
            return this;
        }
    }

    private List<DelveryEventReciever> _deliveryEventRecievers = new List<DelveryEventReciever>();

    public DelveryEventReciever AddDeliveryEventReciever(DelveryEventReciever reciever)
    {
        _deliveryEventRecievers.Add(reciever);

        return reciever;
    }

    public void RemoveDeliveryEventReciever(DelveryEventReciever reciever)
    {
        _deliveryEventRecievers.Remove(reciever);
    }

    public void ClearDeliveryEventRecivers()
    {
        _deliveryEventRecievers.Clear();

        GC.Collect();
    }

    public void StartDeliveryEvent(E_DELIVERY_EVENT eventType)
    {
        if (_deliveryEventRecievers.Count == 0)
            return;

        for (int i = _deliveryEventRecievers.Count - 1; i > -1; --i)
        {
            var r = _deliveryEventRecievers[i];
            if (r.eventType == eventType)
            {
                r.onCallback?.Invoke();
            }
        }
    }

    public void StartDeliveryEvent(E_DELIVERY_EVENT eventType, double value)
    {
        if (_deliveryEventRecievers.Count == 0)
            return;

        for (int i = _deliveryEventRecievers.Count - 1; i > -1; --i)
        {
            try
            {
                var r = _deliveryEventRecievers[i];
                if (r.eventType == eventType)
                {
                    r.onCallbackDouble?.Invoke(value);
                }
            }catch(Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public void StartDeliveryEvent(E_DELIVERY_EVENT eventType, Vector2Int value)
    {
        if (_deliveryEventRecievers.Count == 0)
            return;

        for (int i = _deliveryEventRecievers.Count - 1; i > -1; --i)
        {
            var r = _deliveryEventRecievers[i];
            if (r.eventType == eventType)
            {
                r.onCallbackVector2Int?.Invoke(value);
            }
        }
    }

    public void StartDeliveryEvent(E_DELIVERY_EVENT eventType, WorldTile value)
    {
        if (_deliveryEventRecievers.Count == 0)
            return;

        for (int i = _deliveryEventRecievers.Count - 1; i > -1; --i)
        {
            var r = _deliveryEventRecievers[i];
            if (r.eventType == eventType)
            {
                r.onCallbackTile?.Invoke(value);
            }
        }
    }

    #endregion DELIVERY RECIEVER

    private bool _isExcutingSyncDeliveryEvent;
    public Queue<Action> _syncDeliveryEvents = new Queue<Action>();

    public long Score = 0;
    public long minusScorePerSec = 0;
    public ReactiveProperty<long> minusScore = new ReactiveProperty<long>(0); //배달 UI에서 점수 표시 시 사용하기 위해 public ReactiveProperty로 제작.

    public void InitDelivery()
    {
        _isExcutingSyncDeliveryEvent = false;
        _syncDeliveryEvents.Clear();

        ClearDeliveryEventRecivers();

        AddDeliveryEventReciever(new DeliveryController.DelveryEventReciever().
          SetEvent(DeliveryController.E_DELIVERY_EVENT.GAME_PAUSE).
          SetCallback(GamePause));
        AddDeliveryEventReciever(new DeliveryController.DelveryEventReciever().
         SetEvent(DeliveryController.E_DELIVERY_EVENT.GAME_RESUME).
         SetCallback(GameResume));
        AddDeliveryEventReciever(new DeliveryController.DelveryEventReciever().
          SetEvent(DeliveryController.E_DELIVERY_EVENT.GAME_FAIL).
          SetCallback(DeilveryFail));
        AddDeliveryEventReciever(new DeliveryController.DelveryEventReciever().
         SetEvent(DeliveryController.E_DELIVERY_EVENT.GAME_SUCCESS).
         SetCallback(DeilverySuccess));
        AddDeliveryEventReciever(new DeliveryController.DelveryEventReciever().
         SetEvent(DeliveryController.E_DELIVERY_EVENT.ARRIVE_TO_CUSTOMER).
         SetCallback(ArriveToCustomer));

        World.InitDelivery();

        AddTimerCallback(E_TIMER.DELIVERY, OnTimer);

        StartCoroutine(StartDelivery());
    }

    private IEnumerator StartDelivery()
    {
        GetTimer(E_TIMER.DELIVERY).ResetTimer(Score / Mathf.Abs(minusScorePerSec));
        StartTimer(E_TIMER.DELIVERY);

        StartDeliveryEvent(E_DELIVERY_EVENT.GAME_RESUME);

        //배달 시작
        World.StartDelivery();
    }

    
    //반드시 순차적으로 실행해야하는 이벤트 같은 경우에 사용한다.
    public void AddSyncDeliveryEvent(Action action)
    {
        _syncDeliveryEvents.Enqueue(action);

        if (_isExcutingSyncDeliveryEvent == false)
        {
            StartDeliveryEvent(E_DELIVERY_EVENT.GAME_PAUSE);
            _syncDeliveryEvents.Peek().Invoke();
        }
    }

    public void RemoveSyncDeliveryEvent(Action action)
    {
        _syncDeliveryEvents.Dequeue();

        if (_syncDeliveryEvents.Count == 0)
        {
            _isExcutingSyncDeliveryEvent = false;
            StartDeliveryEvent(E_DELIVERY_EVENT.GAME_RESUME);
        }
        else
        {
            _syncDeliveryEvents.Peek().Invoke();
        }
    }


    private void GamePause()
    {
        PauseTimer(DeliveryController.E_TIMER.DELIVERY);
    }

    private void GameResume()
    {
        ResumeTimer(DeliveryController.E_TIMER.DELIVERY);
    }



    private void ArriveToCustomer()
    {
        _world.DeliveryDispose();

        //경로 그리기 컨텐츠 일시정지
        StartDeliveryEvent(E_DELIVERY_EVENT.GAME_PAUSE);

        //전투 컨텐츠 로딩 후 전투 진입
        ShowController(E_CONTROLLER.BATTLE);

        _battle.EnterGame((data) =>
        {
            //전투 종료 후 결과에 따라 이벤트 송출
            if (!data.IsClear)
            {
                StartDeliveryEvent(E_DELIVERY_EVENT.GAME_FAIL);
            }
            else
            {
                StartDeliveryEvent(E_DELIVERY_EVENT.GAME_SUCCESS);
            }
        });
    }

    private void OnTimer(double sec)
    {
        //남은 시간에 따라 점수 계산
        minusScore.Value = /*점수 계산 공식 처리*/;

        //별 1개보다 점수가 작아지면 실패 처리
        if (currentQuest.starCondition[0] >= Score + minusScore.Value)
        {
            StartDeliveryEvent(E_DELIVERY_EVENT.DELIVERY_TIME_OUT);
        }
    }

    private void DeilveryFail()
    {
        _world.DeliveryDispose();

        StopTimer(E_TIMER.DELIVERY);
        RemoveTimerCallback(E_TIMER.DELIVERY, OnTimer);

        //실패 팝업 노출 후 닫을 때 통신
        RootScene.uiSystem.OpenUI<PopupFail>((popup) =>
        {
            popup.Show(Score + minusScore.Value <= 0, _retryCount, () =>
            {
                //팝업에서 재시도 버튼 누른 경우 경로 그리기 컨텐츠 초기화
                ShowController(E_CONTROLLER.WORLD);
                DELIVERY_STATE = E_DELIVERY_STATE.START_DRAWING;
                --_retryCount;
            });
        }
       , SendToServer_Fail);
    }

    private void SendToServer_Fail()
    {
        //서버로 실패 전송
    }

    private void DeilverySuccess()
    {
        _world.DeliveryDispose();

       //성공 시 필요한 값 계산 후 서버로 성공 전송
    }

    //게임 결과 서버 통신 완료 후 콜백 리스트에 호출할 순서대로 함수 추가 후 함수 기능 처리 완료 시 순차 처리.
    //DeliveryClearData 게임 결과 데이터가 담긴 struct
    public void OnReceive_GameResult(bool isSuccess, DeliveryClearData clearData)
    {
        if (isSuccess == false)
        {
            DELIVERY_STATE = E_DELIVERY_STATE.RETURN_TO_NORMAL;
            return;
        }


        if (gameEndPopupCallbacks == null)
            gameEndPopupCallbacks = new List<Action>();
        else
            gameEndPopupCallbacks.Clear();


        //결과 팝업
        gameEndPopupCallbacks.Add(() =>
        {
            RootScene.uiSystem.OpenUI<PopupResult>((popup) =>
            {
                popup.SetData(clearData);
            }
            , CallGameEndPopupCallbacks);
        });

        //획득 아이템 팝업
        if (clearData.rewards.Count != 0)
        {
            gameEndPopupCallbacks.Add(() =>
            {
                RootScene.uiSystem.OpenUI<PopupCommonReward>((popup) =>
                {
                    //TO-DO
                }
                , CallGameEndPopupCallbacks);
            });            
        }

        //장비 레벨업 팝업
        if (RootScene.userInfo.userVehicleInfo.BeforeGasLv != RootScene.userInfo.userVehicleInfo.GasLv)
        {
            gameEndPopupCallbacks.Add(() =>
            {
                RootScene.uiSystem.OpenUI<PopupRidingLevelUp>((popup) =>
                {
                    //TO-DO
                }
                , CallGameEndPopupCallbacks);
            });
        }

        //유저 레벨업 팝업
        if (RootScene.userInfo.userStatInfo.BeforeLv != RootScene.userInfo.userStatInfo.Lv)
        {
            gameEndPopupCallbacks.Add(() =>
            {
                RootScene.uiSystem.OpenUI<PopupPlayerLevelup>((popup) =>
                {
                    popup.SetData();
                }
                , CallGameEndPopupCallbacks);
            });
        }

        CallGameEndPopupCallbacks();
    }


    private List<Action> gameEndPopupCallbacks;
    private void CallGameEndPopupCallbacks()
    {
        if(gameEndPopupCallbacks.Count == 0)
        {
            DELIVERY_STATE = E_DELIVERY_STATE.RETURN_TO_NORMAL;
        }
        else
        {
            gameEndPopupCallbacks[0].Invoke();
            gameEndPopupCallbacks.RemoveAt(0);
        }
    }
}