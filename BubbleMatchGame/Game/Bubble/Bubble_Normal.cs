using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble_Normal : Bubble
{
    public override void SetData(int x, int y, Config.BUBBLE_COLOR color)
    {
        base.SetData(x, y, color);

        SetMatchCount(3);
    }

    public override int GetPopBubbles(ref Bubble[] result)
    {
        Bubble[,] bubbleMap = Game.Mode.BubbleMap;

        int bubbleMaxY = bubbleMap.GetLength(1);

        List<Bubble> aroundList = new List<Bubble>();
        Queue<Bubble> checkedList = new Queue<Bubble>();
        HashSet<Bubble> completeList = new HashSet<Bubble>();

        //내 주변 먼저 찾기
        GameMode.FindAroundBubble(ref aroundList, ref checkedList, ref completeList, this, ref bubbleMap, bubbleMaxY);

        if (aroundList.Count == 0)
        {
            return 0;
        }
        else
        {
            foreach (Bubble item in aroundList)
            {
                if (item.Color == Color)
                {
                    checkedList.Enqueue(item);
                }
            }

            while (checkedList.Count != 0)
            {
                var checkBubble = checkedList.Dequeue();
                completeList.Add(checkBubble);

                GameMode.FindAroundBubble(ref aroundList, ref checkedList, ref completeList, checkBubble, ref bubbleMap, bubbleMaxY);
                if (aroundList.Count != 0)
                {
                    foreach (Bubble item in aroundList)
                    {
                        if (item.Color == Color)
                        {
                            checkedList.Enqueue(item);
                        }
                    }
                }
            }
        }

        int completeCount = completeList.Count;
        if (completeCount < MatchCount)
        {
            return 0;
        }

        if (completeCount > result.Length)
        {
            result = new Bubble[completeCount];
        }

        completeList.CopyTo(result, 0);

        return completeCount;
    }

}
