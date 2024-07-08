using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public partial class DeliveryController : ContentController
{
    public enum E_TIMER
    {
        DELIVERY
    }

    private GameTimer[] _timers;

    private void CreateTimerSystem()
    {
        int timerTypeCount = System.Enum.GetValues(typeof(E_TIMER)).Length;
        _timers = new GameTimer[timerTypeCount];
        for (int i = 0; i < timerTypeCount; ++i)
        {
            _timers[i] = gameObject.AddComponent<GameTimer>();
        }

        GetTimer(E_TIMER.DELIVERY).Init(GameTimer.MODE.COUNTDOWN, 0);
    }

    public void AddTimerCallback(E_TIMER type, Config.DelegateDouble callback)
    {
        _timers[(int)type].AddTimerCallback(callback);
    }

    public void RemoveTimerCallback(E_TIMER type, Config.DelegateDouble callback)
    {
        _timers[(int)type].RemoveTimerCallback(callback);
    }

    public void StopTimer(E_TIMER type)
    {
        _timers[(int)type].StopTimer();
    }

    public void ResumeTimer(E_TIMER type)
    {
        _timers[(int)type].Resume();
    }

    public void PauseTimer(E_TIMER type)
    {
        _timers[(int)type].Pause();
    }

    public void StartTimer(E_TIMER type)
    {
        _timers[(int)type].StartTimer();
    }

    public GameTimer GetTimer(E_TIMER type)
    {
        return _timers[(int)type];
    }

    public void AddSaveSec(E_TIMER type, double addSec)
    {
        _timers[(int)type].AddSaveSeconds(addSec);
    }
}
