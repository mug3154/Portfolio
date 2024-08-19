using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupGameSuccess : PopupBase
{
    [SerializeField] Text _CurrentScore;
    [SerializeField] Text _BestScore;


    protected override void OnPrevShow()
    {
        base.OnPrevShow();

        _CurrentScore.gameObject.SetActive(false);
        _BestScore.gameObject.SetActive(false);
    }

    public void SetScore(int score, int bestScore)
    {
        _CurrentScore.text = $"{score}��!";
        _CurrentScore.gameObject.SetActive(true);

        _BestScore.text = $"�ְ� ������ {bestScore}��!";
        _BestScore.gameObject.SetActive(true);
    }
}

public class PopupBase : MonoBehaviour
{
    public Button CloseBtn;

    protected Action _OnHideCallback;

    private void Start()
    {
        OnStart();
    }

    protected virtual void OnStart()
    {
        if (CloseBtn != null)
        {
            CloseBtn.onClick.AddListener(OnClickCloseBtn);
        }
    }

    public void Show(Action<PopupBase> onShowCallback, Action onHideCallback)
    {
        _OnHideCallback = onHideCallback;

        OnPrevShow();

        gameObject.SetActive(true);

        OnShow();

        onShowCallback?.Invoke(this);
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        _OnHideCallback?.Invoke();
    }

    private void OnClickCloseBtn()
    {
        Hide();
    }

    protected virtual void OnPrevShow()
    {
        //�˾� ���������� �ص� ��ġ ���������� �����ϱ�~
    }

    protected virtual void OnShow()
    {

    }
}