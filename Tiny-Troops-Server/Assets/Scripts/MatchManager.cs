using System;
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

    private bool timerActive = false;
    private DateTime startTime;
    private float duration;
    private MatchState targetMatchState;
    private string countdownTag = "Match State Countdown";

    public MatchState MatchState { get => matchState; set => matchState = value; }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Match Manager instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
    }
    
    private void Update() {
        Countdown();

        if (USNL.ServerManager.GetNumberOfConnectedClients() <= 0)
            ChangeMatchState(MatchState.Lobby);
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnClientConnected += OnClientConnected;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnClientConnected -= OnClientConnected;
    }

    private void Countdown() {
        if (!timerActive) return;
        
        float timePassed = (float)(DateTime.Now - startTime).TotalSeconds;
        if (timePassed >= duration) {
            ChangeMatchState(targetMatchState);
        }
    }

    public void ChangeMatchState(MatchState _matchState) {
        matchState = _matchState;

        timerActive = false;

        USNL.PacketSend.MatchUpdate((int)matchState);
    }
    
    // If _duration is lower than 0, it cancels the timer client side.
    public void NewCountdown(float _duration, MatchState _targetMatchState) {
        // Cancel timer if _duration is lower than 0
        if (_duration < 0) {
            timerActive = false;
        } else {
            timerActive = true;
        }

        startTime = DateTime.Now;
        duration = _duration;
        targetMatchState = _targetMatchState;

        int[] startTimeArray = new int[4] { startTime.Hour, startTime.Minute, startTime.Second, startTime.Millisecond };

        USNL.PacketSend.Countdown(startTimeArray, duration, countdownTag);
    }

    private void OnClientConnected(object _clientIdObject) {
        int[] startTimeArray = new int[4] { startTime.Hour, startTime.Minute, startTime.Second, startTime.Millisecond };

        USNL.PacketSend.Countdown(startTimeArray, duration, countdownTag);
    }
}
