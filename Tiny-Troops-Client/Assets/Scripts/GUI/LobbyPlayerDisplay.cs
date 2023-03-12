using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyPlayerDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private GameObject togglableReady;

    public TextMeshProUGUI UsernameText { get => usernameText; set => usernameText = value; }
    public GameObject TogglableReady { get => togglableReady; set => togglableReady = value; }
}
