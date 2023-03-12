using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour {
    [SerializeField] private GameObject lobbyCanvas;
    [SerializeField] private GameObject gameCanvas;
    [Space]
    [SerializeField] private GameObject pauseMenuCanvas;
    [Space]
    [SerializeField] private GameObject connectingCanvas;
    [SerializeField] private GameObject serverClosedCanvas;
    [SerializeField] private GameObject timedOutCanvas;

    private void Start() {
        OnMatchStateChanged(MatchManager.instance.MatchState);

        // Active connecting screen
        connectingCanvas.SetActive(true);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            pauseMenuCanvas.SetActive(!pauseMenuCanvas.activeSelf);
        }

        if (USNL.ClientManager.instance.ServerClosed) {
            serverClosedCanvas.SetActive(true);
            TogglePrimaryCanvases(false, false);
        } else if (USNL.ClientManager.instance.TimedOut) {
            timedOutCanvas.SetActive(true);
            TogglePrimaryCanvases(false, false);
        }
    }

    private void OnEnable() {
        MatchManager.OnMatchStateChanged += OnMatchStateChanged;
        USNL.CallbackEvents.OnConnected += OnConnected;
    }

    private void OnDisable() {
        MatchManager.OnMatchStateChanged -= OnMatchStateChanged;
        USNL.CallbackEvents.OnConnected -= OnConnected;
    }

    private void OnMatchStateChanged(MatchState _matchState) { 
        if (_matchState == MatchState.InGame) {
            TogglePrimaryCanvases(false, true);
        } else if (_matchState == MatchState.Lobby) {
            TogglePrimaryCanvases(true, false);
        }
    }

    private void TogglePrimaryCanvases(bool _lobbyCanvas, bool _gameCanvas) {
        lobbyCanvas.SetActive(_lobbyCanvas);
        gameCanvas.SetActive(_gameCanvas);
    }

    private void OnConnected(object _object) {
        connectingCanvas.SetActive(false);
    }
}
