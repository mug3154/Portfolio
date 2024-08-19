using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class Root : Singleton<Root>
{
    public enum SCENE_IDX
    { 
        ROOT,
        TITLE,
        HOME,
        GAME,
    }

    [SerializeField] Loading _Loading;

    [SerializeField] ResManager _ResManager;
    public ResManager ResManager => _ResManager;


    public GameInfo GameInfo { get; private set; }



    private void Start()
    {

        DOTween.Init(this);

        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = false;

        GameInfo = new GameInfo();
        
        
        _Loading.Show();

        GameInfo.LoadMapData(1);

        _ResManager.LoadResource((value) =>
        {
            _Loading.SetProgress(value);

            if (value == 1)
            {
                _Loading.Hide();

                LoadScene(SCENE_IDX.TITLE);
            }
        });
    }



    public void StartGame(int map, int stage)
    {
        var data = Addressables.LoadAssetAsync<TextAsset>($"Map/Map{map}Stages/{stage}.csv").WaitForCompletion();

        var rows = data.text.Split("\r\n");
        if(rows.Length == 0)
        {
            rows = data.text.Split("\n");
        }

        if(Game.Instance != null)
        {
            Home.Instance.Hide();
            Game.Instance.Show(map, stage, rows);
        }
        else
        {
            LoadScene(SCENE_IDX.GAME, (value) =>
            {
                if (value == 1)
                {
                    Home.Instance.Hide();
                    Game.Instance.Show(map, stage, rows);
                }
            });
        }
    }




    public void LoadScene(SCENE_IDX scene, Action<float> progressCallback = null)
    {
        StartCoroutine(LoadAsyncScene(scene, progressCallback));
    }

    IEnumerator LoadAsyncScene(SCENE_IDX scene, Action<float> progressCallback)
    {
        _Loading.Show();

        AsyncOperation async = SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive);

        while(!async.isDone)
        {
            _Loading.SetProgress(async.progress);

            progressCallback?.Invoke(async.progress);

            yield return null;
        }

        _Loading.SetProgress(1);
        progressCallback?.Invoke(1);

        _Loading.Hide();
    }
}
