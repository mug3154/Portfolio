using DG.Tweening;
using UnityEngine;

public class CustomTweenUIMoveY : CustomTween
{
    public Vector3 startPosition;
    public Vector3 targetPosition;
    [SerializeField] RectTransform _target = null;
    protected RectTransform targetTransform
    {
        get
        {
            if (_target == null)
                _target = GetComponent<RectTransform>();

            return _target;
        }
    }
    protected override void StartTween()
    {
        targetTransform.anchoredPosition = new Vector2(targetTransform.anchoredPosition.x, startPosition.y);

        _tween = targetTransform.DOAnchorPosY(targetPosition.y, duration, false);
        _tween.SetUpdate(IsTimeUnscaledMode);
        _tween.SetEase(ease);
        _tween.onComplete = OnEndTween;
    }

    public override void SetTargetState()
    {
        targetTransform.anchoredPosition = new Vector2(targetTransform.anchoredPosition.x, targetPosition.y);
    }

    public override bool IsComplete()
    {
        return targetTransform.anchoredPosition.y == targetPosition.y;
    }
}
