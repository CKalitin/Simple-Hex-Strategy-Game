using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UsernameField : MonoBehaviour {
    [SerializeField] private TMP_InputField usernameField;

    string previousUsername;

    private void Awake() {
        if (PlayerPrefs.HasKey("Username")) {
            usernameField.text = PlayerPrefs.GetString("Username");
        }
    }

    private void Update() {
        if (usernameField.text != previousUsername) {
            previousUsername = usernameField.text;
            PlayerPrefs.SetString("Username", usernameField.text);
        }
    }
}
