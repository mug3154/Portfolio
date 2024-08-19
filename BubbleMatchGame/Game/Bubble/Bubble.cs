using DG.Tweening;
using System;
using UnityEngine;
using static Config;

public class Bubble : MonoBehaviour
{
    public int X { get; protected set; }
    public int Y { get; protected set; }
    public BUBBLE_COLOR Color { get; protected set; }
    public BUBBLE_TYPE Type;

    public int MatchCount { get; protected set; }

    //보스 모드 정보.. 나중에 모드별 행동 코드 만들 때 빼기..
    public int Line = -1;
    public int LineIndex;


    public virtual void SetData(int x, int y, BUBBLE_COLOR color)
    {
        X = x; 
        Y = y;

        Color = color;

        GetComponent<SpriteRenderer>().sprite = Root.Instance.ResManager.GetBubbleSprite((int)color);

        Line = -1;

        gameObject.name = $"{X}_{Y}";
    }

    public void ChangeXY(int x, int y)
    {
        X= x; Y=y;
    }

    public void SetMatchCount(int matchCount)
    {
        MatchCount = matchCount;
    }

    public virtual bool HaveSideEffectOfShootBubble()
    {
        return false;
    }

    public virtual int GetPopBubbles(ref Bubble[] result)
    {
        return 0;
    }

    public virtual bool Pop(float delay, Action<Bubble, bool> endCallback)
    {
        transform.DOPunchScale(Vector3.one * 1.2f, 0.3f).SetDelay(delay).OnComplete(() =>
        {
            endCallback?.Invoke(this, true);
        });

        return true;
    }

    public virtual void ForceFall(Vector3 endPos, Action<Bubble, bool> endCallback)
    {
        Fall(endPos, endCallback);
    }

    public virtual bool Fall(Vector3 endPos, Action<Bubble, bool> endCallback)
    {
        transform.DOMove(endPos, 0.5f).OnComplete(() =>
        {
            endCallback?.Invoke(this, true);
        });
        return true;
    }

    public virtual void Dispose()
    {
        DOTween.Kill(transform);
    }
}
