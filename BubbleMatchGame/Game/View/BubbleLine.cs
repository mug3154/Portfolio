using System;
using System.Collections.Generic;
using UnityEngine;

public class BubbleLine : MonoBehaviour
{
    [SerializeField] LineRenderer _LineRenderer;

    Vector2 _StartPos = new Vector2(0, -2f);
    Vector3 _LinePos2;
    Vector3 _LinePos3;

    Action<Bubble, Vector2> _FindBubbleCallback;
    Action _CancelShootingLineCallback;


    public void Initialize(Action<Bubble, Vector2> findBubbleCallback, Action cancelShootingLineCallback)
    {
        _FindBubbleCallback = findBubbleCallback;
        _CancelShootingLineCallback = cancelShootingLineCallback;

        _LineRenderer.positionCount = 2;
        _LineRenderer.SetPosition(0, _StartPos);

        Hide();
    }

    public int GetLinePath(ref Vector3[] path)
    {
        return _LineRenderer.GetPositions(path);
    }

    public void Show(Vector2 worldPos)
    {
        float angle = Vector3.SignedAngle(new Vector3(1, 0), worldPos + new Vector2(0, 2), Vector3.forward);
        if (angle > -30 && angle < 20)
        {
            _CancelShootingLineCallback?.Invoke();
            return;
        }

        if (angle < -150 || angle > 150)
        {
            _CancelShootingLineCallback?.Invoke();
            return;
        }

        if (angle < 0)
            angle = 180 + angle;

        gameObject.SetActive(true);


        Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.forward);
        _LinePos2 = (quaternion * new Vector3(8, 0, 0));
        _LinePos2.y -= 2;

        Bubble bubble = FindClosestBubble(2, _StartPos, ref _LinePos2);
        if(bubble != null)
        {
            _FindBubbleCallback?.Invoke(bubble, _LinePos2);
            return;
        }

        _LinePos3 = _LinePos2;
            
        if(angle < 73 || angle > 108)
        {
            if (_LinePos2.x > 1.9f)
            {
                _LinePos2.x = 1.9f;
                _LinePos2.y = Mathf.Tan(angle * Mathf.Deg2Rad) * 1.9f;
            }
            else if (_LinePos2.x < -1.9f)
            {
                _LinePos2.x = -1.9f;
                _LinePos2.y = Mathf.Tan(angle * Mathf.Deg2Rad) * -1.9f;
            }
            _LinePos2.y -= 2;
            _LinePos3.x = _LinePos3.x * -1f;
        }
        else
        {
            if (_LinePos2.x > 0)
            {
                _LinePos2.y = 3.9f;
                _LinePos2.x = (1 / Mathf.Tan(angle * Mathf.Deg2Rad)) * 5.9f;
            }
            else
            {
                _LinePos2.y = 3.9f;
                _LinePos2.x = (1 / Mathf.Tan(angle * Mathf.Deg2Rad)) * 5.9f;
            }

            _LineRenderer.positionCount = 2;
            _LineRenderer.SetPosition(1, _LinePos2);
            return;
        }

        _LineRenderer.positionCount = 3;
        _LineRenderer.SetPosition(1, _LinePos2);

        bubble = FindClosestBubble(3, _LinePos2, ref _LinePos3);
        if(bubble != null)
        {
            _FindBubbleCallback?.Invoke(bubble, _LinePos3);
        }

        _LineRenderer.SetPosition(2, _LinePos3);
    }

    private Bubble FindClosestBubble(int lineCount, Vector3 startPos, ref Vector3 endPos)
    {
        Tuple<Bubble, Vector2> find = FindClosestBubble(startPos, endPos);
        if (find != null)
        {
            endPos = find.Item2;

            _LineRenderer.positionCount = lineCount;
            _LineRenderer.SetPosition(lineCount - 1, endPos);
            
            return find.Item1;
        }

        return null;
    }

    private Tuple<Bubble, Vector2> FindClosestBubble(Vector3 startPos, Vector3 endPos)
    {
        //기울기
        double m = (endPos.y - startPos.y) / (endPos.x - startPos.x);
        double b = startPos.y - m * startPos.x;

        Bubble bestBubble = null;
        Vector2 bestPos = Vector2.zero;
        float bestDistance = 1000000;
        float distance;

        foreach(var bubble in Game.Mode.BubbleMap)
        {
            if (bubble == null) continue;

            Vector2[] findList = FindIntersections(bubble.transform.position.x, bubble.transform.position.y, Config.BubbleSize, m, b);
            if(findList != null)
            {
                foreach (var pos in findList)
                {
                    distance = Vector2.Distance(startPos, pos);
                    if (distance < bestDistance)
                    {
                        bestPos = pos;

                        bestBubble = bubble;

                        bestDistance =distance;
                    }
                }
            }
        }

        if (bestBubble == null)
            return null;
        
        return Tuple.Create(bestBubble, bestPos);
    }

    public Vector2[] FindIntersections(double h, double k, double r, double m, double b)
    {
        // 원의 방정식 (x - h)^2 + (y - k)^2 = r^2
        // 직선의 방정식 y = mx + b

        // 직선의 방정식을 원의 방정식에 대입하여 이차방정식을 풉니다.
        // (x - h)^2 + (mx + b - k)^2 = r^2

        // 이차방정식 ax^2 + bx + c = 0의 계수 a, b, c를 계산합니다.
        double A = 1 + m * m;
        double B = 2 * (m * (b - k) - h);
        double C = h * h + (b - k) * (b - k) - r * r;

        // 판별식 계산
        double discriminant = B * B - 4 * A * C;

        // 교차점의 결과를 저장할 리스트
        var intersections = new List<Vector2>();

        if (discriminant < 0)
        {
            // 교차점이 없음
            return null;
        }
        else if (discriminant == 0)
        {
            // 한 개의 교차점
            double x = -B / (2 * A);
            double y = m * x + b;
            intersections.Add(new Vector2((float)x, (float)y));
        }
        else
        {
            // 두 개의 교차점
            double sqrtDiscriminant = Math.Sqrt(discriminant);
            double x1 = (-B + sqrtDiscriminant) / (2 * A);
            double y1 = m * x1 + b;
            intersections.Add(new Vector2((float)x1, (float)y1));

            double x2 = (-B - sqrtDiscriminant) / (2 * A);
            double y2 = m * x2 + b;
            intersections.Add(new Vector2((float)x2, (float)y2));
        }

        return intersections.ToArray();
    }



    public void Hide()
    {
        gameObject.SetActive(false);
    }

}
