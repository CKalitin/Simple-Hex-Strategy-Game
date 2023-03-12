using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyController : MonoBehaviour {
    public static LobbyController instance;

    [SerializeField] private LobbyPlayerDisplay[] playerDisplays;

    private void Awake() {
        Debug.Log("Awake");
        if (instance == null) instance = this;
        else {
            Debug.Log($"Lobby Controller instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
        Debug.Log(playerDisplays[0].gameObject);
        Debug.Log(playerDisplays[1].gameObject);
        Debug.Log(playerDisplays[2].gameObject);
        Debug.Log(playerDisplays[3].gameObject);

        // Set all player displays to inactive
        for (int i = 0; i < playerDisplays.Length; i++) playerDisplays[i].gameObject.SetActive(false);
    }

    private void Update() {
        
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnPlayerReadyPacket += OnPlayerReadyPacket;
        USNL.CallbackEvents.OnPlayerInfoPacket += OnPlayerInfoPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnPlayerReadyPacket -= OnPlayerReadyPacket;
        USNL.CallbackEvents.OnPlayerInfoPacket += OnPlayerInfoPacket;
    }
    
    private void OnPlayerReadyPacket(object _packetObject) {
        USNL.PlayerReadyPacket packet = (USNL.PlayerReadyPacket)_packetObject;
        int id = GetIndex(packet.ClientID);

        // If player display is not active, activate it. Then update Ready display
        if (playerDisplays[id].gameObject.activeSelf == false) SetPlayerDisplayActive(id, true);
        playerDisplays[id].TogglableReady.SetActive(packet.Ready);
    }

    private void OnPlayerInfoPacket(object _packetObject) {
        USNL.PlayerInfoPacket packet = (USNL.PlayerInfoPacket)_packetObject;
        int id = GetIndex(packet.ClientID);

        // Check Client disconnected
        if (packet.Username == "") {
            OnClientDisconnected(packet.ClientID);
            return;
        }

        // If player display is not active, activate it. Then update Username
        if (playerDisplays[id].gameObject.activeSelf == false)  SetPlayerDisplayActive(id, true);
        playerDisplays[id].UsernameText.text = packet.Username;
    }

    private void OnClientDisconnected(int _clientID) {
        Debug.Log(playerDisplays[0].gameObject);
        int id = GetIndex(_clientID);
        playerDisplays[id].gameObject.SetActive(false);
    }

    // This function makes the local client always appear first
    private int GetIndex(int _id) {
        if (_id == USNL.ClientManager.instance.ClientId) return 0;
        else if (_id == 0) return USNL.ClientManager.instance.ClientId;
        return _id;
    }

    private void SetPlayerDisplayActive(int id, bool _active) {
        playerDisplays[id].UsernameText.text = "";
        playerDisplays[id].TogglableReady.SetActive(false);
        playerDisplays[id].gameObject.SetActive(_active);
    }
}
