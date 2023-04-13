using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour {
    #region Variables

    public static CanvasManager instance;

    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject lobbyCanvas;
    [SerializeField] private GameObject gameEndedCanvas;
    [Space]
    [SerializeField] private GameObject pauseMenuCanvas;
    [Space]
    [SerializeField] private GameObject connectingCanvas;
    [SerializeField] private GameObject serverClosedCanvas;
    [SerializeField] private GameObject timedOutCanvas;

    #endregion

    #region Core

    private void Awake() {
        Singleton();

        TogglePrimaryCanvases(false, false, false);
    }

    private void Singleton() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Canvas Manager instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

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
            TogglePrimaryCanvases(false, false, false);
        } else if (USNL.ClientManager.instance.TimedOut) {
            timedOutCanvas.SetActive(true);
            TogglePrimaryCanvases(false, false, false);
        }
    }

    private void OnEnable() {
        MatchManager.OnMatchStateChanged += OnMatchStateChanged;
        GameController.OnGameInitialized += OnGameInitialized;
    }

    private void OnDisable() {
        MatchManager.OnMatchStateChanged -= OnMatchStateChanged;
        GameController.OnGameInitialized -= OnGameInitialized;
    }

    #endregion

    #region Callbacks

    private void OnMatchStateChanged(MatchState _matchState) {
        if (_matchState == MatchState.InGame) {
            TogglePrimaryCanvases(true, false, false);
        } else if (_matchState == MatchState.Lobby) {
            if (!gameEndedCanvas.activeSelf) TogglePrimaryCanvases(false, true, false);
        } else if (_matchState == MatchState.Ended) {
            TogglePrimaryCanvases(false, false, true);
        }
    }

    private void OnGameInitialized() {
        connectingCanvas.SetActive(false);
    }

    #endregion

    #region Utils

    public void TogglePrimaryCanvases(bool _gameCanvas, bool _lobbyCanvas, bool _gameEndedCanvas) {
        gameCanvas.SetActive(_gameCanvas);
        lobbyCanvas.SetActive(_lobbyCanvas);
        gameEndedCanvas.SetActive(_gameEndedCanvas);
    }

    public void TogglePrimaryCanvases(bool _gameCanvas, bool _lobbyCanvas) {
        gameCanvas.SetActive(_gameCanvas);
        lobbyCanvas.SetActive(_lobbyCanvas);
    }

    #endregion
}
