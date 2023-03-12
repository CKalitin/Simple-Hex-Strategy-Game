using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReadyButton : MonoBehaviour {
    [SerializeField] private GameObject togglableIsReady;

    private bool ready = false;

    public void OnReadyButton() {
        ready = !ready;
        togglableIsReady.SetActive(ready);
        USNL.PacketSend.PlayerReady(ready);
    }
}
