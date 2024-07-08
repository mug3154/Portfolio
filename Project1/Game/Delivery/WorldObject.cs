using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Config;
using System.Linq;
using UnityEngine.Rendering;

public class WorldObject : MonoBehaviour
{
    [SerializeField] SpriteRenderer _sprite;

    public DeliveryMapObjData Data;

    private IWorldObjectEffect _effect;
    private float _effectValue;

    public void SetData(DeliveryMapObjData data, float effectValue)
    {
        _sprite.sprite = RootScene.atlasPool.GetImage(AtlasPool.ATLAS_TYPE.WORLD, ((uint)data.type).ToString());

        Data.type = data.type;
        Data.pos = data.pos;

        //맵 타일 효과 타입에 따른 이펙트 클래스 생성 
        //ex) if(data.type == 길)
        //    {
        //        _effect = new WorldObjectEffect_길();
        //    }

        _effectValue = effectValue;
    }

    public void InTile()
    {
        _effect?.InTile(_effectValue);
    }

    public void OutTile()
    {
        _effect?.OutTile(_effectValue);
    }

    public void PlayEffect()
    {
        _effect?.PlayEffect();
    }

}
