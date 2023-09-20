using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverParentToggler : MonoBehaviour {
    [SerializeField] private GameObject gameUiParent;
    [SerializeField] private GameObject gameOverParent;

    private void Awake() {
        gameOverParent.SetActive(false);
        gameUiParent.SetActive(true);
    }

    private void Update() {
        ClientBase[] bases = FindObjectsOfType<ClientBase>();

        bool disable = true;

        for (int i = 0; i < bases.Length; i++) {
            if (bases[i].GetComponent<Structure>().PlayerID == MatchManager.instance.PlayerID) disable = false;
        }

        gameOverParent.SetActive(disable);
        gameUiParent.SetActive(!disable);
    }
}
