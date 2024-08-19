using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class Home : Singleton<Home>
{
    [SerializeField] StageButton _StageButtonPrefab;
    [SerializeField] GameObject _DisableStagePrefab;

    [SerializeField] ScrollRect _ScrollView;

    ObjectPool<StageButton> _StageButtonPool;
    ObjectPool<GameObject> _DisableStagePool;


    List<StageButton> _StageButtons;
    List<GameObject> _DisableStages;


    int _MapIdx = 1;


    private void Start()
    {
        _StageButtonPool = new ObjectPool<StageButton>(() => Instantiate(_StageButtonPrefab));
        _DisableStagePool = new ObjectPool<GameObject>(() => Instantiate(_DisableStagePrefab));

    }

    public void OnClickStageButton(int stage)
    {
        Root.Instance.StartGame(_MapIdx, stage);
    }


    public void Show()
    {
        if(_StageButtons == null)
        {
            _StageButtons = new List<StageButton>();
            _DisableStages = new List<GameObject>();

            MapData mapData = Root.Instance.GameInfo.GetMapData(_MapIdx);

            _ScrollView.content.sizeDelta = new Vector2(_ScrollView.content.sizeDelta.x, mapData.BGHeight);
            _ScrollView.content.anchoredPosition = new Vector2(0, 0);

            StageButton button;
            GameObject disableObj;

            Vector2 selectPos = Vector2.zero;

            int playableStageIdx = UserInfo.Instance.GetLastPlayableStage(_MapIdx);

            foreach (var map in mapData.StageDic)
            {
                var userData = UserInfo.Instance.GetStageClearData(_MapIdx, map.Value.Idx);
                if(playableStageIdx != map.Value.Idx && userData.MapIdx == 0)
                {
                    disableObj = _DisableStagePool.Get();
                    disableObj.transform.SetParent(_ScrollView.content);
                    disableObj.transform.localScale = Vector3.one;
                    disableObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(map.Value.X, map.Value.Y);
                    disableObj.SetActive(true);
                    _DisableStages.Add(disableObj);
                }
                else
                {
                    button = _StageButtonPool.Get();
                    button.transform.SetParent(_ScrollView.content);
                    button.transform.localScale = Vector3.one;
                    button.GetComponent<RectTransform>().anchoredPosition = new Vector2(map.Value.X, map.Value.Y);
                    button.OnClickCallback = OnClickStageButton;
                    button.SetData(map.Value.Idx, map.Value.Mode, map.Value.Value, userData);
                    button.gameObject.SetActive(true);
                    _StageButtons.Add(button);

                    if (playableStageIdx == map.Value.Idx)
                    {
                        selectPos = button.GetComponent<RectTransform>().anchoredPosition;
                    }
                }
            }

            _ScrollView.content.anchoredPosition = new Vector2(0, (selectPos.y * -1) - (_ScrollView.viewport.rect.size.y * 0.5f));
        }
        else
        {
            foreach(var stage in _StageButtons)
            {
                stage.SetClearData(UserInfo.Instance.GetStageClearData(_MapIdx, stage.Stage));
            }
        }


        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        foreach(var stage in _StageButtons)
        {
            _StageButtonPool.Release(stage);
            stage.gameObject.SetActive(false);
        }
        _StageButtons = null;

        foreach(var obj in _DisableStages)
        {
            _DisableStagePool.Release(obj);
            obj.SetActive(false);
        }
        _DisableStages = null;
    }
}

