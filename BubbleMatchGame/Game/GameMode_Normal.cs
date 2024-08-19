using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine.SocialPlatforms.Impl;

public class GameMode_Normal : GameMode
{

    public ReactiveProperty<int> Score { get; protected set; } = new ReactiveProperty<int>(0);
    public int[] Star = new int[3];
    protected int _Combo;
    protected int Combo
    {
        set
        {
            _Combo = value;
            OnChangeCombo?.Invoke(_Combo);
        }

        get => _Combo;
    }
    public Action<int> OnChangeCombo;

    protected bool _IsShoot = false;

    public override void SetGameData(string[] info)
    {
        //info[1] = GAME_CLEAR_TYPE 버블 없애기, 부엉이 없애기 등등, 별1,별2,별3 점수
        //info[2] = 보조컨트롤러유무01,버블개수13,유저사용버블색상1,2,3

        //GAME_TYPE = (Config.GAME_TYPE)(int.Parse(gameType));

        string[] infoRow1 = info[1].Split(",");

        Score.Value = 0;

        for(int i = 0; i < 3; ++i)
        {
            Star[i] = int.Parse(infoRow1[1 + i]);
        }

        base.SetGameData(info);
    }



    protected override void GameSuccess()
    {
        int starCount = 0;
        for(int i = 0; i < 3; ++i)
        {
            if(Score.Value >= Star[i])
            {
                starCount++;
            }
        }

        UserInfo.Instance.SaveStageData(new StageClearData()
        {
            MapIdx = MapIdx,
            StageIdx = StageIdx,
            Score = Score.Value,
            StarCount = starCount
        });

        PopupManager.Instance.ShowPopup(PopupManager.POPUP.GAME_SUCCESS, (popup) =>
        {
            var data = UserInfo.Instance.GetStageClearData(MapIdx, StageIdx);

            ((PopupGameSuccess)popup).SetScore(Score.Value, data.Score);
        }, () =>
        {
            Game.Instance.Hide();
            Home.Instance.Show();
        });
    }

    protected override void SetClearCondition()
    {
    }


    protected override void Shoot()
    {
        _IsShoot = true;
        ++Combo;


        base.Shoot();
    }


    protected override void BubblesPop()
    {
        int pop = Pop();

        pop *= (Combo * 10);

        Score.Value += pop;

        if (_IsShoot)
        {
            _IsShoot = false;

            if (pop == 0)
                Combo = 0;
        }

        SetClearCondition();

        CheckingReadyBubbles();
    }


    public override void Dispose()
    {
        base.Dispose();

        Score?.Dispose();
        Score = null;
        OnChangeCombo = null;
    }
}
