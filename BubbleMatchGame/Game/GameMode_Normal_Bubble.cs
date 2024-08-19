using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode_Bubble : GameMode_Normal
{
    public override void Initialize(GameView gameView, GameUI gameUI, int map, int stage, string[] data)
    {
        GAME_TYPE = Config.GAME_TYPE.BUBBLE;

        base.Initialize(gameView, gameUI, map, stage, data);
    }

    protected override void SetClearCondition()
    {
        int count = 0;

        foreach(var bubble in _BubbleMap)
        {
            if (bubble != null) ++count;
        }

        RestTargetCount.Value = count;
    }


    protected override void Shoot()
    {
        ++RestTargetCount.Value;
     
        base.Shoot();
    }
}
