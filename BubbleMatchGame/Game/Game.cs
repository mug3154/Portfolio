using UnityEngine;
using static Config;

public class Game : Singleton<Game>
{
   
    [SerializeField] GameUI _UI;
    [SerializeField] GameView _View;


    static GameMode _Mode;
    static public GameMode Mode => _Mode;

    Vector3 _CameraStartPos = new Vector3(0, 0, -10);

    public Config.GAME_MODE GAME_MODE {  get; private set; }


    private void Start()
    {
        GameFlowEventManager.Instance.Initialize();
    }


    public void Show(int map, int stage, string[] data)
    {
        GAME_MODE = (Config.GAME_MODE)(int.Parse(data[0]));
        if (GAME_MODE == 0)
        {
            string[] infoRow1 = data[1].Split(",");

            Config.GAME_TYPE type = (Config.GAME_TYPE)(int.Parse(infoRow1[0]));
    
            if(type == GAME_TYPE.BUBBLE)    _Mode = new GameMode_Bubble();
            else if(type == GAME_TYPE.BIRD) _Mode = new GameMode_Bird();
        }
        else
        {
            _Mode = new GameMode_Boss();
        }

        _Mode.Initialize(_View, _UI, map, stage, data);

        Camera.main.transform.position = _CameraStartPos;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        _Mode?.Dispose();
        _Mode = null;

        GameFlowEventManager.Instance.Dispose();
    }
}
