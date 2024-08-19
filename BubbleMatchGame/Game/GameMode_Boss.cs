using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Config;

public class GameMode_Boss : GameMode
{
    public ushort BossIdx { get; private set; }
    public int MaxHP { get; private set; }

    Config.BUBBLE_COLOR[] _BossBubbleColors;
    ushort _BossBubbleColorLength;

    Config.BUBBLE_TYPE[] _BossBubbleTypes;
    ushort _BossBubbleTypeLength;

    Dictionary<int, List<BossStageBubbleData>> _LineInfos = new Dictionary<int, List<BossStageBubbleData>>(); //라인 정보
    Dictionary<int, int> _PrevRemovedBubbleCounts = new Dictionary<int, int>(); //방금 전에 라인에서 삭제 된 구슬 개수

    public override void Initialize(GameView gameView, GameUI gameUI, int map, int stage, string[] data)
    {
        MapIdx = map;
        StageIdx = stage;

        SetGameData(data);

        GameValueInitialize();

        _View = gameView;
        _View.Initialize(ShowGuideBubble, CancelBubbleLine);

        _UI = gameUI;
        _UI.Initialize(Config.GAME_MODE.BOSS, MoveBubbleLine, CancelBubbleLine);

        SetGameFlowEventReceivers();

        CreateBubbleMap(data);

        SetClearCondition();

        CheckingReadyBubbles();
    }

    public override void SetGameData(string[] info)
    {
        //0 = 게임타입2
        //1 = 보스이미지,HP
        //보스 사용 버블 컬러, 
        //보스 사용 버블 타입
        //보조컨트롤러유무01,버블개수13,유저사용버블색상1,2,3,
        //버블라인

        string[] bossInfo = info[1].Split(",");
        string[] bossBubbleColor = info[2].Split(",");
        string[] bossBubbleType = info[3].Split(",");
        string[] gameInfo = info[4].Split(",");

        BossIdx = ushort.Parse(bossInfo[0]);

        RestTargetCount.Value = ushort.Parse(bossInfo[1]);
        MaxHP = RestTargetCount.Value;


        //보스 버블 컬러
        int rowLength = bossBubbleColor.Length;
        _BossBubbleColors = new Config.BUBBLE_COLOR[rowLength];
        for (int i = 0; i < rowLength; i++)
        {
            _BossBubbleColors[i] = (Config.BUBBLE_COLOR)int.Parse(bossBubbleColor[i]);
        }
        _BossBubbleColorLength = (ushort)_BossBubbleColors.Length;

        //보스 버블 타입
        rowLength = bossBubbleType.Length;
        _BossBubbleTypes = new Config.BUBBLE_TYPE[rowLength];
        for (int i = 0; i < rowLength; i++)
        {
            _BossBubbleTypes[i] = (Config.BUBBLE_TYPE)int.Parse(bossBubbleType[i]);
        }
        _BossBubbleTypeLength = (ushort)_BossBubbleTypes.Length;

        //유저용 버블
        rowLength = gameInfo.Length;
        _UsableBubbleColors = new Config.BUBBLE_COLOR[rowLength - 2];
        for (int i = 2; i < rowLength; i++)
        {
            _UsableBubbleColors[i - 2] = (Config.BUBBLE_COLOR)int.Parse(gameInfo[i]);
        }
        _UsableBubbleColorLength = _UsableBubbleColors.Length;


        IsNeroMode = gameInfo[0] == "1";

        RestBubbleCount.Value = int.Parse(gameInfo[1]);
    }


    protected override void SetClearCondition()
    {

    }


    protected override void GameSuccess()
    {
        UserInfo.Instance.SaveStageData(new StageClearData()
        {
            MapIdx = MapIdx,
            StageIdx = StageIdx,
            Score = 0,
            StarCount = 3
        });

        PopupManager.Instance.ShowPopup(PopupManager.POPUP.GAME_SUCCESS, (popup) =>
        {

        }, () =>
        {
            Game.Instance.Hide();
            Home.Instance.Show();
        });
    }


    protected async override void CheckingReadyBubbles()
    {
        if (_IsChangeBottomY)
        {
            CheckBubbleBottomY();
        }
        else if (_BubbleEventCallbacks.Count != 0)
        {

        }
        else if (_CheckReadyBubbles.Count == 0)
        {
            await MakeBubbles();

            GameFlowEventManager.Instance.Notify(GameFlowEventManager.GAME_FLOW_EVENT.CHECK_CLEAR_CONDITION);
        }
        else
        {
            BubblesPop();
        }
    }

    public override void CreateBubbleMap(string[] data)
    {
        int infoLineCount = 5; //위에서부터 모드, 보스 정보, 보스 컬러, 보스 버블, 게임 정보까지 4줄 삭제

        string[] bubbleLineData = new string[data.Length - infoLineCount];

        for (int i = infoLineCount; i < data.Length; i++)
        {
            bubbleLineData[i - infoLineCount] = data[i];
        }

        //헷갈리니까 데이터 세팅 먼저

        string[] bubbleRowData;
        string[] bubbleData;

        List<BossStageBubbleData> bubbleInfos = new List<BossStageBubbleData>();

        int columnCount;

        _BubbleMapMaxY = bubbleLineData.Length;
        _BottomBubbleY = 0;

        int maxLine = 0;

        for (int row = 0; row < _BubbleMapMaxY; row++)
        {
            bubbleRowData = bubbleLineData[row].Split("\t");

            columnCount = bubbleRowData.Length;

            for (int column = 0; column < columnCount; column++)
            {
                if (bubbleRowData[column] == "") continue;

                bubbleData = bubbleRowData[column].Split(","); //x,y,line,idx

                int X = int.Parse(bubbleData[0]) - 1;
                int Y = int.Parse(bubbleData[1]) - 1;
                int line = int.Parse(bubbleData[2]);

                if (line == 999) //boss
                {
                    if(int.Parse(bubbleData[3]) == 100)
                    {
                        _View.SetBoss(BossIdx, X, Y);
                    }
                }
                else
                {
                    bubbleInfos.Add(new BossStageBubbleData()
                    {
                        X = X,
                        Y = Y,
                        Line = line,
                        Idx = int.Parse(bubbleData[3]),
                    });

                    if (maxLine < line)
                        maxLine = line;
                }

                if (Y > _BottomBubbleY)
                    _BottomBubbleY = Y;
            }
        }

        //라인 정보 재분배~
        _LineInfos = new Dictionary<int, List<BossStageBubbleData>>();
        for(int i = 0; i <= maxLine; ++i)
        {
            _LineInfos.Add(i, new List<BossStageBubbleData>());
        }

        foreach(var bubbleInfo in bubbleInfos)
        {
            _LineInfos[bubbleInfo.Line].Add(bubbleInfo);
        }

        for (int i = 0; i <= maxLine; ++i)
        {
            _LineInfos[i].Sort((BossStageBubbleData a, BossStageBubbleData b) =>
            {
                if (a.Idx < b.Idx) return -1;
                else if (a.Idx > b.Idx) return 1;
                else return 0;
            });

            _PrevRemovedBubbleCounts.Add(i, _LineInfos[i].Count);
        }

        //사용할 수 있는 구슬 수 만큼 일렬로 배치하는 것을 방지하기 위해 RestBubbleCount.Value 추가.
        _BubbleMapMaxY = _BottomBubbleY + RestBubbleCount.Value;
        _BubbleMap = new Bubble[11, _BubbleMapMaxY];

        _View.SetBubbleMapY(_BottomBubbleY, false, null);

    }

    private async UniTask MakeBubbles()
    {
        //이전에 삭제된 개수에서 -1을 하여 생성~

        int totalDelayMillisecond = 0;

        Bubble bubble;

        Config.BUBBLE_COLOR color;
        Config.BUBBLE_TYPE type;

        Vector3 startPos;

        List<Bubble> moveBubbles;

        foreach (var removeBubbleInfo in _PrevRemovedBubbleCounts)
        {
            int removeCount = removeBubbleInfo.Value;
            if (removeCount <= 1)
                continue;

            var lineInfo = _LineInfos[removeBubbleInfo.Key];

            //선 끝난 지점 찾아라앙~
            moveBubbles = new List<Bubble>();

            int endIdx = 0;

            foreach(var line in lineInfo)
            {
                bubble = _BubbleMap[line.X, line.Y];

                if (bubble == null)
                {
                    endIdx = line.Idx;
                    break;
                }
                else
                {
                    moveBubbles.Add(bubble);
                }
            }


            //일단 있는 버블 앞으로 당기기
            int moveBubbleCount = moveBubbles.Count;
            for (int i = 0; i < moveBubbleCount; ++i)
            {
                bubble = moveBubbles[^(i + 1)];

                var line = lineInfo[bubble.LineIndex];
                var targetLine = lineInfo[bubble.LineIndex + removeCount];

                _BubbleMap[bubble.X, bubble.Y] = null;
                _BubbleMap[targetLine.X, targetLine.Y] = bubble;

                bubble.LineIndex = targetLine.Idx;
                bubble.ChangeXY(targetLine.X, targetLine.Y);

                bubble.transform.DOLocalPath(GetLinePath(lineInfo, line.Idx, targetLine.Idx), 0.2f * (targetLine.Idx - line.Idx)).SetEase(Ease.Linear);
            }

            startPos = _View.GetBubblePos(lineInfo[0].X, lineInfo[0].Y);

            for (int i = 0; i < removeCount; ++i)
            {
                var targetLine = lineInfo[removeCount - i - 1];

                if(UnityEngine.Random.Range(0, 1f) < 0.4f)
                {
                    type = _BossBubbleTypes[UnityEngine.Random.Range(0, _BossBubbleTypeLength)];
                }
                else
                {
                    type = Config.BUBBLE_TYPE.NONE;
                }
                    
                color = type == Config.BUBBLE_TYPE.EXPLOSION ? Config.BUBBLE_COLOR.NONE : _BossBubbleColors[UnityEngine.Random.Range(0, _BossBubbleColorLength)];

                bubble = CreateBubble(targetLine.X, targetLine.Y, color, type);
                bubble.transform.localPosition = startPos;
                bubble.Line = removeBubbleInfo.Key;
                bubble.LineIndex = targetLine.Idx;

                bubble.transform.DOLocalPath(GetLinePath(lineInfo, lineInfo[0].Idx, targetLine.Idx), 0.2f * targetLine.Idx).SetDelay(i * 0.2f).SetEase(Ease.Linear);
            }

            if (totalDelayMillisecond < (removeCount * 0.2f)) totalDelayMillisecond = (removeCount * 200);
        }

        for(int i = _PrevRemovedBubbleCounts.Count - 1; i >= 0; --i)
        {
            if(_PrevRemovedBubbleCounts.ContainsKey(i)) 
            { 
                _PrevRemovedBubbleCounts[i] = 0;
            }
        }

        await UniTask.Delay(totalDelayMillisecond);
    }

    protected override void CheckFallBubbles()
    {
        HashSet<Bubble> topBubbles = new HashSet<Bubble>();

        foreach(var lineInfo in _LineInfos)
        {
            var line = lineInfo.Value[0];
            if (_BubbleMap[line.X, line.Y] != null)
            {
                topBubbles.Add(_BubbleMap[line.X, line.Y]);
            }
        }

        Vector3 holePos = _View.GetHolePos();

        if (topBubbles.Count == 0)
        {
            //맨 윗줄이 다 떨어졌으므로 전부 떨어뜨리기
            foreach (var bubble in _BubbleMap)
            {
                if (bubble != null)
                {
                    _BubbleMap[bubble.X, bubble.Y] = null;

                    _BubbleEventCallbacks.Add(bubble);

                    bubble.ForceFall(holePos, OnEndBubbleEvent);
                }
            }

            SetClearCondition();

            return;
        }

        HashSet<Bubble> linkedBubbles = new HashSet<Bubble>();
        HashSet<Bubble> newFallBubbles = new HashSet<Bubble>();


        foreach (var top in topBubbles)
        {
            FindLinkedBubbles(ref linkedBubbles, top);
        }

        foreach (var bubble in _BubbleMap)
        {
            if (bubble == null) continue;

            if (linkedBubbles.Contains(bubble) == false)
            {
                _BubbleMap[bubble.X, bubble.Y] = null;

                _BubbleEventCallbacks.Add(bubble);

                newFallBubbles.Add(bubble);
            }
        }

        foreach (var bubble in newFallBubbles)
        {
            bubble.Fall(holePos, OnEndBubbleEvent);
        }
    }

    protected override void OnEndBubbleEvent(Bubble bubble, bool dispose)
    {
        if (bubble.Line != -1)
        {
            _PrevRemovedBubbleCounts[bubble.Line]++;
        }

        base.OnEndBubbleEvent(bubble, dispose);
    }


    public void AttackToBoss()
    {
        RestTargetCount.Value -= 1;
    }


    private Vector3[] GetLinePath(List<BossStageBubbleData> lineInfo, int startIdx, int endIdx)
    {
        Vector3[] vector3s = new Vector3[endIdx - startIdx];

        int count = 0;
        for(int i = startIdx + 1; i <= endIdx; ++i)
        {
            var data = lineInfo[i];

            vector3s[count] = _View.GetBubblePos(data.X, data.Y);
            count++;
        }

        return vector3s;
    }


    public override void Dispose()
    {
        base.Dispose();

        _BossBubbleColors = null;
        _BossBubbleTypes = null;

        foreach(var line in _LineInfos)
        {
            line.Value.Clear();
        }
        _LineInfos.Clear();
        _LineInfos = null;

        _PrevRemovedBubbleCounts.Clear();
        _PrevRemovedBubbleCounts = null;
    }
}

public struct BossStageBubbleData
{
    public int X;
    public int Y;
    public int Line;
    public int Idx;
}
