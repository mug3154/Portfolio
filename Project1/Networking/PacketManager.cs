using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Web;
using System.Linq;

public class PacketManager : Singleton<PacketManager>
{
    private string _DEFAULT_URL = "";
    private string _dataVersion = "0";

    public delegate void ImageCallback(Texture texture);

    private List<NetBase> _netBases = new List<NetBase>();
    private List<UnityWebRequest> _unityWebRequests = new List<UnityWebRequest>();

    public delegate void OnNetworkingException(UnityWebRequest webRequest);
    public delegate void OnNetworkingSuccess(byte[] data);
    public delegate void OnNetworkingError(long responseCode);

    private WaitForSecondsRealtime _delayRealSec = new WaitForSecondsRealtime(5);

    bool _TableLoading;

    CoroutineQueue _SendQueue;


    private void Start()
    {
        _SendQueue = new CoroutineQueue(1, StartCoroutine);
    }

    public void SetDefaultURL(string url)
    {
        if (url.Last() != '/')
        {
            url += '/';
        }

        _DEFAULT_URL = url;
    }

    public void SetDataVersion(uint version)
    {
        _dataVersion = version.ToString();
    }

    struct ReadyCallNetPost
    {
        public NetBase net;
        public string url;
        public byte[] content;
        public OnNetworkingSuccess onSuccess;
        public OnNetworkingError onNetworkingError;
        public bool delay;
    }

    struct ReadyCallNetGet
    {
        public NetBase net;
        public string url;
        public List<string> content;
        public OnNetworkingSuccess onSuccess;
        public OnNetworkingError onNetworkingError;
        public bool delay;
    }

    List<ReadyCallNetPost> _ReadyPostList = new List<ReadyCallNetPost>();
    List<ReadyCallNetGet> _ReadyGetList = new List<ReadyCallNetGet>();
    
    
    public void StartDirectPostNetworking(string url, byte[] content)
    {
        UnityWebRequest webRequest = new UnityWebRequest(_DEFAULT_URL + url + GetArgumentData(), UnityWebRequest.kHttpVerbPOST);
        webRequest.uploadHandler = new UploadHandlerRaw(content);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/octet-stream");
        webRequest.timeout = 30;

        webRequest.SendWebRequest();
    }


    public void StartPostNetworking(NetBase net, string url, byte[] content, OnNetworkingSuccess onSuccess, OnNetworkingError onNetworkingError, bool delay)
    {
        _SendQueue.Run(PostNetworking(net, url, content, onSuccess, onNetworkingError, delay));
    }

    private IEnumerator PostNetworking(NetBase net, string url, byte[] content, OnNetworkingSuccess onSuccess, OnNetworkingError onNetworkingError, bool delay)
    {
        _netBases.Add(net);

        if (delay)
            yield return _delayRealSec;


        UnityWebRequest webRequest = new UnityWebRequest(_DEFAULT_URL + url + GetArgumentData(), UnityWebRequest.kHttpVerbPOST);
        webRequest.uploadHandler = new UploadHandlerRaw(content);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/octet-stream");
        webRequest.timeout = 30;
        
        Debug.Log("[NETWORK START] Post url:" + webRequest.url);
        _unityWebRequests.Add(webRequest);

        yield return webRequest.SendWebRequest();

        _unityWebRequests.Remove(webRequest);


       if (webRequest.result == UnityWebRequest.Result.Success)
        {
            byte[] resultData = webRequest.downloadHandler.data;
            webRequest.Dispose();
            onSuccess?.Invoke(resultData);
        }
        else
        {
            long responseCode = webRequest.responseCode;        
            
            webRequest.Dispose();

            if (responseCode == 603)
            {
                _ReadyPostList.Add(new ReadyCallNetPost()
                {
                    net = net,
                    url = url,
                    content = content,
                    onSuccess = onSuccess,
                    onNetworkingError = onNetworkingError,
                    delay = delay
                });

                TableDownload();
            }
            else
            {
                onNetworkingError?.Invoke(responseCode);

                OnException(url, responseCode);
            }
        }

    }

    public void StartGetNetworking(NetBase net, string url, List<string> query, OnNetworkingSuccess onSuccess, OnNetworkingError onNetworkingError, bool delay)
    {
        StartCoroutine( GetNetworking(net, url, query, onSuccess, onNetworkingError, delay) );
    }

    private IEnumerator GetNetworking(NetBase net, string url, List<string> query, OnNetworkingSuccess onSuccess, OnNetworkingError onNetworkingError, bool delay)
    {
        _netBases.Add(net);

        if (delay)
            yield return _delayRealSec;

        if (query != null)
        {
            foreach(var q in query)
            {
                url += "/" + HttpUtility.UrlEncode(q);
            }
        }

        UnityWebRequest webRequest = UnityWebRequest.Get(_DEFAULT_URL + url + GetArgumentData());
        webRequest.timeout = 5;
        webRequest.SetRequestHeader("Content-Type", "x-www-form-urlencoded");
        
        _unityWebRequests.Add(webRequest);
      
        yield return webRequest.SendWebRequest();

        _unityWebRequests.Remove(webRequest);
        _netBases.Remove(net);


        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            byte[] resultData = webRequest.downloadHandler.data;
            webRequest.Dispose();
            onSuccess?.Invoke(resultData);
        }
        else
        {
            long responseCode = webRequest.responseCode;

            webRequest.Dispose();

            if (isTableChanged)
            {
                _ReadyGetList.Add(new ReadyCallNetGet()
                {
                    net = net,
                    url = url,
                    content = query,
                    onSuccess = onSuccess,
                    onNetworkingError = onNetworkingError,
                    delay = delay
                });

                TableDownload();
            }
            else
            {
                onNetworkingError?.Invoke(responseCode);

                OnException(url, responseCode);
            }
        }
    }

    public void OnException(string url, long responseCode)
    {
        Debug.Log($"Test =========== OnException url:{url}, responseCode:{responseCode}");
    }

    public void AllNetworkAbort()
    {
        foreach(var n in _unityWebRequests)
        {
            n.Abort();
            n.Dispose();
        }
        _unityWebRequests.Clear();
        _netBases.Clear();
    }
}
