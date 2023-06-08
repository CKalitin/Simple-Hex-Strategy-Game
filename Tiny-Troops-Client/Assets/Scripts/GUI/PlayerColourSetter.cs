using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using USNL.Package;
using System;

public class PlayerColourSetter : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private Color defaultColor;
    [Space]
    [SerializeField] private int clientID;
    [SerializeField] private bool useIdToIndex = false;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;

    public int ClientID { get => clientID; set => clientID = value; }

    private void OnEnable() { USNL.CallbackEvents.OnPlayerInfoPacket += OnPlayerInfoPacket; }
    private void OnDisable() { USNL.CallbackEvents.OnPlayerInfoPacket -= OnPlayerInfoPacket; }

    private void Start() {
        UpdateColor();
    }

    public void UpdateColor() {
        int playerID = clientID;
        if (useIdToIndex) playerID = GameUtils.IdToIndex(clientID);
        
        Color color = Color.Lerp(new Color(defaultColor.r, defaultColor.g, defaultColor.b, defaultColor.a), PlayerInfoManager.instance.PlayerInfos[playerID].Color, 0.5f);
        if (text) text.color = color;
        if (image) image.color = color;
    }
    
    private void OnPlayerInfoPacket(object _packetObject) {
        USNL.PlayerInfoPacket packet = (USNL.PlayerInfoPacket)_packetObject;

        if (!useIdToIndex && packet.ClientID == clientID) {
            Color color = Color.Lerp(new Color(defaultColor.r, defaultColor.g, defaultColor.b, defaultColor.a), new Color(packet.Color.x, packet.Color.y, packet.Color.z, defaultColor.a), 0.5f);
            if (text) text.color = color;
            if (image) image.color = color;
        } else if (useIdToIndex && packet.ClientID == GameUtils.IdToIndex(clientID)) {
            Color color = Color.Lerp(new Color(defaultColor.r, defaultColor.g, defaultColor.b, defaultColor.a), new Color(packet.Color.x, packet.Color.y, packet.Color.z, defaultColor.a), 0.5f);
            if (text) text.color = color;
            if (image) image.color = color;
        }
    }

    public Color GetColor() {
        if (image) return image.color;
        if (text) return text.color;
        return defaultColor;
    }

    public void SetToDefaultColor() {
        if (text) text.color = defaultColor;
        if (image) image.color = defaultColor;
    }
}
