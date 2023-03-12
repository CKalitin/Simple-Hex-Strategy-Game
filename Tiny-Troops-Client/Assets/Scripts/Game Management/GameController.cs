using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    public static bool ApplicationQuitting = false;

    [SerializeField] private bool connectOnStart = true;
    [Space]
    [SerializeField] private int port;

    private void Awake() {
        Application.runInBackground = true;
        Debug.LogError("Opened Console.");
    }

    private void Start() {
        if (connectOnStart) USNL.ClientManager.instance.ConnectToServer(PlayerPrefs.GetInt("HostId"), port);
    }

    private void OnApplicationQuit() {
        ApplicationQuitting = true;
    }
    
    private void OnEnable() {
        USNL.CallbackEvents.OnConnected += OnConnected;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnConnected -= OnConnected;
    }

    private void OnConnected(object _object) {
        USNL.PacketSend.PlayerSetupInfo(PlayerPrefs.GetString("Username", "Username"));
    }
}
