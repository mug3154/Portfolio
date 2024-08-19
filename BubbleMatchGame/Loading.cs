using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : MonoBehaviour
{
    [SerializeField] RectTransform _ProgressBar;

    float _OriginProgressBarWitdh = 0;

    int _ShowCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        _OriginProgressBarWitdh = _ProgressBar.sizeDelta.x;
    }

    public void Show()
    {
        _ShowCount++;

        gameObject.SetActive(true);
    }

    public void Hide(bool force = false)
    {
        --_ShowCount;

        if(force || _ShowCount == 0)
        {
            _ShowCount = 0;

            gameObject.SetActive(false);
        }
    }

    public void SetProgress(float progress)
    {
        _ProgressBar.sizeDelta = new Vector2(_OriginProgressBarWitdh * progress, _ProgressBar.sizeDelta.y);
    }
}
