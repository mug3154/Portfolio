
public class AtlasPool : MonoBehaviour
{
    public enum ATLAS_TYPE
    {
        WORLD = 0,
        UI,
        BANNER,
        EQUIP_ICON,
        ...,
        EVENTS,
        END
    }

    private SpriteAtlas[] _atlas = new SpriteAtlas[(int)ATLAS_TYPE.END];
    private Dictionary<string, Sprite>[] _spriteDic = new Dictionary<string, Sprite>[(int)ATLAS_TYPE.END];
    private int _atlasLoadCount = 0;

    public void LoadAdressables()
    {
        _atlasLoadCount = 0;

        while (_atlasLoadCount < (int)ATLAS_TYPE.END)
        {
            var handler = Addressables.LoadAssetAsync<SpriteAtlas>(GetAtlasName((ATLAS_TYPE)_atlasLoadCount));

            handler.WaitForCompletion();

            _atlas[_atlasLoadCount] = handler.Result;

            _spriteDic[_atlasLoadCount] = new Dictionary<string, Sprite>();

            ++_atlasLoadCount;
        }
    }

    private string GetAtlasName(ATLAS_TYPE type)
    {
        switch (type)
        {
            case ATLAS_TYPE.WORLD: return "Atlas_WorldMap";
            case ATLAS_TYPE.UI: return "Atlas_UI";
            case ATLAS_TYPE.BANNER: return "Atlas_Banner";
            case ATLAS_TYPE.EQUIP_ICON: return "Atlas_EquipIcon";
                ...............,
            case ATLAS_TYPE.EVENTS: return "Atlas_Events";
            default: return "";
        }
    }

    public Sprite GetImage(ATLAS_TYPE type, string name)
    {
        Sprite sprite = null;
        Dictionary<string, Sprite> dic = _spriteDic[(int)type];
        if (dic.ContainsKey(name) == false)
        {
            sprite = _atlas[(int)type].GetSprite(name);
            dic.Add(name, sprite);
        }
        else
        {
            sprite = dic[name];
        }
        return sprite;
    }



    public Sprite GetResourceSprite(uint code)
    {
        uint tableCode = code.GetTableCode();

        if (tableCode == Equipment.tableCode)
        {
            return GetImage(ATLAS_TYPE.EQUIP_ICON, code.ToString());
        }
        else if (tableCode == Item.tableCode)
        {
            return GetImage(ATLAS_TYPE.UI, code.ToString());
        }
        ..........

        return GetImage(ATLAS_TYPE.UI, code.ToString());
    }
}
