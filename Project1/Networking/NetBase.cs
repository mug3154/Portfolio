using System;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections.Generic;


/************************
 * 게임 오브젝트의 클래스에서 직접 PacketManager를 통해서 통신 할 경우에는 다음과 같은 문제가 발생한다.
 * - 게임 오브젝트 Destroy후 통신 완료 시 OnNetworkingSuccess 콜백에서 자식 객체등에 접근 할 때 크래시가 발생
 * 
 * NetBase을 게임오브젝트 클래스에서 생성 후 통신하는 도중에 게임 오브젝트가 Destroy되고 통신이 완료되면 OnSuccess의 
 * if (onCallbackForObj.Target.Equals(null)) 값이 만족되어 return되기 때문에 크래시가 발생하지 않는다.
 * 
 ************************/
public abstract class NetBase
{
    public NetBase() { }

    public PacketManager.OnNetworkingSuccess onCallbackForObj = null;

    protected bool isPost = false;
    protected string url = "";
    protected string query = "";

    protected int retryCount = 100;

    List<IMultipartFormSection> _postData;
    byte[] _byteData;
    List<string> _query = new List<string>();


    public virtual void Start() { }


    protected void StartDirectPostNetworking(string url, byte[] data)
    {
        _byteData = data;

        isPost = true;
        this.url = url;

        PacketManager.Instance.StartDirectPostNetworking(url, _byteData);
    }


    protected void StartPostNetworking(string url, byte[] data, PacketManager.OnNetworkingSuccess onCallbackForObj)
    {
        ShowLoading();

        _byteData = data;

        isPost = true;
        this.url = url;
        this.onCallbackForObj = onCallbackForObj;

        PacketManager.Instance.StartPostNetworking(this, url, _byteData, OnSuccess, Retry, false);
    }


    protected void StartGetNetworking(string url, List<string> query, PacketManager.OnNetworkingSuccess onCallbackForObj)
    {
        ShowLoading();
        
        _query = query;

        isPost = false;
        this.url = url;
        this.onCallbackForObj = onCallbackForObj;

        PacketManager.Instance.StartGetNetworking(this, url, query, OnSuccess, Retry, false);
    }

    protected virtual void ShowLoading()
    {
    }
    protected virtual void HideLoading()
    {
    }


    protected void OnSuccess(byte[] data)
    {
        HideLoading();
            
        Debug.Log($"[NETWORK SUCCESS] url:{url}");
            
        try
        {
            if (onCallbackForObj.Equals(null))
            {
                return;
            }

            if (onCallbackForObj.Target.Equals(null))
            {
                return;
            }

            onCallbackForObj?.Invoke(data);
 
        }
        catch(Exception e)
        {
            string error = StackTraceUtility.ExtractStringFromException(e);

            Debug.LogError("ClassName:" + this.GetType().Name + ", Error:" + error);
        }
    }

    public void Retry(long responseCode)
    {
        if (retryCount > 0)
        {
            retryCount--;

            if(isPost)
            {
                PacketManager.Instance.StartPostNetworking(this, url, _byteData, OnSuccess, Retry, true);
            }
            else
            {
                PacketManager.Instance.StartGetNetworking(this, url, _query, OnSuccess, Retry, true);
            }
        }
        else
        {
            HideLoading();
        }

    }

}
