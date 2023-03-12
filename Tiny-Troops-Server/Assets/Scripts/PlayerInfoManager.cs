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

    public void UpdatePlayerScore() {
        ScoreConfig sc = PlayerInfoManager.instance.ScoreConfig;
        score = 0;
    }
}

[Serializable]
public struct ScoreConfig {
    /*[SerializeField] private int playerKillScore;
    [SerializeField] private int playerDeathScore;
    [Space]
    [SerializeField] private int enemyKillScore;
    [SerializeField] private int enemyDeathScore;
    [Space]
    [SerializeField] private float damageDealtScoreMultipler;
    [SerializeField] private float damageTakenScoreMultipler;

    public int PlayerKillScore { get => playerKillScore; set => playerKillScore = value; }
    public int PlayerDeathScore { get => playerDeathScore; set => playerDeathScore = value; }
    
    public int EnemyKillScore { get => enemyKillScore; set => enemyKillScore = value; }
    public int EnemyDeathScore { get => enemyDeathScore; set => enemyDeathScore = value; }

    public float DamageDealtScoreMultipler { get => damageDealtScoreMultipler; set => damageDealtScoreMultipler = value; }
    public float DamageTakenScoreMultipler { get => damageTakenScoreMultipler; set => damageTakenScoreMultipler = value; }*/
}

public class PlayerInfoManager : MonoBehaviour {
    #region Variables

    public static PlayerInfoManager instance;

    [SerializeField] private ScoreConfig scoreConfig;
    [Space]
    [SerializeField] private List<PlayerInfo> playerInfos = new List<PlayerInfo>();

    public ScoreConfig ScoreConfig { get => scoreConfig; set => scoreConfig = value; }
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
        
        Initialize();
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnPlayerSetupInfoPacket += OnPlayerSetupInfoPacket;
        USNL.CallbackEvents.OnPlayerReadyPacket += OnPlayerReadyPacket;
        USNL.CallbackEvents.OnClientConnected += OnClientConnected;
        USNL.CallbackEvents.OnClientDisconnected += OnClientDisconnected;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnPlayerSetupInfoPacket -= OnPlayerSetupInfoPacket;
        USNL.CallbackEvents.OnPlayerReadyPacket -= OnPlayerReadyPacket;
        USNL.CallbackEvents.OnClientConnected -= OnClientConnected;
        USNL.CallbackEvents.OnClientDisconnected -= OnClientDisconnected;
    }

    #endregion
    
    #region Player Info Management

    private void Initialize() {
        playerInfos = new List<PlayerInfo>();
        for (int i = 0; i < USNL.ServerManager.instance.ServerConfig.MaxClients; i++)
            playerInfos.Add(new PlayerInfo());
    }

    public void SendPlayerInfo(int _id) {
        Debug.Log(playerInfos[_id].Username);
        playerInfos[_id].UpdatePlayerScore();
        USNL.PacketSend.PlayerInfo(_id, playerInfos[_id].Username, playerInfos[_id].Score);
    }

    #endregion

    #region Callbacks

    private void OnPlayerSetupInfoPacket(object _packetObject) {
        USNL.PlayerSetupInfoPacket packet = (USNL.PlayerSetupInfoPacket)_packetObject;
        playerInfos[packet.FromClient].Username = packet.Username;

        SendPlayerInfo(packet.FromClient);
    }

    private void OnPlayerReadyPacket(object _packetObject) {
        USNL.PlayerReadyPacket packet = (USNL.PlayerReadyPacket)_packetObject;
        playerInfos[packet.FromClient].Ready = packet.Ready;

        USNL.PacketSend.PlayerReady(packet.FromClient, packet.Ready);
    }

    private void OnClientConnected(object _clientIdObject) {
        int clientId = (int)_clientIdObject;

        for (int i = 0; i < playerInfos.Count; i++) {
            if (USNL.ServerManager.instance.GetClientConnected(i)) {
                SendPlayerInfo(i);
                USNL.PacketSend.PlayerReady(i, playerInfos[i].Ready);
            }
        }
    }

    private void OnClientDisconnected(object _clientIdObject) {
        int clientId = (int)_clientIdObject;

        playerInfos[clientId] = new PlayerInfo();

        SendPlayerInfo(clientId);
    }

    #endregion
}
