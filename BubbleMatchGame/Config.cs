using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Config
{
    public enum GAME_MODE
    {
        NORMAL,
        BOSS,
    }

    public enum GAME_TYPE : ushort
    {
        NONE = 0,
        BUBBLE,
        BIRD,
    }


    public enum BUBBLE_COLOR : ushort
    { 
        NONE,
        RED,
        YELLOW,
        BLUE,

        NERO = 100,
    }

    public enum BUBBLE_TYPE : ushort
    {
        NONE,
        BIRD,
        EXPLOSION,
        FAIRY,

        NERO = 100,
    }

    public static float BubbleSize = 0.35f;

}
