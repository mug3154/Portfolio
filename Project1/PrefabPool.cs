using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

//아이템칸과 같이 이곳저곳에서 사용하는 프리팹들을 관리
public class PrefabPool : MonoBehaviour
{
    public ObjectPool<CellIcon> CellIconPool;
    public ObjectPool<CellEquipmentSummon> EquipmentSummonPool;

    public void LoadAdressables()
    {
        CreatePool<CellIcon>();
        CreatePool<CellEquipmentSummon>();
    }

    private void CreatePool<T>()
    {
        var type = typeof(T);

        var handler = Addressables.InstantiateAsync(type.FullName, transform);
        handler.WaitForCompletion();

        if (typeof(T).Equals(typeof(CellIcon)))
        {
            CellIconPool = new ObjectPool<CellIcon>(() => Instantiate(handler.Result).GetComponent<CellIcon>());
        }
        else if (typeof(T).Equals(typeof(CellEquipmentSummon)))
        {
            EquipmentSummonPool = new ObjectPool<CellEquipmentSummon>(() => Instantiate(handler.Result).GetComponent<CellEquipmentSummon>());

        }
    }

    public void ReleaseCellIcons(ref List<CellIcon> list)
    {
        if (list.Count > 0)
        {
            for (int i = list.Count - 1; i > -1; --i)
            {
                ReleaseCellIcon(list[i]);
            }
            list.Clear();
        }
    }

    public void ReleaseCellIcon(CellIcon obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        CellIconPool.Release(obj);
    }

    public void ReleaseEquipmentSummon(CellEquipmentSummon obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        EquipmentSummonPool.Release(obj);
    }
}
