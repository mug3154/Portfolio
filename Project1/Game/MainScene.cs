public class MainScene : Singleton<MainScene>
{
    [Header("==== UI")]
    [SerializeField] MainSceneTopUI _topUI;
    [SerializeField] MainSceneBottomUI _bottomUI;
  
    [SerializeField] Camera _mainCamera;

    RestaurantController _restaurantController;
    DeliveryController _deliveryController;

    public static MainSceneTopUI TopUI { get => Instance._topUI; }
    public static MainSceneBottomUI BottomUI { get => Instance._bottomUI; }

    public static RestaurantController RestaurantController { get => Instance._restaurantController; }
    public static DeliveryController DeliveryController { get => Instance._deliveryController; }

    public enum E_CONTENT
    {
        DELIVERY = 0,
        RESTAURANT,
        HUNT,
        MAX
    }

    private void Start()
    {
        //각종 매니저 초기화
        ///TO DO
        //////////////////////////

        HideContent(E_CONTENT.HUNT);

        StartGame();
    }

    public void StartGame()
    {
        //사냥터 컨텐츠 열기
        ShowContent(E_CONTENT.HUNT, onGroundLoaded: () =>
        {
            //컨텐츠락 체크
            RootScene.userInfo.CheckGuide((uint)RootScene.userInfo.userHuntMaxStage.Value);

            //플레이 가능한 튜토리얼 체크 후 없을 경우 LoadFirstPackets()으로 이동.
            RootScene.userInfo.PlayTutorial(LoadFirstPackets, null);
        });
    }

    public void LoadFirstPackets()
    {
        //게임 접속 시 필요한 각종 통신 로드 (오프라인 보상, 출석체크 등)
    }

    public void ShowContent(E_CONTENT content, bool enter = true, Action onLoaded = null, Action onEntered = null, Action onGroundLoaded = null)
    {
        //경우에 따라 다른 컨텐츠를 닫음.

        //배달 컨텐츠 예시 하나만...
        if (content == E_CONTENT.DELIVERY)
        {
            if (_restaurantController != null)
            {
                _restaurantController.ExitContent();
            }

            if (_deliveryController == null)
            {
                LoadContent("Contents/Delivery", content, enter, onLoaded, () =>
                {
                    onEntered?.Invoke();
                }, onGroundLoaded);
            }
            else
            {
                onLoaded?.Invoke();
                _deliveryController.SetGroundLoaded(onGroundLoaded);
                _deliveryController.EnterContent();
                onEntered?.Invoke();
            }

            //사냥터는 뒤에서 계속 돌아야하므로 UI 및 오류가 날만한 연출만 종료
            _huntController.UI.Hide();
            _huntController.View.KillHuntInOut();
        }
    }

    
    //컨텐츠별로 카메라가 존재하고, 전투 컨텐츠가 아닌 이상 사냥터는 항상 열려있어야하므로 각 컨텐츠의 카메라가 중첩되게끔 사용.
    private void AddContentCamera(ContentController contentController)
    {
        if (_mainCamera != null)
        {
            var cameraData = _mainCamera.GetUniversalAdditionalCameraData();
            var camList = contentController.GetComponentsInChildren<Camera>(true);

            if (camList != null)
            {
                foreach (var cam in camList)
                {
                    if(cameraData.cameraStack.Contains(cam)==false)
                        cameraData.cameraStack.Add(cam);
                }
            }
        }
    }

    public void RemoveContentCamera(ContentController contentController)
    {
        if (_mainCamera != null)
        {
            var cameraData = _mainCamera.GetUniversalAdditionalCameraData();
            var camList = contentController.GetComponentsInChildren<Camera>();

            foreach (var cam in camList)
            {
                cameraData.cameraStack.Remove(cam);
            }
        }
    }

    private void LoadContent(string contentLabel, E_CONTENT content, bool enter = true, Action onLoaded = null, Action onEntered = null, Action onGroundLoaded = null)
    {
        ResourceManager.Instance.LoadAsync<GameObject>(contentLabel).ContinueWith((data) =>
        {
            var inst = GameObject.Instantiate(data, HuntController.transform.parent);
            var contentController = SetContentController(content, inst);

            if (contentController != null)
            {
                AddContentCamera(contentController);
                onLoaded?.Invoke();

                if (enter)
                {
                    contentController.SetGroundLoaded(onGroundLoaded);
                    contentController.EnterContent();
                    onEntered?.Invoke();
                }
                else
                {
                    HideContent(content);
                }
            }
        }).Forget();
    }

    private ContentController SetContentController(E_CONTENT content, GameObject gobj)
    {
        if (content == E_CONTENT.DELIVERY)
        {
            _deliveryController = gobj.GetComponent<DeliveryController>();

            _deliveryController.Init();

            return _deliveryController;
        }
        //..............,

        return null;
    }


    public bool KeyDownEscape()
    {
        if(BottomUI != null)
        {
            if(BottomUI.KeyDownEscape())
            {
                return true;
            }
        }

        if (IsShowContent(E_CONTENT.DUNGEON))
        {
            _dungeonController.KeyDownEscape();
            return true;
        }

        if (IsShowContent(E_CONTENT.RESTAURANT))
        {
            MainScene.Instance.HideContent(MainScene.E_CONTENT.RESTAURANT);
            return true;
        }

        if(IsShowContent(E_CONTENT.DELIVERY))
        {
            return true;
        }

        return false;
    }

}
