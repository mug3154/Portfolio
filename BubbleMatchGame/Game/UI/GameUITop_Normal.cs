using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameUITop_Normal : GameUITop
{
    [SerializeField] RectTransform _ScoreObj;
    [SerializeField] Text _ScoreText;
    [SerializeField] RectTransform _ScoreBar;
    [SerializeField] RectTransform[] _StarPins;
    [SerializeField] GameObject[] _Stars;

    [SerializeField] RectTransform _TargetObj;
    [SerializeField] Image _TargetImg;
    [SerializeField] Text _TargetText;


    [SerializeField] Text _ComboText;

    IDisposable _TargetCountDisposal;
    IDisposable _ScoreDisposal;


    GameMode_Normal _GameMode;
    float _ScoreBarWidth;

    private void Awake()
    {
        _ScoreBarWidth = _ScoreBar.sizeDelta.x;
    }

    public override void Initialize()
    {
        Config.GAME_TYPE gameType = Game.Mode.GAME_TYPE;
        if (gameType == Config.GAME_TYPE.BUBBLE)
        {
            _TargetImg.sprite = Root.Instance.ResManager.GetSprite("Bubble_0");
        }
        else if (gameType == Config.GAME_TYPE.BIRD)
        {
            _TargetImg.sprite = Root.Instance.ResManager.GetSprite("Bird");
        }

        _TargetImg.SetNativeSize();

        _GameMode = (GameMode_Normal)Game.Mode;

        _TargetCountDisposal = _GameMode.RestTargetCount.Subscribe(OnChangeTargetCount);
        _ScoreDisposal = _GameMode.Score.Subscribe(OnChangeScore);


        float maxScore = (float)_GameMode.Star[2];
        float pinY = _StarPins[0].anchoredPosition.y;

        float startPosX = -_ScoreBarWidth * 0.5f;
        for (int i = 0; i < 2; ++i)
        {
            _StarPins[i].anchoredPosition = new Vector2(startPosX + (_ScoreBarWidth * (_GameMode.Star[i] / maxScore)), pinY);
        }

        _GameMode.OnChangeCombo += OnChangeCombo;
        OnChangeCombo(0);
    }

    private void OnChangeCombo(int combo)
    {
        _ComboText.text = $"Combo {combo}";
    }

    private void OnChangeScore(int score)
    {
        _ScoreText.text = score.ToString();

        float percent = (float)score / _GameMode.Star[2];
        if (percent > 1) percent = 1;

        _ScoreBar.sizeDelta = new Vector2(_ScoreBarWidth * percent, _ScoreBar.sizeDelta.y);

        for(int i = 0; i < 3; ++i)
        {
            if (_GameMode.Star[i] < score)
                _Stars[i].gameObject.SetActive(true);
            else
                _Stars[i].gameObject.SetActive(false);
        }

    }

    private void OnChangeTargetCount(int count)
    {
        _TargetText.text = count.ToString();
    }

    public override void Dispose()
    {
        _TargetCountDisposal?.Dispose();
        _TargetCountDisposal = null;

        _ScoreDisposal?.Dispose();
        _ScoreDisposal = null;
    }

}
