using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyPlayerDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private GameObject togglableReady;
    [SerializeField] private float togglableReadySpacing = 150f;

    private float togglableReadyDefaultY;

    public TextMeshProUGUI UsernameText { get => usernameText; set => usernameText = value; }
    public GameObject TogglableReady { get => togglableReady; set => togglableReady = value; }

    private void Start() {
        togglableReadyDefaultY = togglableReady.transform.parent.position.y;
    }

    private void Update() {
        if (usernameText.text.Length > 12) {
            togglableReady.transform.parent.position = new Vector3(togglableReady.transform.position.x, togglableReadyDefaultY - togglableReadySpacing, togglableReady.transform.position.z);
        }
    }
}
