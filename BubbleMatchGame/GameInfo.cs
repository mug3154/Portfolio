using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameInfo
{
    Dictionary<int, MapData> _MapDic = new Dictionary<int, MapData>();

    public MapData GetMapData(int map)
    {
        if(_MapDic.ContainsKey(map))
            return _MapDic[map];
        else
            return null;
    }

    public void LoadMapData(int map)
    {
        Root.Instance.ResManager.LoadMapData(map, (data) =>
        {
            if (_MapDic.ContainsKey(map) == false)
            {
                _MapDic.Add(map, new MapData());
            }
                
            _MapDic[map].SetData(map, data);
    
        }, null);
    }

    public int LastStage(int map)
    {
        var data = GetMapData(map);
        
        if (data == null) 
            return -1;
        else 
            return data.StageDic.Last().Value.Idx;
    }
}

public class MapData
{
    public int Idx { get; private set; }
    public int BG {  get; private set; }
    public int BGHeight {  get; private set; }

    public Dictionary<int, MapStageData> StageDic = new Dictionary<int, MapStageData>();

    public void SetData(int idx, string data)
    {
        Idx = idx;

        var rows = data.Split("\r\n");
        if (rows.Length == 0)
        {
            rows = data.Split("\n");
        }

        string[] stageData = rows[0].Split(",");

        BG = int.Parse(stageData[0]);
        BGHeight = int.Parse(stageData[1]);

        Config.GAME_MODE mode;
        int x;
        int y;

        for (int i = 1; i < rows.Length; i++)
        {
            stageData = rows[i].Split(",");

            mode = (Config.GAME_MODE)int.Parse(stageData[0]);
            x = int.Parse(stageData[2]);
            y = int.Parse(stageData[3]);

            MapStageData stage = new MapStageData();
            stage.SetData(i, mode, int.Parse(stageData[1]), x, y);
            StageDic.Add(i, stage);
        }
    }

}

public struct MapStageData
{
    public int Idx { get; private set; }
    public Config.GAME_MODE Mode { get; private set; }
    public int Value { get; private set; }

    public int X { get; private set; }
    public int Y { get; private set; }

    public void SetData(int idx, Config.GAME_MODE mode, int value, int x, int y)
    {
        Idx = idx;
        Mode = mode;
        Value = value;
        X = x; Y = y;
    }
}