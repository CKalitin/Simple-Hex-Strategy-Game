using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuButton : MonoBehaviour {
    public void LoadMainMenu() {
        SceneLoader.instance.LoadMainMenu();

        if (USNL.ClientManager.instance.IsConnected)
            USNL.Package.Client.instance.Disconnect();
    }
}
