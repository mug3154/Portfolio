using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using static Config;
using static DeliveryController;

public class WorldController : MonoBehaviour
{
    public DeliveryController deliveryController { private set; get; }

    public WorldCamera WCamera { get; private set; }
    public WorldView View { get; private set; }
    public WorldUI UI { get; private set; }


    //WorldController의 Setting 호출 시 관리 중인 객체들도 같이 Setting() 호출하여 일괄처리.
    public void Setting(DeliveryController mainGame)
    {
        deliveryController = mainGame;

        View.Setting(this);
        UI.Setting(this);
        WCamera.Setting(this);
    }

    #region STATE
    public void State_Normal()
    {
        //경우에 따라 View, UI, WCamera 세팅
    }

    public void State_SelectQuest(DeliveryQuestInfo info)
    {
        //경우에 따라 View, UI, WCamera 세팅
    }

    public void State_PlayGame()
    {
        //경우에 따라 View, UI, WCamera 세팅

    }

    #endregion STATE

    #region PLAY

    WorldTile[] _routeTiles;
    ushort _moveCount;
    int _routeTileCount;

    bool _isPause;
    bool _isCoroutineBreak;

    Coroutine _deliveryCoroutine;

    List<DelveryEventReciever> _recievers = new List<DelveryEventReciever>();

    public void InitDelivery()
    {
        _isCoroutineBreak = false;

        //리시버 이벤트 등록
        _recievers.Clear();

        _recievers.Add(deliveryController.AddDeliveryEventReciever(new DeliveryController.DelveryEventReciever().
            SetEvent(DeliveryController.E_DELIVERY_EVENT.GAME_PAUSE).
            SetCallback(GamePause)));

        //..............

        ////////////////////////////////////////////

        //변수 및 캐릭터 상태 초기화
        
    }

    //본게임 시작 시 호출 됨.
    public void StartDelivery()
    {
        _deliveryCoroutine = StartCoroutine(PlayDelivery());
    }

    private IEnumerator PlayDelivery()
    {
        View.player.GameResume();
        View.player.PlayMoveAnimation();

        while (!_isCoroutineBreak)
        {
            while (_moveCount != _routeTileCount)
            {
                while (_isPause)
                {
                    if (_isCoroutineBreak)
                        break;

                    yield return null;
                }

                //캐릭터가 이동할 다음 타일에 대한 DeliveryController.E_DELIVERY_EVENT.CHANGE_PLAYER_DIRECTION 이벤트 송출

                yield return View.player.RunToTargetTile(_routeTiles[_moveCount], _routeTiles[_moveCount + 1]); //다음 타일로 이동 완료 할 때까지 대기

                _moveCount += 1;
            }

            //경로 타일 개수만큼 전부 이동 했을 경우 도착지 도착으로 판단. 이벤트 송출.
            deliveryController.StartDeliveryEvent(DeliveryController.E_DELIVERY_EVENT.ARRIVE_TO_CUSTOMER);

            break;
        }
    }


    public void GamePause()
    {
        _isPause = true;

        View.PauseAllMonsters();
    }

    public void GameResume()
    {
        _isPause = false;

        View.ResumeAllMonsters();
    }

    public void DeliveryDispose()
    {
        //각종 이벤트 멈춤 및 풀링 재배치
    }


    /// <summary>
    /// 오브젝트 효과 적용
    /// 몬스터 탐지 범위에 들어갔는지 확인
    /// </summary>
    /// <param name="args">0:WorldTile</param>
    private void OnPlayerEnterTile(WorldTile tile)
    {
        //오브젝트 효과 적용 
        WorldObject obj = View.GetObject(tile.Data.pos);
        if (obj != null)
        {
            obj.InTile();
            obj.PlayEffect();
        }

        //몬스터 탐지 범위에 걸리는지 확인
        View.CheckPlayerOnDetectingTiles(tile.Data.pos);
    }

    private void OnPlayerCenterInTile(WorldTile tile)
    {
        //타일 가운데에 도착 시 이벤트 실행
    }

    /// <summary>
    /// 오브젝트 효과 제거
    /// </summary>
    /// <param name="args">0:WorldTile</param>
    private void OnPlayerOutTile(WorldTile tile)
    {
        WorldObject obj = View.GetObject(tile.Data.pos);
        if (obj != null)
        {
            obj.OutTile();
        }
    }
    #endregion PLAY
}
