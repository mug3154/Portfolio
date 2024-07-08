using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using Maf;
using UnityEngine.AddressableAssets;

//레스토랑 컨텐츠는 페이지 넘기는 형식으로, 레이아웃을 보면 UI로 보이나, 거리에 캐릭터가 돌아다닐 예정이었으므로 최적화를 위해서 GameObject로 제작하였다.
public class RestaurantController : ContentController
{
    [Header("Properties")]
    [SerializeField] GameObject _groupContainer;
    [SerializeField] Camera _camera;

    List<RestaurantGroup> _restaurantGroups;

    ObjectPool<RestaurantOpen> _restaurantPool;
    ObjectPool<RestaurantBase> _restaurantConstPool;
    ObjectPool<RestaurantBase> _restaurantEmptyPool;

    float _maxCameraX;

    static float _groupOriginWidth = 4.6468f;
    float _groupWidth;
    float _groupScale = 1;

    int _groupCount = 0;

    WaitForFixedUpdate _waitForFixedUpdate;
    Coroutine _coroutineMoveToTarget;


    RestaurantBase _touchRestaurant;
    Vector2 _touchDownPos;
    Vector2 _targetPos;
    bool _isTouchDown;
    float _totalMoveX;


    private void Awake()
    {
        //필요한 클래스 및 리스트 생성
        //////

        _waitForFixedUpdate = new WaitForFixedUpdate();
    }


    private float BaseCameraSize = 5.2f;

    private void OnEnable()
    {
        //레스토랑 컨텐츠 오픈 시 해상도에 따라 카메라 사이즈 조정하여 레스토랑 컨텐츠 비율 조정.

        float originRate = 0.4528301886792453f; //1080 * 1920 해상도 기준으로 제작 됨
        float currRate = Screen.width / (float)Screen.height;

        if(currRate < originRate)
        {
            float AddCameraRate = originRate / currRate;
            _camera.orthographicSize = BaseCameraSize * AddCameraRate;
        }
        else
        {
            _camera.orthographicSize = BaseCameraSize;
        }
    }

    public override void EnterContent()
    {
        if (_restaurantPool == null)
        {
            //ObjectPool 생성
            //............
            
            _groupCount = //레스토랑 테이블 데이터에 따라 레스트랑 그룹 생성 (1그룹 당 9개의 식당 = 한페이지);

            //화면 해상도에 따라 레스토랑 크기 조정
            float height = _camera.orthographicSize * 2;
            _groupWidth = height * Screen.width / Screen.height;
            if (_groupWidth < _groupOriginWidth)
            {
                _groupScale = _groupWidth / _groupOriginWidth;
                _groupWidth += 1.5f;
            }

            //그룹 생성 및 스케일 조정
            //............

            //카메라 최대 이동 가능 X 좌표 설정
            _maxCameraX = (_groupCount - 1) * _groupWidth;
        }

        base.EnterContent();
    }

    public void Redraw()
    {
        if (_restaurantGroups.Count == 0)
            return;

        for (int i = 0; i < _groupCount; ++i)
        {
            _restaurantGroups[i].Draw(this);
        }
    }

    //식당 상태에 따라 오브젝트 가져오기
    public RestaurantBase GetRestaurant(RestaurantBase.E_STATE state)
    {
        if (state == RestaurantBase.E_STATE.OPEN)
        {
            return _restaurantPool.Get();
        }
        else if (state == RestaurantBase.E_STATE.CONSTRUCT)
        {
            return _restaurantConstPool.Get();
        }
        else
        {
            return _restaurantEmptyPool.Get();
        }
    }

    public void DisposeRestaurant(RestaurantBase.E_STATE state, RestaurantBase restaurant)
    {
        //사용하지 않는 식당 오브젝트 재배치
        if (state == RestaurantBase.E_STATE.OPEN)
        {
            _restaurantPool.Release((RestaurantOpen)restaurant);
        }
        else if (state == RestaurantBase.E_STATE.CONSTRUCT)
        {
            _restaurantConstPool.Release(restaurant);
        }
        else
        {
            _restaurantEmptyPool.Release(restaurant);
        }
    }

    //식당 터치 감지
    private void Update()
    {
        if (gameObject.activeInHierarchy == false)
            return;
        if (TouchUtils.IsUITouch())
            return;

        if (_isTouchDown == false)
        {
            if (TouchUtils.IsTouchDown())
            {
                _isTouchDown = true;
                _totalMoveX = 0;

                if (_coroutineMoveToTarget != null)
                {
                    StopCoroutine(_coroutineMoveToTarget);
                    _coroutineMoveToTarget = null;
                }

                _touchDownPos = GetTouchPos();

                _touchRestaurant = GetTouchRestaurant();
            }
        }
        else if (_isTouchDown)
        {   
            if (TouchUtils.IsTouchHeld())
            {
                //손가락 이동 시 페이지 넘김 처리
                Vector2 touchPos = GetTouchPos();
                if (touchPos != _touchDownPos)
                {
                    _touchDownPos = _camera.ScreenToWorldPoint(touchPos);
                    Vector3 pos = _camera.ScreenToWorldPoint(_touchDownPos) - _touchDownPos;

                    _totalMoveX += Mathf.Abs(pos.x);


                    var targetPos = _camera.transform.localPosition + pos;
                    SetCameraPosition(targetPos);
                }
            }
            else if (TouchUtils.IsTouchUp())
            {
                //Touch up 시 식당을 터치 한 경우 해당 식당 팝업 열기.
                _isTouchDown = false;

                if (_totalMoveX < 0.05f)
                {
                    //터치 좌표로 현재 손가락 위치의 식당 가져오기
                    RestaurantBase touchRestaurant = GetTouchRestaurant();
                    if (_touchRestaurant != null && touchRestaurant != null)
                    {
                        if (_touchRestaurant == touchRestaurant)
                        {
                            if (_touchRestaurant.state == RestaurantBase.E_STATE.OPEN)
                            {
                                //식당을 구매한 경우 식당 정보 열기
                            }
                            else if (_touchRestaurant.state == RestaurantBase.E_STATE.CONSTRUCT)
                            {
                                //식당을 구매하지 않은 경우 구매 팝업 열기
                            }
                        }
                    }
                }

                int idx = Mathf.RoundToInt(_camera.transform.localPosition.x / _groupWidth);
                if (idx > _groupCount)
                    idx = _groupCount;

                _targetPos = _restaurantGroups[idx].transform.localPosition;
                _targetPos.y = -2;

                if (_coroutineMoveToTarget != null)
                    StopCoroutine(_coroutineMoveToTarget);

                _coroutineMoveToTarget = StartCoroutine(CoroutineMoveToTarget());
            }
        }
    }

    public void SetCameraPosition(Vector3 pos)
    {
        if (_camera == null)
            return;

        Vector3 cameraPos = pos;
        cameraPos.z = -2;
        cameraPos.y = 0;

        if (cameraPos.x < 0)
            cameraPos.x = 0;
        else if (cameraPos.x > _maxCameraX)
            cameraPos.x = _maxCameraX;


        _camera.transform.localPosition = cameraPos;
    }

    private IEnumerator CoroutineMoveToTarget()
    {
        while (Vector3.Distance(_camera.transform.localPosition, _targetPos) > 0.01f)
        {
            SetCameraPosition(Vector3.Lerp(_camera.transform.localPosition, _targetPos, Time.fixedDeltaTime * 8));

            yield return _waitForFixedUpdate;
        }

        _camera.transform.localPosition = _targetPos;

        _coroutineMoveToTarget = null;
    }

}
