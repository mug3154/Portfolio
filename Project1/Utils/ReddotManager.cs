using System.Collections;
using System.Collections.Generic;
using System;

public class ReddotManager
{
    private static ReddotManager _instance;
    public static ReddotManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new ReddotManager();
            }

            return _instance;
        }
    }

    public void Init()
    {
        if (_events == null)
            _events = new Dictionary<EVENT_TYPE, bool>();
        else
            _events.Clear();
        
        for(int i = (int)EVENT_TYPE.START; i < (int)EVENT_TYPE.MAX; ++i)
        {
            _events.Add((EVENT_TYPE)i, false);
        }

        if (_receivers == null)
            _receivers = new List<ReddotReceiver>();
        else
            _receivers.Clear();
    }

    public enum EVENT_TYPE
    {
        START = 0,

        CHARACTER_UPGRADE,

        WEAPON_GRADE_UP,
        RECEIVED_NEW_WEAPON,
        
        ........,

        POST_NORMAL,
        POST_PURCHASE,
        MAX
    }

    private Dictionary<EVENT_TYPE, bool> _events;
    private List<ReddotReceiver> _receivers;

    public void AddReceiver(ReddotReceiver receiver)
    {
        _receivers.Add(receiver);

        int max = receiver.eventTypes.Length;
        for (int i = 0; i < max; ++i)
        {
            if (_events[receiver.eventTypes[i]])
            {
                receiver.onCallback?.Invoke(true);
                return;
            }
        }

        receiver.onCallback?.Invoke(false);
    }

    public void RemoveReceiver(ReddotReceiver receiver)
    {
        _receivers.Remove(receiver);
    }

    public void ChangeEvent(ReddotReceiver receiver, EVENT_TYPE[] events)
    {
        if (_receivers.Find(r => r.Equals(receiver)) == null)
            return;

        receiver.SetEvents(events);

        int max = receiver.eventTypes.Length;
        for (int i = 0; i < max; ++i)
        {
            if (_events[receiver.eventTypes[i]])
            {
                receiver.onCallback?.Invoke(true);
                return;
            }
        }

        receiver.onCallback?.Invoke(false);
    }

    public void SetAlarm(EVENT_TYPE type, bool isOn)
    {
        _events[type] = isOn;

        int max;
        bool result;

        for(int i = _receivers.Count - 1; i > -1; --i)
        {
            if (_receivers[i].target is null)
            {
                _receivers.RemoveAt(i);
            }
        }

        foreach (var r in _receivers)
        {
            result = false;
            max = r.eventTypes.Length;

            for (int i = 0; i < max; ++i)
            {
                var receiveEventType = r.eventTypes[i];

                if (_events[receiveEventType])
                {
                    result = true;
                    break;
                }
            }

            r.onCallback?.Invoke(result);
        }
    }

    public bool IsAlarm(EVENT_TYPE type)
    {
        return _events[type];
    }


    public record ReddotReceiver
    {
        public UnityEngine.Object target { private set; get; }
        public EVENT_TYPE[] eventTypes { private set; get; }
        public Action<bool> onCallback { private set; get; }

        public ReddotReceiver SetTarget(UnityEngine.Object target)
        {
            this.target = target;
            return this;
        }

        public ReddotReceiver SetEvents(EVENT_TYPE[] eventTypes)
        {
            this.eventTypes = eventTypes;
            return this;
        }

        public ReddotReceiver SetCallback(Action<bool> onCallback)
        {
            this.onCallback = onCallback;
            return this;
        }
    }
}