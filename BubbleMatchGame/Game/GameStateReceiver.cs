using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowEventReceiver
{   
    public GameFlowEventManager.GAME_FLOW_EVENT Flow { private set; get; }
    public Action Callback { private set; get; }
    public Action<Vector2> Vector2Callback { private set; get; }

    public GameFlowEventReceiver SetEvent(GameFlowEventManager.GAME_FLOW_EVENT type)
    {
        Flow = type;

        return this;
    }

    public GameFlowEventReceiver SetCallback(Action callback)
    {
        Callback = callback;

        return this;
    }

    public GameFlowEventReceiver SetCallback(Action<Vector2> callback)
    {
        Vector2Callback = callback;

        return this;
    }
}

public class GameFlowEventManager
{
    public enum GAME_FLOW_EVENT
    {
        NONE = 0,

        GAME_OVER,
        GAME_CLEAR,

        CHECK_CLEAR_CONDITION,

        CONTROLL_POSSIBLE_PLAYER,
        //MOVE_BUBBLE_LINE,
        //CANCEL_BUBBLE_LINE,

        CHANGE_BUBBLE_ORDER,

        REQUEST_TO_SHOOT,
        SHOOTING_SUCCESS,

        BUBBLE_POP,
        NERO_ON,
        NERO_OFF,

        MAX
    }

    static GameFlowEventManager _Instance;
    public static GameFlowEventManager Instance => _Instance ?? (_Instance = new GameFlowEventManager());

    HashSet<GameFlowEventReceiver> _Receivers;

    public void Initialize()
    {
        if(_Receivers == null)
        {
            _Receivers = new HashSet<GameFlowEventReceiver>();
        }
        else
        {
            _Receivers.Clear();
        }
    }

    public void AddReceiver(GameFlowEventReceiver receiver)
    {
        _Receivers.Add(receiver);
    }

    public void Notify(GAME_FLOW_EVENT flow)
    {
        Debug.Log("Notify " +  flow);

        foreach (var r in _Receivers)
        {
            if (r.Flow == flow)
                r.Callback?.Invoke();
        }
    }

    public void Notify(GAME_FLOW_EVENT flow, Vector2 vector2)
    {
        foreach (var r in _Receivers)
        {
            if (r.Flow == flow)
                r.Vector2Callback?.Invoke(vector2);
        }
    }

    public void Dispose()
    {
        _Receivers.Clear();
    }
}