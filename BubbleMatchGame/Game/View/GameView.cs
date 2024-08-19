using DG.Tweening;
using System;
using UnityEngine;

public class GameView : MonoBehaviour
{
    [SerializeField] Transform _BubbleContainer;
    public Transform BubbleContainer => _BubbleContainer;

    [SerializeField] Transform _EffectContainer;
    public Transform EffectContainer => _EffectContainer;

    [SerializeField] Transform _Hole;
    [SerializeField] BubbleLine _Line;
    public BubbleLine Line => _Line;

    [SerializeField] Transform _GuideBubble;

    public SpriteRenderer _Boss;

    public void Initialize(Action<Bubble, Vector2> findBubbleCallback, Action cancelShootingLineCallback)
    {
        _Line.Initialize(findBubbleCallback, cancelShootingLineCallback);

        _Boss.gameObject.SetActive(false);

        HideGuideBubble();
    }

    public void ShowGuideBubble(int x, int y)
    {
        _GuideBubble.transform.localPosition = GetBubblePos(x, y);
        _GuideBubble.gameObject.SetActive(true);
    }

    public void HideGuideBubble()
    {
        _GuideBubble.gameObject.SetActive(false);
    }

    public bool IsShowGuideBubble()
    {
        return _GuideBubble.gameObject.activeSelf;
    }



    public void SetBubble(Bubble bubble, int x, int y)
    {
        bubble.transform.SetParent(_BubbleContainer);
        bubble.transform.localScale = Vector3.one;
        bubble.transform.localPosition = GetBubblePos(x, y);
        bubble.gameObject.SetActive(true);
    }

    public void ShootBubble(Bubble bubble)
    {
        bubble.transform.SetParent(_BubbleContainer);
        bubble.transform.localScale = Vector3.one;
        bubble.transform.position = new Vector3(0, -2f, 0);
        bubble.gameObject.SetActive(true);
    }

    public Vector3 GetBubblePos(int x, int y)
    {
        float posX = (x - 5) * Config.BubbleSize;

        if (y % 2 == 1)
            posX += Config.BubbleSize * 0.5f;

        return new Vector3(posX, -0.175f + (y * -0.35f));
    }

    public Vector3 GetBubbleWorldPos(int x, int y)
    {
        float posX = (x - 5) * Config.BubbleSize;

        if (y % 2 == 1)
            posX += Config.BubbleSize * 0.5f;

        Vector3 pos = new Vector3(posX, -0.175f + (y * -0.35f));

        return _BubbleContainer.TransformPoint(pos);
    }

    public void SetBubbleMapY(int currMaxY, bool playAnimation, Action endCallback)
    {
        float bubbleH = currMaxY * Config.BubbleSize;

        Vector3 targetPos;

        if (bubbleH <= 4)
        {
            targetPos = new Vector3(0, 4, 0);
        }
        else
        {
            targetPos = new Vector3(0, bubbleH, 0);
        }

        if (playAnimation)
        {
            if (_BubbleContainer.transform.localPosition != targetPos)
            {
                _BubbleContainer.transform.DOLocalMove(targetPos, 0.3f).OnComplete(() => endCallback?.Invoke());
            }
            else
            {
                endCallback?.Invoke();
            }
        }
        else
        {
            _BubbleContainer.transform.localPosition = targetPos;

            endCallback?.Invoke();
        }
    }


    public void SetBoss(int idx, int x, int y)
    {
        _Boss.sprite = Root.Instance.ResManager.GetSprite($"Boss_{idx}");
        _Boss.transform.localPosition = GetBubblePos(x, y);
        _Boss.gameObject.SetActive(true);
    }

    public Vector2 GetBossPos()
    {
        return _Boss.transform.localPosition;
    }



    public Vector3 GetHolePos()
    {
        return _Hole.transform.position;
    }
}
