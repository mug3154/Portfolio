using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserInfo : Singleton<UserInfo>
{
    public Dictionary<int, Dictionary<int, StageClearData>> MapInfo = new Dictionary<int, Dictionary<int, StageClearData>>();

    public void SaveStageData(StageClearData data)
    {
        if(MapInfo.ContainsKey(data.MapIdx) == false)
        {
            MapInfo.Add(data.MapIdx, new Dictionary<int, StageClearData>());
        }

        if(MapInfo[data.MapIdx].ContainsKey(data.StageIdx) == false)
        {
            MapInfo[data.MapIdx].Add(data.StageIdx, data);
        }
        else
        {
            var originData = MapInfo[data.MapIdx][data.StageIdx];
            if(originData.Score < data.Score)
            {
                MapInfo[data.MapIdx][data.StageIdx] = data;
            }
        }
    }

    public StageClearData GetStageClearData(int map, int stage)
    {
        if (MapInfo.ContainsKey(map) == false)
        {
            return new StageClearData();
        }

        if(MapInfo[map].ContainsKey(stage) == false)
        {
            return new StageClearData();
        }

        return MapInfo[map][stage];
    }

    public int GetLastPlayableStage(int map)
    {
        int mapLastIdx = Root.Instance.GameInfo.LastStage(map);
        if (mapLastIdx == -1)
            return 1;

        if (MapInfo.ContainsKey(map))
        {
            var mapData = MapInfo[map];
            foreach(var data in mapData.Reverse())
            {
                if (data.Value.StageIdx == mapLastIdx)
                    return mapLastIdx;
                else
                    return data.Value.StageIdx + 1;
            }
        }

        return 1;
    }
}

public struct StageClearData
{
    public int MapIdx;
    public int StageIdx;
    public int Score;
    public int StarCount;
}