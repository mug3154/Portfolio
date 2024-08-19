using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.EventSystems;


public class GameUI_Player : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] Button _ChangeButton;
    [SerializeField] Text _RestCountText;
    [SerializeField] Image[] _Bubbles;

    [SerializeField] GameObject _Nero;
    [SerializeField] GameObject _NeroOn;
    [SerializeField] RectTransform _NeroGuageMask;


    IDisposable _ShootBubbleRemoveDisposal;
    IDisposable _ShootBubbleAddDisposal;
    IDisposable _ShootBubbleChangeDisposal;

    IDisposable _RestBubbleDisposal;

    bool _IsControllPossible = true;

    Action<Vector2> _MoveShootingLineCallback;
    Action _CancelShootingLineCallback;

    float _NeroGuageMaskH;

    private void Start()
    {
        _ChangeButton.onClick.AddListener(OnClickChangeButton);
        _NeroGuageMaskH = _NeroGuageMask.sizeDelta.y;

    }

    private void OnClickChangeButton()
    {
        if (_IsControllPossible == false)
            return;
        
        GameFlowEventManager.Instance.Notify(GameFlowEventManager.GAME_FLOW_EVENT.CHANGE_BUBBLE_ORDER);
    }

    public virtual void Initialize(Action<Vector2> moveShootingLineCallback, Action cancelShootingLineCallback)
    {
        _MoveShootingLineCallback = moveShootingLineCallback;
        _CancelShootingLineCallback = cancelShootingLineCallback;

        _ShootBubbleRemoveDisposal?.Dispose();
        _ShootBubbleRemoveDisposal = Game.Mode.ShootReadyBubbles.ObserveRemove().Subscribe(OnRemoveShootBubbleCount);
        
        _ShootBubbleAddDisposal?.Dispose();
        _ShootBubbleAddDisposal = Game.Mode.ShootReadyBubbles.ObserveAdd().Subscribe(OnAddShootBubbleCount);

        _ShootBubbleChangeDisposal?.Dispose();
        _ShootBubbleChangeDisposal = Game.Mode.ShootReadyBubbles.ObserveReplace().Subscribe(OnChangeShootBubbleIdx);

        _RestBubbleDisposal?.Dispose();
        _RestBubbleDisposal = Game.Mode.RestBubbleCount.Subscribe(OnChangedRestBubbleCount);

        GameFlowEventManager.Instance.AddReceiver(new GameFlowEventReceiver().SetEvent(GameFlowEventManager.GAME_FLOW_EVENT.CONTROLL_POSSIBLE_PLAYER).SetCallback(SetControllPossibleState));

        if(Game.Mode.IsNeroMode)
        {
            _Nero.gameObject.SetActive(true);
            Game.Mode.OnChangeNeroNeedBubbleCount += OnChangeNeroGauge;
            GameFlowEventManager.Instance.AddReceiver(new GameFlowEventReceiver().SetEvent(GameFlowEventManager.GAME_FLOW_EVENT.NERO_ON).SetCallback(NeroOn));
            GameFlowEventManager.Instance.AddReceiver(new GameFlowEventReceiver().SetEvent(GameFlowEventManager.GAME_FLOW_EVENT.NERO_OFF).SetCallback(NeroOff));

            NeroOff();
            OnChangeNeroGauge(0, 1);
        }
        else
        {
            _Nero.gameObject.SetActive(false);
        }

        _Bubbles[2].gameObject.SetActive(false);

        SetShootBubbleIdx(0, Game.Mode.ShootReadyBubbles[0]);
        SetShootBubbleIdx(1, Game.Mode.ShootReadyBubbles[1]);

        _IsControllPossible = false;
    }

    private void OnRemoveShootBubbleCount(CollectionRemoveEvent<int> @event)
    {
        _Bubbles[2].gameObject.SetActive(false);

        SetShootBubbleIdx(0, Game.Mode.ShootReadyBubbles[0]);
        SetShootBubbleIdx(1, Game.Mode.ShootReadyBubbles[1]);
    }

    private void OnAddShootBubbleCount(CollectionAddEvent<int> @event)
    {
        _Bubbles[2].gameObject.SetActive(true);

        SetShootBubbleIdx(0, Game.Mode.ShootReadyBubbles[0]);
        SetShootBubbleIdx(1, Game.Mode.ShootReadyBubbles[1]);
        SetShootBubbleIdx(2, Game.Mode.ShootReadyBubbles[2]);
    }

    private void NeroOn()
    {
        _NeroOn.SetActive(true);
    }

    private void NeroOff()
    {
        _NeroOn.SetActive(false);
    }

    private void OnChangeNeroGauge(int curr, int max)
    {
        _NeroGuageMask.sizeDelta = new Vector2(_NeroGuageMask.sizeDelta.x, _NeroGuageMaskH * (curr / (float)max));
    }

    private void OnChangeShootBubbleIdx(CollectionReplaceEvent<int> @event)
    {
        SetShootBubbleIdx(@event.Index, @event.NewValue);
    }

    private void SetShootBubbleIdx(int idx, int color)
    {
        _Bubbles[idx].overrideSprite = Root.Instance.ResManager.GetBubbleSprite(color);
    }



    private void SetControllPossibleState()
    {
        _IsControllPossible = true;
    }

    private void OnChangedRestBubbleCount(int restCount)
    {
        _RestCountText.text = restCount.ToString();
    }

    public void Dispose()
    {
        if (_RestBubbleDisposal != null)
        {
            _RestBubbleDisposal.Dispose();
            _RestBubbleDisposal = null;
        }

        if (_ShootBubbleRemoveDisposal != null)
        {
            _ShootBubbleRemoveDisposal.Dispose();
            _ShootBubbleRemoveDisposal = null;
        }

        if (_ShootBubbleAddDisposal != null)
        {
            _ShootBubbleAddDisposal.Dispose();
            _ShootBubbleAddDisposal = null;
        }

        if (_ShootBubbleChangeDisposal != null)
        {
            _ShootBubbleChangeDisposal.Dispose();
            _ShootBubbleChangeDisposal = null;
        }

        _CancelShootingLineCallback = null;
        _MoveShootingLineCallback = null;
    }



    public void OnPointerExit(PointerEventData eventData)
    {
        if (_IsControllPossible == false) return;

        _IsPointerDown = false;

        _CancelShootingLineCallback?.Invoke();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (eventData.pointerEnter == _ChangeButton.gameObject) return;

        if (_IsPointerDown == false) return;

        if (_IsControllPossible == false) return;

        _MoveShootingLineCallback?.Invoke(Camera.main.ScreenToWorldPoint(eventData.position));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_IsControllPossible == false) return;

        _IsPointerDown = false;

        _IsControllPossible = false;

        GameFlowEventManager.Instance.Notify(GameFlowEventManager.GAME_FLOW_EVENT.REQUEST_TO_SHOOT);
    }

    bool _IsPointerDown = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_IsControllPossible == false) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);

        //Debug.Log(worldPos);
        //if (worldPos.y < -3f)
        //{
        //    return;
        //}

        _IsPointerDown = true;

        _MoveShootingLineCallback?.Invoke(worldPos);
    }

}
