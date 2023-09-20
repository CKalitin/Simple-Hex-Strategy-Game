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

    private float defaultPreferredHeight;

    public TextMeshProUGUI UsernameText { get => usernameText; set => usernameText = value; }
    public GameObject TogglableReady { get => togglableReady; set => togglableReady = value; }

    private void Awake() {
        usernameText.text = "‎";
        defaultPreferredHeight = usernameText.preferredHeight;
    }

    private void Start() {
        Debug.Log(usernameText.preferredHeight + ", " + defaultPreferredHeight + ", " + Mathf.FloorToInt(usernameText.preferredHeight / (defaultPreferredHeight - 1) - 1));
        int spacingLines = Mathf.FloorToInt(usernameText.preferredHeight / (defaultPreferredHeight - 1) - 1);

        togglableReady.transform.parent.position = togglableReadyPositions[spacingLines].position;
    }
}
