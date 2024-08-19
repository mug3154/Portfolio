using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Config;

public class GameMode_Bird : GameMode_Normal
{
    public override void Initialize(GameView gameView, GameUI gameUI, int map, int stage, string[] data)
    {
        GAME_TYPE = Config.GAME_TYPE.BIRD;

        base.Initialize(gameView, gameUI, map, stage, data);
    }

    protected override void SetClearCondition()
    {
        int count = 0;

        foreach (var bubble in _BubbleMap)
        {
            if (bubble == null) continue;

            if(bubble.Type == BUBBLE_TYPE.BIRD) count++;
        }

        RestTargetCount.Value = count;
    }


}
