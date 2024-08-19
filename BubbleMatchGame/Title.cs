using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] Button _PlayButton;

    // Start is called before the first frame update
    void Start()
    {
        _PlayButton.onClick.AddListener(OnClickPlayButton);
    }

    private void OnClickPlayButton()
    {
        Root.Instance.LoadScene(Root.SCENE_IDX.HOME, (progress) =>
        {
            if(progress == 1)
            {
                Home.Instance.Show();

                Destroy(gameObject);
            }
        });
    }
}
