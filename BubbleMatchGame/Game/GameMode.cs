using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using static Config;

public abstract class GameMode : MonoBehaviour
{
    protected GameView _View;
    public GameView View=>_View;
    protected GameUI _UI;

    public int MapIdx;
    public int StageIdx;

    public Config.GAME_TYPE GAME_TYPE { get; protected set; }
    public ReactiveProperty<int> RestTargetCount { get; protected set; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> RestBubbleCount { get; protected set; } = new ReactiveProperty<int>(0);
    
    protected int _UsableBubbleColorLength = 0;
    protected Config.BUBBLE_COLOR[] _UsableBubbleColors;

    public ReactiveCollection<int> ShootReadyBubbles { get; protected set; } = new ReactiveCollection<int>();

    public bool IsNeroMode = false;
    protected int _ChargeNeroNeedBubbleCount;
    protected const int _NeroNeedBubbleCount = 30;
    public Action<int, int> OnChangeNeroNeedBubbleCount;
    protected bool _IsNeroOn;


    protected Bubble[,] _BubbleMap;
    public Bubble[,] BubbleMap => _BubbleMap;

    protected int _BubbleMapMaxY = 0;
    
    
    protected int _BottomBubbleY = 0;
    public int BottomBubbleY=>_BottomBubbleY;

    protected int _GuideBubbleX;
    protected int _GuideBubbleY;


    protected Queue<Bubble> _CheckReadyBubbles = new Queue<Bubble>();
    protected HashSet<Bubble> _BubbleEventCallbacks = new HashSet<Bubble>();


    protected Bubble[] _TempBubbles = new Bubble[50];

    protected bool _IsChangeBottomY = false;



    public virtual void Initialize(GameView gameView, GameUI gameUI, int mapIdx, int stage, string[] data)
    {
        MapIdx = mapIdx;
        StageIdx = stage;

        SetGameData(data);

        GameValueInitialize();

        _View = gameView;
        _View.Initialize(ShowGuideBubble, CancelBubbleLine);

        _UI = gameUI;
        _UI.Initialize(Config.GAME_MODE.NORMAL, MoveBubbleLine, CancelBubbleLine);

        SetGameFlowEventReceivers();

        CreateBubbleMap(data);

        SetClearCondition();

        CheckingReadyBubbles();
    }

    public virtual void SetGameData(string[] info)
    {
        //info[1] = GAME_CLEAR_TYPE 버블 없애기, 부엉이 없애기 등등, 별1,별2,별3 점수
        //info[2] = 보조컨트롤러유무01,버블개수13,유저사용버블색상1,2,3

        string[] Info1 = info[1].Split(",");

        IsNeroMode = Info1[0] == "1";

        string[] Info2 = info[2].Split(",");
        RestBubbleCount.Value = int.Parse(Info2[1]);

        int rowLength = Info2.Length;
        _UsableBubbleColors = new Config.BUBBLE_COLOR[rowLength - 2];

        for (int i = 2; i < rowLength; i++)
        {
            _UsableBubbleColors[i - 2] = (Config.BUBBLE_COLOR)int.Parse(Info2[i]);
        }
        _UsableBubbleColorLength = _UsableBubbleColors.Length;
    }

    protected void GameValueInitialize()
    {
        _IsNeroOn = false;
        _ChargeNeroNeedBubbleCount = 0;

        ShootReadyBubbles.Clear();
        ShootReadyBubbles.Add((int)_UsableBubbleColors[UnityEngine.Random.Range(0, _UsableBubbleColorLength)]);
        ShootReadyBubbles.Add((int)_UsableBubbleColors[UnityEngine.Random.Range(0, _UsableBubbleColorLength)]);
    }

    protected abstract void SetClearCondition();

    public List<Bubble> GetShowBubbles()
    {
        List<Bubble> result = new List<Bubble>();

        int top = BottomBubbleY - 10;
        if (top < 0) top = 0;

        for (int y = _BottomBubbleY; y >= top; --y)
        {
            for(int x = 0; x < 11; ++x)
            {
                if (_BubbleMap[x, y] != null)
                {
                    result.Add(BubbleMap[x, y]);
                }
            }
        }

        return result;
    }


    protected void SetGameFlowEventReceivers()
    {
        GameFlowEventManager.Instance.AddReceiver(new GameFlowEventReceiver().SetEvent(GameFlowEventManager.GAME_FLOW_EVENT.REQUEST_TO_SHOOT).SetCallback(RequstShoot));

        GameFlowEventManager.Instance.AddReceiver(new GameFlowEventReceiver().SetEvent(GameFlowEventManager.GAME_FLOW_EVENT.CHANGE_BUBBLE_ORDER).SetCallback(ChangeBubbleOrder));

        GameFlowEventManager.Instance.AddReceiver(new GameFlowEventReceiver().SetEvent(GameFlowEventManager.GAME_FLOW_EVENT.CHECK_CLEAR_CONDITION).SetCallback(CheckGameSuccess));

        GameFlowEventManager.Instance.AddReceiver(new GameFlowEventReceiver().SetEvent(GameFlowEventManager.GAME_FLOW_EVENT.GAME_CLEAR).SetCallback(GameSuccess));

        GameFlowEventManager.Instance.AddReceiver(new GameFlowEventReceiver().SetEvent(GameFlowEventManager.GAME_FLOW_EVENT.GAME_OVER).SetCallback(GameFailure));
    }

    protected virtual void GameFailure()
    {
        PopupManager.Instance.ShowPopup(PopupManager.POPUP.GAME_FAILURE, (popup) =>
        {
        }, () =>
        {
            Game.Instance.Hide();
            Home.Instance.Show();
        });
    }

    protected virtual void GameSuccess()
    {
        PopupManager.Instance.ShowPopup(PopupManager.POPUP.GAME_SUCCESS, (popup) =>
        {
        }, () =>
        {
            Game.Instance.Hide();
            Home.Instance.Show();
        });
    }

    protected void CheckGameSuccess()
    {
        if (RestTargetCount.Value == 0)
        {
            GameFlowEventManager.Instance.Notify(GameFlowEventManager.GAME_FLOW_EVENT.GAME_CLEAR);
        }
        else if (RestBubbleCount.Value == 0)
        {
            GameFlowEventManager.Instance.Notify(GameFlowEventManager.GAME_FLOW_EVENT.GAME_OVER);
        }
        else
        {
            TryNeroOn();

            GameFlowEventManager.Instance.Notify(GameFlowEventManager.GAME_FLOW_EVENT.CONTROLL_POSSIBLE_PLAYER);
        }
    }

    protected void ChangeBubbleOrder()
    {
        List<int> bubbles = ShootReadyBubbles.ToList();
        int size = bubbles.Count;

        for (int i = 0; i < size; i++)
        {
            if(i == size - 1)
            {
                ShootReadyBubbles[i] = bubbles[0];
            }
            else
            {
                ShootReadyBubbles[i] = bubbles[i + 1];
            }
        }
    }

    public void SetNextBubbles()
    {
        Config.BUBBLE_COLOR bubbleColor = (Config.BUBBLE_COLOR)ShootReadyBubbles[0];
        if (bubbleColor == Config.BUBBLE_COLOR.NERO)
        {
            _IsNeroOn = false;

            ShootReadyBubbles.RemoveAt(0);

            GameFlowEventManager.Instance.Notify(GameFlowEventManager.GAME_FLOW_EVENT.NERO_OFF);
        }
        else
        {
            RestBubbleCount.Value -= 1;
        
            if(RestBubbleCount.Value > 1)
            {
                ShootReadyBubbles[0] = ((int)_UsableBubbleColors[UnityEngine.Random.Range(0, _UsableBubbleColorLength)]);
            }
        }
    }

    protected void TryNeroOn()
    {
        if (_ChargeNeroNeedBubbleCount == _NeroNeedBubbleCount)
        {
            _ChargeNeroNeedBubbleCount = 0;
            ShootReadyBubbles.Insert(0, 100);

            _IsNeroOn = true;

            GameFlowEventManager.Instance.Notify(GameFlowEventManager.GAME_FLOW_EVENT.NERO_ON);
        }
    }


    #region Bubble Map
    public virtual void CreateBubbleMap(string[] data)
    {
        int infoLineCount = 3; //위에서부터 모드, 게임 타입, 기타 정보까지 3줄 삭제

        string[] bubbleLineData = new string[data.Length - infoLineCount];

        for (int i = infoLineCount; i < data.Length; i++)
        {
            bubbleLineData[i - infoLineCount] = data[i];
        }

        //사용할 수 있는 구슬 수 만큼 일렬로 배치하는 것을 방지하기 위해 RestBubbleCount.Value 추가.
        _BubbleMapMaxY = bubbleLineData.Length + RestBubbleCount.Value;
        _BubbleMap = new Bubble[11, _BubbleMapMaxY];

        string[] bubbleRowData;
        string[] bubbleData;


        _BottomBubbleY = bubbleLineData.Length;

        int columnCount;

        for (int row = 0; row < _BottomBubbleY; row++)
        {
            bubbleRowData = bubbleLineData[row].Split("\t"); //(x좌표,컬러,타입)

            columnCount = bubbleRowData.Length;

            for (int column = 0; column < columnCount; column++)
            {
                if (bubbleRowData[column]  == "") continue;

                bubbleData = bubbleRowData[column].Split(","); //x,color,type

                int x = int.Parse(bubbleData[0]) - 1;

                CreateBubble(x, row, (Config.BUBBLE_COLOR)int.Parse(bubbleData[1]), (Config.BUBBLE_TYPE)int.Parse(bubbleData[2]));
            }
        }

        _View.SetBubbleMapY(_BottomBubbleY, false, null);
    }

    public Bubble CreateBubble(int x, int y, Config.BUBBLE_COLOR color, Config.BUBBLE_TYPE type)
    {
        Bubble bubble = Root.Instance.ResManager.GetBubble(type);
        bubble.SetData(x, y, color);
        _View.SetBubble(bubble, x, y);
        _BubbleMap[x, y] = bubble;
        return bubble;
    }


    public void SetMatchCountOfBubble(int x, int y, int matchCount)
    {
        Bubble bubble = _BubbleMap[x, y];
        if (bubble == null) return;

        bubble.SetMatchCount(matchCount);

        _CheckReadyBubbles.Enqueue(bubble);
    }

    public void ShowGuideBubble(Bubble bubble, Vector2 intersectionPos)
    {
        _GuideBubbleX = bubble.X;
        _GuideBubbleY = bubble.Y;

        if (intersectionPos.y - bubble.transform.position.y > -0.08f) //위는 도달 할 수 없으므로 오른쪽 왼쪽 처리
        {
            if (bubble.transform.position.x < intersectionPos.x) //오른쪽
                _GuideBubbleX += 1;
            else
                _GuideBubbleX -= 1;
        }
        else
        {
            ++_GuideBubbleY;

            if (bubble.transform.position.x < intersectionPos.x) //오른쪽
            {
                if (bubble.Y % 2 == 1)
                    _GuideBubbleX += 1;
            }
            else
            {
                if (bubble.Y % 2 == 0)
                    _GuideBubbleX -= 1;
            }
        }

        if (_GuideBubbleX < 0) return;
        if (_GuideBubbleX > 10) return;

        if (_GuideBubbleY < 0) return;
        if (_GuideBubbleY >= _BubbleMapMaxY) return;

        if (_BubbleMap[_GuideBubbleX, _GuideBubbleY] != null)
            return;

        _View.ShowGuideBubble(_GuideBubbleX, _GuideBubbleY);
    }

    public virtual void RequstShoot()
    {
        _View.Line.Hide();

        if (_View.IsShowGuideBubble() == false)
        {
            GameFlowEventManager.Instance.Notify(GameFlowEventManager.GAME_FLOW_EVENT.CONTROLL_POSSIBLE_PLAYER);

            return;
        }

        _View.HideGuideBubble();

        _CheckReadyBubbles.Clear();
        _BubbleEventCallbacks.Clear();

        Shoot();
    }


    protected virtual void Shoot()
    {
        Config.BUBBLE_COLOR bubbleColor = (Config.BUBBLE_COLOR)ShootReadyBubbles[0];

        Bubble bubble = Root.Instance.ResManager.GetBubble(bubbleColor == BUBBLE_COLOR.NERO ? BUBBLE_TYPE.NERO : BUBBLE_TYPE.NONE);
        bubble.SetData(_GuideBubbleX, _GuideBubbleY, bubbleColor);

        _View.ShootBubble(bubble);

        _BubbleMap[_GuideBubbleX, _GuideBubbleY] = bubble;

        _CheckReadyBubbles.Enqueue(bubble);

        List<Bubble> aroundList = new List<Bubble>();
        Queue<Bubble> checkedList = new Queue<Bubble>();
        HashSet<Bubble> completeList = new HashSet<Bubble>();

        //내 주변 먼저 찾기
        FindAroundBubble(ref aroundList, ref checkedList, ref completeList, bubble, ref _BubbleMap, _BubbleMapMaxY);

        foreach(var aroundBubble in aroundList)
        {
            if(aroundBubble.HaveSideEffectOfShootBubble())
            {
                _CheckReadyBubbles.Enqueue(aroundBubble);
            }
        }

        SetNextBubbles();

        Vector3[] paths = new Vector3[3];
        int pathCount = _View.Line.GetLinePath(ref paths);
        paths[pathCount - 1] = _View.GetBubbleWorldPos(_GuideBubbleX, _GuideBubbleY);

        Vector3[] newPath = new Vector3[pathCount];
        Array.Copy(paths, newPath, pathCount);
    
        
        _IsChangeBottomY = true;

        bubble.transform.DOPath(newPath, 0.3f).OnComplete(CheckingReadyBubbles);
    }


    protected virtual void CheckingReadyBubbles()
    {
        if (_IsChangeBottomY)
        {
            CheckBubbleBottomY();
        }
        else if(_BubbleEventCallbacks.Count != 0)
        {

        }
        else if (_CheckReadyBubbles.Count == 0)
        {
            GameFlowEventManager.Instance.Notify(GameFlowEventManager.GAME_FLOW_EVENT.CHECK_CLEAR_CONDITION);
        }
        else
        {
            BubblesPop();
        }
    }

    protected virtual void BubblesPop()
    {
        Pop();

        SetClearCondition();

        CheckingReadyBubbles();
    }

    protected int Pop()
    {
        int pop = 0;

        while (_CheckReadyBubbles.Count != 0)
        {
            var bubble = _CheckReadyBubbles.Dequeue();

            //같이 터질 버블들 가져오기.
            int popCount = bubble.GetPopBubbles(ref _TempBubbles);
            if (popCount > 0)
            {
                //특수 이벤트 발생 시 미리 검색에 걸리지 않도록 삭제
                for (int i = 0; i < popCount; ++i)
                {
                    var popBubble = _TempBubbles[i];
                    _BubbleMap[popBubble.X, popBubble.Y] = null;

                    _BubbleEventCallbacks.Add(popBubble);
                }

                for (int i = 0; i < popCount; ++i)
                {
                    var popBubble = _TempBubbles[i];
                    if (popBubble.Pop(0.05f * i, OnEndBubbleEvent))
                    {
                        ++pop;

                        CountingNeroGuage();
                    }
                }

                //줄 끊어진 버블 삭제
                CheckFallBubbles();
            }
        }

        if(pop > 0) _IsChangeBottomY = true;

        return pop;
    }

    protected void CountingNeroGuage()
    {
        if (IsNeroMode == false) return;
        if (_IsNeroOn) return;

        ++_ChargeNeroNeedBubbleCount;
        if (_ChargeNeroNeedBubbleCount > _NeroNeedBubbleCount) _ChargeNeroNeedBubbleCount = _NeroNeedBubbleCount;

        OnChangeNeroNeedBubbleCount?.Invoke(_ChargeNeroNeedBubbleCount, _NeroNeedBubbleCount);
    }

    protected void CheckBubbleBottomY()
    {
        bool changed = false;

        int h = _BubbleMap.GetLength(1) - 1;

        for (int y = h; y > -1; --y)
        {
            for (int x = 0; x < 11; ++x)
            {
                if (_BubbleMap[x, y] != null)
                {
                    changed = true;
                    _BottomBubbleY = y + 1;

                    break;
                }
            }

            if (changed)
                break;
        }

        MoveBubbleMapY();
    }

    protected void MoveBubbleMapY()
    {
        if (_IsChangeBottomY)
        {
            _IsChangeBottomY = false;

            _View.SetBubbleMapY(_BottomBubbleY, true, CheckingReadyBubbles);
        }
        else
        {
            CheckingReadyBubbles();
        }
    }

    protected virtual void CheckFallBubbles()
    {
        HashSet<Bubble> topBubbles = new HashSet<Bubble>();

        for (int x = 0; x < 11; ++x)
        {
            if (_BubbleMap[x, 0] != null)
            {
                topBubbles.Add(_BubbleMap[x, 0]);
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

            if(linkedBubbles.Contains(bubble) == false)
            {
                _BubbleMap[bubble.X, bubble.Y] = null;

                _BubbleEventCallbacks.Add(bubble);

                newFallBubbles.Add(bubble);
            }
        }

        foreach(var bubble in newFallBubbles)
        {
            bubble.Fall(holePos, OnEndBubbleEvent);
        }
    }

    protected void FindLinkedBubbles(ref HashSet<Bubble> linkedBubbles, Bubble bubble)
    {
        List<Bubble> aroundList = new List<Bubble>();
        Queue<Bubble> checkedList = new Queue<Bubble>();

        if(bubble.Y == 0)
        {
            linkedBubbles.Add(bubble);
        }

        //내 주변 먼저 찾기
        FindAroundBubble(ref aroundList, ref checkedList, ref linkedBubbles, bubble, ref _BubbleMap, _BubbleMapMaxY);

        if (aroundList.Count != 0)
        {
            foreach (Bubble item in aroundList)
            {
                checkedList.Enqueue(item);
            }

            while (checkedList.Count != 0)
            {
                var checkBubble = checkedList.Dequeue();
                linkedBubbles.Add(checkBubble);

                FindAroundBubble(ref aroundList, ref checkedList, ref linkedBubbles, checkBubble, ref _BubbleMap, _BubbleMapMaxY);
                if (aroundList.Count != 0)
                {
                    foreach (Bubble item in aroundList)
                    {
                        checkedList.Enqueue(item);
                    }
                }
            }
        }
    }

    protected virtual void OnEndBubbleEvent(Bubble bubble, bool dispose)
    {
        if (dispose)
            Root.Instance.ResManager.ReleaseBubble(bubble);

        _BubbleEventCallbacks.Remove(bubble);
        if(_BubbleEventCallbacks.Count == 0)
        {
            CheckingReadyBubbles();
        }
    }

    protected void ClearBubbles()
    {
        foreach (var bubble in _BubbleMap)
        {
            if (bubble != null)
            {
                Root.Instance.ResManager.ReleaseBubble(bubble);
            }
        }
        _BubbleMap = null;
    }

    #endregion Bubble Map


    #region Bubble line
    protected void CancelBubbleLine()
    {
        _View.Line.Hide();
        _View.HideGuideBubble();
    }

    protected void MoveBubbleLine(Vector2 worldPos)
    {
        _View.HideGuideBubble();

        _View.Line.Show(worldPos);
    }

    #endregion Bubble line




    #region Utils
    public static void GetEmtpyCellOfAround(ref HashSet<Vector2Int> result, int targetX, int targetY, ref Bubble[,] bubbleMap, int bubbleMaxY)
    {
        result.Clear();

        if (targetY % 2 == 0)
        {
            GetEmptyBubblePos(ref result, targetX - 1, targetY - 1, ref bubbleMap, bubbleMaxY); //왼쪽 위
            GetEmptyBubblePos(ref result, targetX, targetY - 1, ref bubbleMap, bubbleMaxY);     //오른쪽 위
            GetEmptyBubblePos(ref result, targetX - 1, targetY, ref bubbleMap, bubbleMaxY);     //왼쪽
            GetEmptyBubblePos(ref result, targetX + 1, targetY, ref bubbleMap, bubbleMaxY);     //오른쪽
            GetEmptyBubblePos(ref result, targetX - 1, targetY + 1, ref bubbleMap, bubbleMaxY); //왼쪽 아래
            GetEmptyBubblePos(ref result, targetX, targetY + 1, ref bubbleMap, bubbleMaxY); //오른쪽 아래
        }
        else
        {
            GetEmptyBubblePos(ref result, targetX, targetY - 1, ref bubbleMap, bubbleMaxY); //왼쪽 위
            GetEmptyBubblePos(ref result, targetX + 1, targetY - 1, ref bubbleMap, bubbleMaxY);     //오른쪽 위
            GetEmptyBubblePos(ref result, targetX - 1, targetY, ref bubbleMap, bubbleMaxY);     //왼쪽
            GetEmptyBubblePos(ref result, targetX + 1, targetY, ref bubbleMap, bubbleMaxY);     //오른쪽
            GetEmptyBubblePos(ref result, targetX, targetY + 1, ref bubbleMap, bubbleMaxY); //왼쪽 아래
            GetEmptyBubblePos(ref result, targetX + 1, targetY + 1, ref bubbleMap, bubbleMaxY); //오른쪽 아래
        }
    }

    public static void GetEmptyBubblePos(ref HashSet<Vector2Int> result, int x, int y, ref Bubble[,] bubbleMap, int bubbleMaxY)
    {
        if (x < 0) return;
        if (x > 10) return;

        if (y < 0) return;
        if (y >= bubbleMaxY) return;

        if(bubbleMap[x, y] == null)
            result.Add(new Vector2Int(x, y));
    }


    public static Bubble GetBubble(int x, int y, ref Bubble[,] bubbleMap, int bubbleMaxY)
    {
        if (x < 0) return null;
        if (x > 10) return null;

        if (y < 0) return null;
        if (y >= bubbleMaxY) return null;
        
        return bubbleMap[x, y];
    }

    public static void FindAroundBubble(ref List<Bubble> result, ref Queue<Bubble> checkedList, ref HashSet<Bubble> completeList, Bubble i, ref Bubble[,] bubbleMap, int maxY)
    {
        if (i.Y % 2 == 0)
        {
            GetSameColorAroundBubble_Y0(ref result, ref checkedList, ref completeList, i, ref bubbleMap, maxY);
        }
        else
        {
            GetSameColorAroundBubble_Y1(ref result, ref checkedList, ref completeList, i, ref bubbleMap, maxY);
        }
    }

    protected static void GetSameColorAroundBubble_Y0(ref List<Bubble> result, ref Queue<Bubble> checkedList, ref HashSet<Bubble> completeList, Bubble i, ref Bubble[,] bubbleMap, int maxY)
    {
        result.Clear();

        GetBubble(ref result, ref checkedList, ref completeList, i.X - 1, i.Y - 1, i.Color, ref bubbleMap, maxY); //왼쪽 위
        GetBubble(ref result, ref checkedList, ref completeList, i.X, i.Y - 1, i.Color, ref bubbleMap, maxY); //오른쪽 위
        GetBubble(ref result, ref checkedList, ref completeList, i.X - 1, i.Y, i.Color, ref bubbleMap, maxY); //왼쪽
        GetBubble(ref result, ref checkedList, ref completeList, i.X + 1, i.Y, i.Color, ref bubbleMap, maxY); //오른쪽
        GetBubble(ref result, ref checkedList, ref completeList, i.X - 1, i.Y + 1, i.Color, ref bubbleMap, maxY); //왼쪽 아래
        GetBubble(ref result, ref checkedList, ref completeList, i.X, i.Y + 1, i.Color, ref bubbleMap, maxY); //오른쪽 아래
    }

    protected static void GetSameColorAroundBubble_Y1(ref List<Bubble> result, ref Queue<Bubble> checkedList, ref HashSet<Bubble> completeList, Bubble i, ref Bubble[,] bubbleMap, int maxY)
    {
        result.Clear();

        GetBubble(ref result, ref checkedList, ref completeList, i.X, i.Y - 1, i.Color, ref bubbleMap, maxY); //왼쪽 위
        GetBubble(ref result, ref checkedList, ref completeList, i.X + 1, i.Y - 1, i.Color, ref bubbleMap, maxY); //오른쪽 위
        GetBubble(ref result, ref checkedList, ref completeList, i.X - 1, i.Y, i.Color, ref bubbleMap, maxY); //왼쪽
        GetBubble(ref result, ref checkedList, ref completeList, i.X + 1, i.Y, i.Color, ref bubbleMap, maxY); //오른쪽
        GetBubble(ref result, ref checkedList, ref completeList, i.X, i.Y + 1, i.Color, ref bubbleMap, maxY); //왼쪽 아래
        GetBubble(ref result, ref checkedList, ref completeList, i.X + 1, i.Y + 1, i.Color, ref bubbleMap, maxY); //오른쪽 아래
    }

    protected static void GetBubble(ref List<Bubble> list, ref Queue<Bubble> checkedList, ref HashSet<Bubble> completeList, int x, int y, Config.BUBBLE_COLOR color, ref Bubble[,] bubbleMap, int bubbleMaxY)
    {
        Bubble bubble = GameMode.GetBubble(x, y, ref bubbleMap, bubbleMaxY);

        if (bubble == null) return;

        if (checkedList.Contains(bubble)) return;
        if (completeList.Contains(bubble)) return;

        list.Add(bubble);
    }
    #endregion Utils



    public virtual void Dispose()
    {
        ClearBubbles();

        RestTargetCount?.Dispose();
        RestTargetCount = null;

        RestBubbleCount?.Dispose();
        RestBubbleCount = null;

        _UsableBubbleColors = null;

        ShootReadyBubbles?.Dispose();
        ShootReadyBubbles = null;

        OnChangeNeroNeedBubbleCount = null;

        _BubbleMap = null;

        _CheckReadyBubbles = null;
        _BubbleEventCallbacks = null;

        _TempBubbles = null;
    }

}
