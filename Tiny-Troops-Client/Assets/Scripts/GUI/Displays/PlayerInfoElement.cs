using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInfoElement : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI idText;
    [Space]
    [SerializeField] private TextMeshProUGUI usernameText;
    [Space]
    [SerializeField] private TextMeshProUGUI scoreText;
        
    public void SetInfo(USNL.PlayerInfoPacket _packet) {
        if (idText != null) idText.text = _packet.ClientID.ToString();
        if (usernameText != null) usernameText.text = _packet.Username;
        if (scoreText != null) scoreText.text = _packet.Score.ToString();
    }
}
