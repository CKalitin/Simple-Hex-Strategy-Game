using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatchState {
    Lobby,
    InGame,
    Paused,
    Ended
}

public class MatchManager : MonoBehaviour {
    public static MatchManager instance;

    private MatchState matchState = MatchState.Lobby;

    public MatchState MatchState { get => matchState; set => matchState = value; }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Match Manager instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
    }
    
    private void OnEnable() {
        USNL.CallbackEvents.OnMatchUpdatePacket += OnMatchUpdatePacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnMatchUpdatePacket -= OnMatchUpdatePacket;
    }

    private void OnMatchUpdatePacket(object _packetObject) {
        USNL.MatchUpdatePacket packet = (USNL.MatchUpdatePacket)_packetObject;
        matchState = (MatchState)packet.MatchState;
        Debug.Log(matchState);
    }
}
