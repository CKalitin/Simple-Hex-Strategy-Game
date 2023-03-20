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

    private int playerID = -1;

    public delegate void MatchStateChangedCallback(MatchState matchState);
    public static event MatchStateChangedCallback OnMatchStateChanged;

    public MatchState MatchState { get => matchState; set => matchState = value; }
    public int PlayerID { get => playerID; set => playerID = value; }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Match Manager instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
        
        // Calling this from Awake means it will be called before other OnConnected() callbacks, see Unity Core of Execution for Event Functions
        // Also, this doesn't work from OnEnable()
        USNL.CallbackEvents.OnWelcomePacket += OnWelcomePacket;
    }
    
    private void OnEnable() {
        USNL.CallbackEvents.OnMatchUpdatePacket += OnMatchUpdatePacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnWelcomePacket -= OnWelcomePacket;
        USNL.CallbackEvents.OnMatchUpdatePacket -= OnMatchUpdatePacket;
    }

    private void OnMatchUpdatePacket(object _packetObject) {
        USNL.MatchUpdatePacket packet = (USNL.MatchUpdatePacket)_packetObject;
        matchState = (MatchState)packet.MatchState;
        
        // Call callbacks
        if (OnMatchStateChanged != null) OnMatchStateChanged(matchState);
    }
    
    private void OnWelcomePacket(object _packetObject) {
        USNL.Package.WelcomePacket _wp = (USNL.Package.WelcomePacket)_packetObject;
        playerID = _wp.ClientId;
    }
}
