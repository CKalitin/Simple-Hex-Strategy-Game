using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using TMPro;

public class PortField : MonoBehaviour {
    [SerializeField] private TMP_InputField portField;

    private int previousPort;

    private void Awake() {
        if (PlayerPrefs.HasKey("PortId")) {
            portField.text = PlayerPrefs.GetInt("PortId").ToString();
            previousPort = PlayerPrefs.GetInt("PortId");
        }
    }

    private void Update() {
        UpdateId();
    }
    
    private void UpdateId() {
        int port = portField.text.Length > 0 ? int.Parse(portField.text) : 0;

        if (port != previousPort) {
            previousPort = port;
            PlayerPrefs.SetInt("PortId", port);
        }
    }
}
