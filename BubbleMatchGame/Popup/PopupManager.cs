using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    public enum POPUP : ushort
    { 
        GAME_SUCCESS = 0,
        GAME_FAILURE
    }

    [SerializeField] PopupGameSuccess _PopupGameSuccess;
    [SerializeField] PopupGameFailure _PopupGameFailure;


    private void Start()
    {
        _PopupGameSuccess.gameObject.SetActive(false);
        _PopupGameFailure.gameObject.SetActive(false);
    }


    public PopupBase GetPopup(POPUP popup)
    {
        PopupBase findPopup = null;

        switch(popup)
        {
            case POPUP.GAME_SUCCESS: findPopup = _PopupGameSuccess; break;
            case POPUP.GAME_FAILURE: findPopup = _PopupGameFailure; break;
            default: break;
        }

        return findPopup;
    }

    public void ShowPopup(POPUP popup, Action<PopupBase> onShowCallback, Action onHideCallback) 
    {
        PopupBase popupBase = GetPopup(popup);

        if(popupBase != null)
        {
            popupBase.Show(onShowCallback, onHideCallback);
        }
    }
}
