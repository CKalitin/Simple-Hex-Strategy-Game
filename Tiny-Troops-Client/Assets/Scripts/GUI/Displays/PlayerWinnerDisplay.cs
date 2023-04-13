using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerWinnerDisplay : MonoBehaviour {
    [SerializeField] private GameObject wonDisplayParent;
    [SerializeField] private GameObject lostDisplayParent;
    [Space]
    [SerializeField] private TextMeshProUGUI[] playerWinnerText;

    void Update() {
        // If no one won
        if (GameController.instance.WinnerPlayerID < 0) return;

        for (int i = 0; i < playerWinnerText.Length; i++) {
            playerWinnerText[i].text = $"{PlayerInfoManager.instance.PlayerInfos[GameController.instance.WinnerPlayerID].Username} Won!";
        }

        if (GameController.instance.WinnerPlayerID == MatchManager.instance.PlayerID) {
            wonDisplayParent.SetActive(true);
            lostDisplayParent.SetActive(false);
        } else {
            wonDisplayParent.SetActive(false);
            lostDisplayParent.SetActive(true);
        }
    }

    private void OnDisable() {
        wonDisplayParent.SetActive(false);
        lostDisplayParent.SetActive(false);
    }
}
