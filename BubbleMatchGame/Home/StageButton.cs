using UnityEngine;
using UnityEngine.UI;
using System;

public class StageButton : MonoBehaviour
{
    [SerializeField] Image[] _Stars;
    [SerializeField] Image _ModeIcon;
    [SerializeField] Text _NumberText;

    public Action<int> OnClickCallback;


    public int Stage {  get; private set; }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            OnClickCallback?.Invoke(Stage);
        });
    }

    public void SetData(int stage, Config.GAME_MODE mode, int value, StageClearData data)
    {
        Stage = stage;

        _NumberText.text = stage.ToString();

        SetClearData(data);

        if(mode == Config.GAME_MODE.BOSS)
        {
            _ModeIcon.sprite = Root.Instance.ResManager.GetSprite($"Boss_{value}");
        }
        else
        {
            if (value == (int)Config.GAME_TYPE.BUBBLE)
            {
                _ModeIcon.sprite = Root.Instance.ResManager.GetSprite("Bubble_0");
            }
            else if (value == (int)Config.GAME_TYPE.BIRD)
            {
                _ModeIcon.sprite = Root.Instance.ResManager.GetSprite("Bird");
            }
        }
    }

    public void SetClearData(StageClearData data)
    {
        if (data.MapIdx == 0)
        {
            foreach (var star in _Stars)
            {
                star.color = Color.gray;
            }

            GetComponent<Image>().color = Color.magenta;
        }
        else
        {
            for (int i = 0; i < 3; ++i)
            {
                _Stars[i].color = data.StarCount > i ? Color.white : Color.gray;
            }

            GetComponent<Image>().color = Color.yellow;
        }
    }
}
