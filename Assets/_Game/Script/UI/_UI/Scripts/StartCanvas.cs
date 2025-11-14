using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartCanvas : UICanvas
{
    [SerializeField] private Button startBtn;

    //Login
    [SerializeField] private Button anoSignBtn;
    [SerializeField] private Button unitySignBtn;
    [SerializeField] private Button FbSignBtn;

    private void Start()
    {
        startBtn.onClick.AddListener(() =>
        {
            //AudioManager.Ins.PlaySFX(AudioManager.Ins.click);

            UIManager.Ins.TransitionUI<ChangeUICanvas, StartCanvas>(0.5f,
                () =>
                {
                    UIManager.Ins.OpenUI<MainCanvas>();
                });
        });

        anoSignBtn.onClick.AddListener(() =>
        {
            LoginManager.Ins.StartAnonymousSignIn();
        });

        unitySignBtn.onClick.AddListener(() =>
        {
            LoginManager.Ins.StartUnitySignInAsync();
        });

        FbSignBtn.onClick.AddListener(() =>
        {
            LoginManager.Ins.StartFacebookSignIn();
        });
    }
}
