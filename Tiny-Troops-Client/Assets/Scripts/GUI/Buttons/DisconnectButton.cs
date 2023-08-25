using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectButton : MonoBehaviour {
    public static bool VoluntaryDisconnection = false;

    private void OnEnable() {
        USNL.CallbackEvents.OnConnected += OnConnected;
    }
    
    private void OnDisable() {
        USNL.CallbackEvents.OnConnected -= OnConnected;
    }

    public void OnDisconnectButtonDown() {
        VoluntaryDisconnection = true;
        USNL.ClientManager.instance.DisconnectFromServer();
        SceneLoader.instance.LoadMainMenu();
    }

    private void OnConnected(object _object) {
        VoluntaryDisconnection = false;
    }
}
