using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameUI : MonoBehaviour
{
    [SerializeField] RectTransform _Top;
    public RectTransform Top => _Top;

    [SerializeField] GameUI_Player _Player;


    GameUITop _GameUITop;


    public void Initialize(Config.GAME_MODE mode, Action<Vector2> moveShootingLineCallback, Action cancelShootingLineCallback)
    {
        if(mode == Config.GAME_MODE.NORMAL)
        {
            var handler = Addressables.LoadAssetAsync<GameObject>("GameUITop_Normal").WaitForCompletion();
            _GameUITop = Instantiate(handler).GetComponent<GameUITop_Normal>();
        }
        else
        {
            var handler = Addressables.LoadAssetAsync<GameObject>("GameUITop_Boss").WaitForCompletion();
            _GameUITop = Instantiate(handler).GetComponent<GameUITop_Boss>();
        }

        _GameUITop.transform.SetParent(_Top);
            
        RectTransform rect = _GameUITop.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.sizeDelta = _Top.sizeDelta;
        rect.anchoredPosition = Vector3.zero;


        _GameUITop.Initialize();

        _Player.Initialize(moveShootingLineCallback, cancelShootingLineCallback);
    }

    public void Dispose()
    {
        if(_GameUITop != null )
        {
            _GameUITop.Dispose();
            Destroy(_GameUITop.gameObject);
            Destroy(_GameUITop);
            _GameUITop = null;
        }

        _Player.Dispose();
    }
}
