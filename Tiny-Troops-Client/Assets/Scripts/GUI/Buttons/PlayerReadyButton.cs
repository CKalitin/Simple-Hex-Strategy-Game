using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReadyButton : MonoBehaviour {
    [SerializeField] private GameObject togglableIsReady;

    private bool ready = false;

    private void OnEnable() {
        MatchManager.OnMatchStateChanged += OnMatchStateChanged;
    }

    private void OnDisable() {
        MatchManager.OnMatchStateChanged -= OnMatchStateChanged;
    }

    public void OnReadyButton() {
        ready = !ready;
        togglableIsReady.SetActive(ready);
        USNL.PacketSend.PlayerReady(ready);
    }

    private void OnMatchStateChanged(MatchState _matchState) {
        if (_matchState == MatchState.InGame) {
            togglableIsReady.SetActive(false);
            ready = false;
        }
    }
}
