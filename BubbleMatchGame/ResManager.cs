using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.U2D;
using static Config;

public class ResManager : MonoBehaviour 
{
    [SerializeField] SpriteAtlas _Atlas;

    Dictionary<BUBBLE_TYPE, ObjectPool<Bubble>> _BubblePoolDic;

    Action<float> _ProgressCallback;

    public void LoadResource(Action<float> progressCallback)
    {
        if (_BubblePoolDic != null)
        {
            progressCallback?.Invoke(1);

            return;
        }

        _ProgressCallback = progressCallback;

        StartCoroutine(LoadResource());
    }

    private IEnumerator LoadResource()
    {
        Array bubbleTypeArr = Enum.GetValues(typeof(BUBBLE_TYPE));
        int TOTAL_RESOURCE_COUNT = bubbleTypeArr.Length; 

        _BubblePoolDic = new Dictionary<BUBBLE_TYPE, ObjectPool<Bubble>>();

        ushort idx = 0;

        for (int i = 0; i < TOTAL_RESOURCE_COUNT; i++)
        {
            idx = (ushort)bubbleTypeArr.GetValue(i);

            var handler = Addressables.LoadAssetAsync<GameObject>($"Bubble_{idx}");
            yield return handler;

            if(handler.Result != null)
            {
                ObjectPool<Bubble> pool = new ObjectPool<Bubble>(() => Instantiate(handler.Result.GetComponent<Bubble>()));
                _BubblePoolDic.Add((BUBBLE_TYPE)idx, pool);
                _ProgressCallback?.Invoke((i + 1) / TOTAL_RESOURCE_COUNT);
            }
            else
            {
                Addressables.Release(handler);
            }
        }
    }


    public Sprite GetSprite(string name)
    {
        return _Atlas.GetSprite(name);
    }

    public Sprite GetBubbleSprite(int color)
    {
        return GetSprite($"Bubble_{color}");
    }


    public Bubble GetBubble(Config.BUBBLE_TYPE type)
    {
        return _BubblePoolDic[type].Get();
    }

    public void ReleaseBubble(Bubble bubble)
    {
        bubble.Dispose();
        bubble.gameObject.SetActive(false);
        bubble.transform.SetParent(transform);

        _BubblePoolDic[bubble.Type].Release(bubble);
    }



    public void LoadMapData(int map, Action<string> endCallback, Action failCallback)
    {
        var handler = Addressables.LoadAssetAsync<TextAsset>($"Map/MapInfos/Map{map}.csv").WaitForCompletion();

        if(handler != null)
        {
            endCallback?.Invoke(handler.text);
        }
        else
        {
            failCallback?.Invoke();
        }
    }

}
