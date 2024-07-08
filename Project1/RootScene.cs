using System.Collections;


#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class RootScene : Singleton<RootScene>
{
    [Header("[ Root ]======")]
    [SerializeField] TitleScene _title;

    [SerializeField] Transform _popupLayout;
    [SerializeField] Transform _TutorialLayout;
    [SerializeField] GameObject _TopLayout;

    [SerializeField] PopupLoading loading; //모든 레이어보다 최상위에 있어야하므로 따로 관리.
    public PopupFade Fade;

    public delegate void OnProgress(float value);
    public delegate void OnCompleteSceneLoad(int sceneIdx);
    public delegate void OnCompleteSceneUnLoad(int sceneIdx);

    [SerializeField] AtlasPool _atlasPool;
    public static AtlasPool atlasPool { get => Instance._atlasPool; }
    [SerializeField] PrefabPool _prefabPool;
    public static PrefabPool prefabPool { get => Instance._prefabPool; }



    private UserInfo _userInfo;
    public static UserInfo userInfo { get => Instance._userInfo; }


    public static UISystem uiSystem;

    public static Config.DelegateLocalized OnLocalized = null;


    double _TimeSec = 0;

    CustomTimer _TimerTemp;
    List<CustomTimer> _Timers = new List<CustomTimer>();
    List<CustomTimer> _TimerPool = new List<CustomTimer>();
    List<CustomTimer> _RemovableTimers = new List<CustomTimer>();

    double _PlayTimeSec;

    int _ShowLoadingCnt;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //ui관리 시스템. 회사 프레임워크이므로 미첨부.
        uiSystem = new UISystem();

        _TopLayout.SetActive(true);
        loading.Hide();
        Fade.Hide();

        LoadTitle();

    }

    private void Update()
    {
        _TimeSec = Time.unscaledDeltaTime;
        for (int i = _Timers.Count - 1; i > -1; --i)
        {
            _TimerTemp = _Timers.ElementAt(i);
            _TimerTemp?.Update(_TimeSec);
        }

        foreach (var remove in _RemovableTimers)
        {
            _Timers.Remove(remove);
            _TimerPool.Add(remove);
        }
        _RemovableTimers.Clear();


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (loading.gameObject.activeSelf)
                return;

            if (uiSystem.ClosePopupBackBtn())
            {
                //팝업 닫힘
            }
            else if (MainScene.Instance.KeyDownEscape())
            {
                //메인 백버튼 처리 됨
            }
            else
            {
                //게임 종료 처리 팝업 띄움.
            }
        }
    }


    public void LoadTitle()
    {
        //로그아웃 한 경우 타이틀로 돌아오므로 여기서 리소스 초기화.
        PacketManager.Instance.AllNetworkAbort();
        TimelineManager.Instance.AllTimelineAbort();
        AudioManager.Instance.PlayBGM("BGM_beach");

        _userInfo = new UserInfo();

        for (int i = _Timers.Count - 1; i >= 0; --i)
        {
            RemoveTimer(_Timers[i]);
        }
        _Timers.Clear();

        ReddotManager.instance.Init(); 

        if (_title == null)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>("Title").WaitForCompletion();
            _title = Instantiate(handle.GetComponent<TitleScene>());
            _title.transform.SetParent(transform);
            _title.transform.localScale = Vector3.one;
            _title.transform.localPosition = Vector3.zero;
            Addressables.Release(handle);
        }
        _title.OnHideCallback = OnEndTitle;
        _title.InitServer();
    }

    public void OnEndTitle()
    {
        if (_title != null)
        {
            Destroy(_title.gameObject);
            _title = null;
        }
    }

    #region TUTORIAL

    Vector3 _TutorialPos = new Vector3(0, 0, 5000);

    uint _TutorialIdx = 0;
    public bool IsDeliveryTutorial = false;
    bool _IsTutorialLoading = false;

    public bool IsPlayingTutorial()
    {
        return _TutorialLayout.childCount > 0;
    }

    public void LoadTutorial(uint tutorialIdx)
    {
        LoadTutorial<TutorialTimeline>(tutorialIdx);
    }

    public T LoadTutorial<T>(uint tutorialIdx) where T : TutorialTimeline
    {
        if (_IsTutorialLoading || IsPlayingTutorial())
            return null;

        _IsTutorialLoading = true;

        //튜토리얼 정보 유효성 체크
        var table = Tables.Instance.GetTable<Tutorial>();
        var data = table.GetData(tutorialIdx);
        if (data.code == 0)
        {
            _IsTutorialLoading = false;
            return null;
        }


        //튜토리얼 타임라인 로드
        var handler = Addressables.LoadAssetAsync<GameObject>(data.name).WaitForCompletion();
        if (handler == null)
        {
            _IsTutorialLoading = false;
            return null;
        }

        T tutorial = Instantiate<T>(handler.GetComponent<T>(), _TutorialLayout);
        tutorial.transform.position = _TutorialPos;
        tutorial.Play();
        tutorial.OnEndCallback = OnEndTutorial;
        Addressables.Release(handler);

        _TutorialIdx = tutorialIdx;

        _IsTutorialLoading = false;

        return tutorial;
    }

    private void OnEndTutorial()
    {
        _TopLayout.SetActive(true);

        DeliveryTutorial = null;
    }
    #endregion TUTORIAL

    #region STORY

    Action _OnEndStoryCallback;
    public TutorialTimeline LoadStoryTimeline(string timelineName, float timeScale = 0, Action onEndCallback = null, Action<bool> onSkipCallback = null)
    {
        if (_IsTutorialLoading)
        {
            onEndCallback?.Invoke();
            return null;
        }

        if (IsPlayingTutorial())
        {
            onEndCallback?.Invoke();
            return null;
        }

        _IsTutorialLoading = true;

        GameObject handler = Addressables.LoadAssetAsync<GameObject>(timelineName.Trim()).WaitForCompletion();
        if (handler == null)
        {
            _IsTutorialLoading = false;
            return null;
        }

        TutorialTimeline tutorial = Instantiate<TutorialTimeline>(handler.GetComponent<TutorialTimeline>(), _TutorialLayout);
        if (tutorial == null)
        {
            _IsTutorialLoading = false;
            return null;
        }

        _OnEndStoryCallback = onEndCallback;

        tutorial.transform.position = _TutorialPos;
        tutorial.Play();
        tutorial.OnEndCallback = OnEndStoryTimeline;
        
        if(onSkipCallback != null)
        {
            tutorial.OnEndSkipCallback = onSkipCallback;
        }

        Addressables.Release(handler);

        _TopLayout.SetActive(false);
        
        _IsTutorialLoading = false;

        if(tutorial == null)
        {
            onEndCallback?.Invoke();
        }

        return tutorial;
    }

    private void OnEndStoryTimeline()
    {
        _TopLayout.SetActive(true);

        _OnEndStoryCallback?.Invoke();
    }
    #endregion STORY


    #region Loading
    public void ShowLoading(string callClassName)
    {
        if (_TutorialLayout.childCount > 0)
            return;

        ++_ShowLoadingCnt;

        //로딩이 안닫히는 경우의 검증용
        //Debug.Log("<<<<<   ShowLoading : " + callClassName + ", _ShowLoadingCnt:"+ _ShowLoadingCnt);

        loading.Show();
    }

    public void HideLoading(string callClassName, bool force = false)
    {
        if(force)
        {
            _ShowLoadingCnt = 0;
            loading.Hide();
            return;
        }

        --_ShowLoadingCnt;

        //로딩이 안닫히는 경우의 검증용
        //Debug.Log("<<<<<   HideLoading : " + callClassName + ", _ShowLoadingCnt:" + _ShowLoadingCnt);

        if (_ShowLoadingCnt > 0)
            return;

        _ShowLoadingCnt = 0;

        loading.Hide();
    }
    #endregion Loading


    #region Timer
    public CustomTimer CreateCountDown(double restSec, Action<double> onChangeCallback)
    {
        CustomTimer timer;// = GenericPool<CustomTimer>.Get();

        if(_TimerPool.Count > 0)
        {
            timer = _TimerPool[0];
            _TimerPool.RemoveAt(0);
        }
        else
        {
            timer = new CustomTimer();
        }

        timer.Init(CustomTimer.TYPE.Countdown, restSec, onChangeCallback);
        _Timers.Add(timer);

        return timer;
    }

    public CustomTimer CreateTimer(Action<double> onChangeCallback)
    {
        CustomTimer timer;

        if (_TimerPool.Count > 0)
        {
            timer = _TimerPool[0];
            _TimerPool.RemoveAt(0);
        }
        else
        {
            timer = new CustomTimer();
        }

        timer.Init(CustomTimer.TYPE.Timer, 0, onChangeCallback);
        _Timers.Add(timer);

        return timer;
    }

    public void RemoveTimer(CustomTimer timer)
    {
        if (timer == null)
            return;

        timer.Reset();
        _RemovableTimers.Add(timer);
    }
    #endregion Timer
}

public class CustomTimer
{
    public enum TYPE
    {
        Countdown,
        Timer
    }
    TYPE _Type;


    double _OriginRestSec;
    double _RestSec;
    public double RestSec { get => _RestSec; }
    public Action<double> OnChangeTimeCallback;
    
    Action<double> _OnUpdate;

    public void Reset()
    {
        _OnUpdate = null;
        OnChangeTimeCallback = null;
    }

    public void Init(TYPE type, double restSec, Action<double> onChangeCallback)
    {
        Reset();

        _Type = type;

        _OriginRestSec = restSec;
        _RestSec = restSec;

        OnChangeTimeCallback += onChangeCallback;

        if (_Type == TYPE.Countdown)
            _OnUpdate = Countdown;
        else
            _OnUpdate = Timer;
    }

    public void SetSec(double sec)
    {
        _RestSec = sec;
    }


    public void Update(double updateSec)
    {
        _OnUpdate?.Invoke(updateSec);
    }

    protected void Timer(double sec)
    {
        OnChangeTimeCallback?.Invoke(sec);
    }

    protected void Countdown(double sec)
    {
        double prev = _RestSec;

        _RestSec -= sec;
        if (_RestSec < 0)
            _RestSec = 0;

        if (prev != _RestSec)
        {
            OnChangeTimeCallback?.Invoke(_RestSec);
        }
    }
}