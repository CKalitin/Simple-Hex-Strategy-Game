using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyController : MonoBehaviour {
    [SerializeField] private float waitTimeAllReady = 5f;
    [SerializeField] private float waitTimeOneOrMoreReady = 30f;

    private bool noneReady;
    private bool oneOrMoreReady;
    private bool allReady;

    private void Update() {
        CheckReadiness();
    }
    
    private void CheckReadiness() {
        int readyCount = 0;

        for (int i = 0; i < PlayerInfoManager.instance.PlayerInfos.Count; i++) {
            if (PlayerInfoManager.instance.PlayerInfos[i].Ready) readyCount++;
        }
        
        if (readyCount == 0) {
            if (!noneReady) {
                noneReady = true;
                oneOrMoreReady = false;
                allReady = false;
                MatchManager.instance.NewCountdown(-1, MatchState.InGame);
            }
            return;
        }

        if (readyCount == USNL.ServerManager.GetNumberOfConnectedClients()) {
            if (!allReady) {
                noneReady = false;
                oneOrMoreReady = false;
                allReady = true;
                MatchManager.instance.NewCountdown(waitTimeAllReady, MatchState.InGame);
            }
            return;
        }

        if (readyCount > 0) {
            if (!oneOrMoreReady) {
                oneOrMoreReady = true;
                noneReady = false;
                allReady = false;
                MatchManager.instance.NewCountdown(waitTimeOneOrMoreReady, MatchState.InGame);
            }
            return;
        }
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnPlayerSetupInfoPacket += OnPlayerSetupInfoPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnPlayerSetupInfoPacket -= OnPlayerSetupInfoPacket;
    }

    private void OnPlayerSetupInfoPacket(object _packetObject) {
        USNL.PlayerSetupInfoPacket packet = (USNL.PlayerSetupInfoPacket)_packetObject;

        // Do something? TODO DELETE
    }
}
