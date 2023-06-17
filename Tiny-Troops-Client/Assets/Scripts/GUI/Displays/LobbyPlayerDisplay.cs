using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyPlayerDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private GameObject togglableReady;
    [Space]
    [Tooltip("If the player's username skips to the next line, the next ready position is used.")]
    [SerializeField] Transform[] togglableReadyPositions;

    public TextMeshProUGUI UsernameText { get => usernameText; set => usernameText = value; }
    public GameObject TogglableReady { get => togglableReady; set => togglableReady = value; }

    private void Start() {
        togglableReady.transform.parent.position = togglableReadyPositions[0].position;
        if (usernameText.text.Length > 12) togglableReady.transform.parent.position = togglableReadyPositions[1].position;
    }
}
