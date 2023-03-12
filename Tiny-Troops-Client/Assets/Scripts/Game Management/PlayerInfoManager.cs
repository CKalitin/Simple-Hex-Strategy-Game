using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Should be a struct, ugh
[Serializable]
public class PlayerInfo {
    [SerializeField] private string username;
    
    [SerializeField] private bool ready;

    [SerializeField] private int score;

    public PlayerInfo() {
        username = "";
        ready = false;
        score = 0;
    }

    public string Username { get => username; set => username = value; }

    public bool Ready { get => ready; set => ready = value; }

    public int Score { get => score; set => score = value; }
}

public class PlayerInfoManager : MonoBehaviour {
    #region Variables
    
    public static PlayerInfoManager instance;

    [SerializeField] private List<PlayerInfo> playerInfos = null;

    public List<PlayerInfo> PlayerInfos { get => playerInfos; set => playerInfos = value; }

    #endregion

    #region Core

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Match Manager instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnPlayerInfoPacket += OnPlayerInfoPacket;
        USNL.CallbackEvents.OnPlayerReadyPacket += OnPlayerReadyPacket;
        USNL.CallbackEvents.OnServerInfoPacket += OnServerInfoPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnPlayerInfoPacket -= OnPlayerInfoPacket;
        USNL.CallbackEvents.OnPlayerReadyPacket -= OnPlayerReadyPacket;
        USNL.CallbackEvents.OnServerInfoPacket -= OnServerInfoPacket;
    }

    #endregion

    #region Callbacks

    private void OnPlayerInfoPacket(object _packetObject) {
        USNL.PlayerInfoPacket packet = (USNL.PlayerInfoPacket)_packetObject;

        if (playerInfos.Count <= 0) return;
        
        playerInfos[packet.ClientID].Username = packet.Username;
        playerInfos[packet.ClientID].Score = packet.Score;
    }

    private void OnPlayerReadyPacket(object _packetObject) {
        USNL.PlayerReadyPacket packet = (USNL.PlayerReadyPacket)_packetObject;
        playerInfos[packet.ClientID].Ready = packet.Ready;
    }

    private void OnServerInfoPacket(object _clientIdObject) {
        USNL.Package.ServerInfoPacket packet = (USNL.Package.ServerInfoPacket)_clientIdObject;
        
        if (playerInfos.Count > 0) return;
        playerInfos = new List<PlayerInfo>();
        for (int i = 0; i < packet.MaxClients; i++)
            playerInfos.Add(new PlayerInfo());
    }

    #endregion
}
