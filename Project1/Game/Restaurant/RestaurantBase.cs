using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestaurantBase : MonoBehaviour
{
    public enum E_STATE
    {
        OPEN,
        CONSTRUCT,
        EMPTY
    }

    [HideInInspector] public E_STATE state;
    [HideInInspector] public int idx;
    private uint _code;
    public uint code { get => _code; }

    public void SetData(int idx, E_STATE state)
    {
        this.idx = idx;
        this.state = state;
    }

    public virtual void SetCode(uint code)
    {
        _code = code;
    }

    public virtual void OnClick()
    {

    }
}
