using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectButton : MonoBehaviour {
    public void OnDisconnectButtonDown() {
        USNL.ClientManager.instance.DisconnectFromServer();
        SceneLoader.instance.LoadMainMenu();
    }
}
