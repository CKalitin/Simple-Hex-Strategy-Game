using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    #region Variables

    public static GameController instance;
    
    private MatchState previousMatchState = MatchState.Lobby;

    #endregion

    #region Core

    private void Awake() {
        Debug.LogError("Opened Console.");

        if (instance == null) instance = this;
        else {
            Debug.Log($"Game Controller instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
    }

    private void Start() {
        USNL.ServerManager.instance.StartServer();
    }

    private void Update() {
        if (previousMatchState != MatchManager.instance.MatchState) OnMatchStateChange(MatchManager.instance.MatchState);
        previousMatchState = MatchManager.instance.MatchState;
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnClientConnected += OnClientConnected;
        USNL.CallbackEvents.OnClientDisconnected += OnClientDisconnected;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnClientConnected -= OnClientConnected;
        USNL.CallbackEvents.OnClientDisconnected -= OnClientDisconnected;
    }

    #endregion

    #region Match State

    private void OnMatchStateChange(MatchState _ms) {
        if (_ms == MatchState.Lobby) {
            USNL.ServerManager.instance.AllowNewConnections = true;
        } else if (_ms == MatchState.InGame) {
            USNL.ServerManager.instance.AllowNewConnections = false;
            StartGame();
        } else if (_ms == MatchState.Ended) {
            ResetGame();
        }
    }

    private void StartGame() {
        for (int i = 0; i < USNL.ServerManager.GetConnectedClientIds().Length; i++) {
            
        }
    }

    private void ResetGame() {
        
    }

    #endregion

    #region Callbacks

    private void OnClientConnected(object _clientIdObject) {
        int clientId = (int)_clientIdObject;
        
    }

    private void OnClientDisconnected(object _clientIdObject) {
        int clientId = (int)_clientIdObject;
    }
    
    #endregion
}
