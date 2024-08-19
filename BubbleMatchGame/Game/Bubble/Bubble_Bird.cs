using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Bubble_Bird : Bubble_Normal
{

    public override bool Pop(float delay, Action<Bubble, bool> endCallback)
    {
        DOTween.Kill(this);

        return base.Pop(delay, endCallback);
    }

    public override bool Fall(Vector3 endPos, Action<Bubble, bool> endCallback)
    {
        HashSet<Vector2Int> find = new HashSet<Vector2Int>();
        List<Vector2Int> result = new List<Vector2Int>();

        Bubble[,] bubbleMap = Game.Mode.BubbleMap;

        int mapY = bubbleMap.GetLength(1);

        Bubble bubble;

        int bottomY = Game.Mode.BottomBubbleY;
        int top = bottomY - 10;
        if (top < 0) top = 0;

        for (int y = bottomY; y >= top; --y)
        {
            for (int x = 0; x < 11; ++x)
            {
                bubble = bubbleMap[x, y];
                if (bubble != null)
                {
                    if (bubble.Color != Color) continue;

                    GameMode.GetEmtpyCellOfAround(ref find, bubble.X, bubble.Y, ref bubbleMap, mapY);

                    result.AddRange(find);
                }
            }
        }

        if (result.Count == 0)
        {
            for (int y = bottomY; y > -1; --y)
            {
                for (int x = 0; x < 11; ++x)
                {
                    bubble = bubbleMap[x, y];
                    if (bubble != null)
                    {
                        GameMode.GetEmtpyCellOfAround(ref find, bubble.X, bubble.Y, ref bubbleMap, mapY);

                        result.AddRange(find);
                    }
                }
            }
        }

        if (result.Count > 0)
        {
            int random = UnityEngine.Random.Range(0, result.Count);

            //bubbleMap[X, Y] = null;

            X = result[random].x;
            Y = result[random].y;
            bubbleMap[X, Y] = this;

            transform.SetParent(Game.Mode.View.EffectContainer);
            
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOMove(transform.position + (UnityEngine.Random.insideUnitSphere * 1f), 0.3f));
            seq.AppendInterval(0.7f);
            seq.AppendCallback(() => { transform.SetParent(Game.Mode.View.BubbleContainer); });
            seq.Append(transform.DOLocalMove(Game.Mode.View.GetBubblePos(X, Y), 0.3f));
            seq.OnComplete(() => { endCallback?.Invoke(this, false); });

            return false;
        }
        else
        {
            //붙을게 없음!
            Pop(0, endCallback);

            return true;
        }
    }

    public override void ForceFall(Vector3 endPos, Action<Bubble, bool> endCallback)
    {
        Pop(0, endCallback);
    }
}
