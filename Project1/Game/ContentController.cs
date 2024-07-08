using System;
using UnityEngine;

public class ContentController : MonoBehaviour
{
    public Action OnGroundLoaded => _onGroundLoaded;

    protected Action _onGroundLoaded = null;

    public virtual void EnterContent()
    {
        if (gameObject.activeInHierarchy)
            return;

        gameObject.SetActive(true);
    }

    public virtual void ExitContent()
    {
        if (gameObject.activeInHierarchy == false)
            return;

        gameObject.SetActive(false);
    }

    public void SetGroundLoaded(Action onGroundLoaded)
    {
        _onGroundLoaded = onGroundLoaded;
    }
}