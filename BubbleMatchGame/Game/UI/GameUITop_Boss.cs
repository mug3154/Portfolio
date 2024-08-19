using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameUITop_Boss : GameUITop
{
    [SerializeField] RectTransform _HpBar;
    [SerializeField] Image _BossIcon;

    [SerializeField] Text _ComboText;
 
    float _HpBarWidth;

    GameMode_Boss _GameMode;

    IDisposable _ScoreDisposal;


    private void Awake()
    {
        _HpBarWidth = _HpBar.sizeDelta.x;
    }

    public override void Initialize()
    {
        _GameMode = (GameMode_Boss)Game.Mode;

        _BossIcon.overrideSprite = Root.Instance.ResManager.GetSprite($"Boss_{_GameMode.BossIdx}");

        _ScoreDisposal = _GameMode.RestTargetCount.Subscribe(OnChangeHP);

    }

    private void OnChangeHP(int curr)
    {
        _HpBar.sizeDelta = new Vector2((curr / (float)_GameMode.MaxHP) * _HpBarWidth, _HpBar.sizeDelta.y);
    }

    public override void Dispose()
    {
        _ScoreDisposal?.Dispose();
        _ScoreDisposal = null;
    }
}
