using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameTimer : MonoBehaviour
{
    public enum MODE
    {
        COUNTDOWN,
        STOPWATCH,
        TIMER
    }
    private MODE _mode;

    public double saveSeconds { private set; get; }
    public double currentSeconds { private set; get; }

    private Coroutine _coroutine;
    private Config.DelegateDouble _onTimer;

    private bool _isPause = false;
    private bool _isCoroutineBreak = false;

    private delegate bool DelegateTime();
    private DelegateTime _callback;

    private float _WaitForSecondsValue = 0.016667f;
    private WaitForSeconds _WaitForSeconds = new WaitForSeconds(0.016667f);

    public void Init(MODE mode, double saveSecond = 0)
    {
        _mode = mode;

        StopTimer();

        ResetTimer(saveSecond);

        if (_mode == MODE.COUNTDOWN)
            _callback = CountDown;
        else if (mode == MODE.STOPWATCH)
            _callback = StopWatch;
        else
            _callback = Timer;
        
    }

    public void ResetTimer(double saveSecond)
    {
        this.saveSeconds = saveSecond;
        currentSeconds = saveSecond;
    }

    public void AddTimerCallback(Config.DelegateDouble callback)
    {
        _onTimer += callback;
    }

    public void RemoveTimerCallback(Config.DelegateDouble callback)
    {
        _onTimer -= callback;
    }

   
    public void StopTimer()
    {
        _isCoroutineBreak = true;
        _isPause = true;
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    public void StartTimer()
    {
        StopTimer();

        currentSeconds = saveSeconds;

        _isCoroutineBreak = false;
        _isPause = false;
        _coroutine = StartCoroutine(TimerCount());
    }

    public void AddSaveSeconds(double addSec)
    {
        saveSeconds += addSec;
        currentSeconds += addSec;

        if (currentSeconds > saveSeconds)
            currentSeconds = saveSeconds;
        else if (currentSeconds < 0)
            currentSeconds = 0;
    }

    public void Resume()
    {
        if (_isPause && _coroutine != null)
        {
            _isPause = false;
            StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(TimerCount());
        }
    }

    public void Pause()
    {
        if (_isPause == false &&_coroutine != null)
        {
            _isPause = true;
            StopCoroutine(_coroutine);
        }
    }

    public IEnumerator TimerCount()
    {
        while (!_isCoroutineBreak)
        {
            while (_isPause)
            {
                yield return _WaitForSeconds;
            }

            if (_callback() == false)
            {
                break;
            }

            yield return _WaitForSeconds;
        }
    }


    public void SetCurrentSeconds(double seconds)
    {
        currentSeconds = seconds;

        if (_mode == MODE.COUNTDOWN)
            _onTimer?.Invoke(currentSeconds);
        else if (_mode == MODE.STOPWATCH)
            _onTimer?.Invoke(currentSeconds);
    }

    public void AddCurrentSeconds(double addSec)
    {
        currentSeconds += addSec;

        if (_mode == MODE.COUNTDOWN)
            _onTimer?.Invoke(currentSeconds);
        else if (_mode == MODE.STOPWATCH)
            _onTimer?.Invoke(currentSeconds);
    }


    public void MinusCurrentSeconds(double minusSec)
    {
        currentSeconds -= minusSec;
        if(currentSeconds <= 0)
        {
            currentSeconds = 0;
        }

        if (_mode == MODE.COUNTDOWN)
            _onTimer?.Invoke(currentSeconds);
        else if (_mode == MODE.STOPWATCH)
            _onTimer?.Invoke(currentSeconds);
    }


    private bool CountDown()
    {
        if (currentSeconds <= 0)
            return false;

        currentSeconds -= _WaitForSecondsValue;
        _onTimer?.Invoke(currentSeconds);

        return true;
    }

    private bool StopWatch()
    {
        currentSeconds += _WaitForSecondsValue;
        _onTimer?.Invoke(currentSeconds);

        return true;
    }

    private bool Timer()
    {
        currentSeconds += _WaitForSecondsValue;
        if(currentSeconds >= 1)
        {
            currentSeconds -= 1;
            _onTimer?.Invoke(1);
        }

        return true;
    }
}
