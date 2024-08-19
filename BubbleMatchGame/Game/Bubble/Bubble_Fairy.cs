using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Bubble_Fairy : Bubble_Normal
{
    Bubble_Bairy_Behavior _Behavior;

    public override void SetData(int x, int y, Config.BUBBLE_COLOR color)
    {
        GetComponent<SpriteRenderer>().enabled = true;

        if(Game.Instance.GAME_MODE == Config.GAME_MODE.NORMAL)
        {
            _Behavior = new Bubble_Bairy_Behavior_Normal();
        }
        else
        {
            _Behavior = new Bubble_Bairy_Behavior_Boss();
        }

        base.SetData(x, y, color);
    }

    public override bool Pop(float delay, Action<Bubble, bool> endCallback)
    {
        return _Behavior.Pop(delay, endCallback, this);
    }

    public override void Dispose()
    {
        base.Dispose();

        _Behavior = null;
    }
}

public abstract class Bubble_Bairy_Behavior
{
    public abstract bool Pop(float delay, Action<Bubble, bool> endCallback, Bubble i);
}

public class Bubble_Bairy_Behavior_Normal : Bubble_Bairy_Behavior
{
    public override bool Pop(float delay, Action<Bubble, bool> endCallback, Bubble i)
    {
        List<Bubble> result = Game.Mode.GetShowBubbles();

        if (result.Count > 0)
        {
            int random = UnityEngine.Random.Range(0, result.Count);

            i.ChangeXY(result[random].X, result[random].Y);

            i.transform.SetParent(Game.Mode.View.EffectContainer);

            i.GetComponent<SpriteRenderer>().enabled = false;

            Sequence seq = DOTween.Sequence();
            seq.Append(i.transform.DOMove(i.transform.position + (UnityEngine.Random.insideUnitSphere * 1f), 0.3f));
            seq.AppendInterval(0.7f);
            seq.AppendCallback(() => { i.transform.SetParent(Game.Mode.View.BubbleContainer); });
            seq.Append(i.transform.DOLocalMove(Game.Mode.View.GetBubblePos(i.X, i.Y), 0.3f));
            seq.OnComplete(() => {

                Game.Mode.SetMatchCountOfBubble(i.X, i.Y, 1);

                endCallback?.Invoke(i, true);
            });

            return false;
        }
        else
        {
            //붙을게 없음!
            return true;
        }
    }
}


public class Bubble_Bairy_Behavior_Boss : Bubble_Bairy_Behavior
{
    public override bool Pop(float delay, Action<Bubble, bool> endCallback, Bubble i)
    {
        //보스를 공격

        Sequence seq = DOTween.Sequence();
        seq.Append(i.transform.DOMove(i.transform.position + (UnityEngine.Random.insideUnitSphere * 1f), 0.3f));
        seq.AppendCallback(() => { i.transform.SetParent(Game.Mode.View.BubbleContainer); });
        seq.Append(i.transform.DOLocalMove(Game.Mode.View.GetBossPos(), 0.3f));
        seq.OnComplete(() => {

            ((GameMode_Boss)Game.Mode).AttackToBoss();

            endCallback?.Invoke(i, true);
        });

        return false;
    }
}