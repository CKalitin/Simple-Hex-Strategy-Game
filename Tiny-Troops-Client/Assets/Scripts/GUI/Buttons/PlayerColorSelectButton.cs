using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class PlayerColorSelectButton : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;

    private Color[] colorOptions;
    private int index;

    private bool firstPlayerInfoPacketReceived = false;
    private bool delayedColourPickExecuted = false;

    private void Start() {
        colorOptions = ColorManager.instance.Colors;
        
        //if (PlayerInfoManager.instance.PlayerInfos.Count > MatchManager.instance.PlayerID) GetNextAvailableColor();
    }

    private void OnEnable() { USNL.CallbackEvents.OnPlayerInfoPacket += OnPlayerInfoPacket; }
    private void OnDisable() { USNL.CallbackEvents.OnPlayerInfoPacket -= OnPlayerInfoPacket; }

    public void ButtonDown() {
        index++;
        if (index >= colorOptions.Length) index = 0;
        GetNextAvailableColor();
    }

    private void OnPlayerInfoPacket(object _packetObject) {
        USNL.PlayerInfoPacket packet = (USNL.PlayerInfoPacket)_packetObject;

        if (!firstPlayerInfoPacketReceived && packet.Username == PlayerPrefs.GetString("Username", "")) {
            firstPlayerInfoPacketReceived = true;
            StartCoroutine(DelayedColourPick());
            return;
        }
        if (!delayedColourPickExecuted) return;

        Color color = new Color(packet.Color.x, packet.Color.y, packet.Color.z);
        bool desync = false;

        if (packet.ClientID == MatchManager.instance.PlayerID && color != colorOptions[index]) { 
            Debug.Log("Desync 1");
            desync = true; 
        }
        if (packet.ClientID != MatchManager.instance.PlayerID && color == colorOptions[index]) {
            Debug.Log("Desync 2");
            desync = true;
        }

        if (desync) {
            GetNextAvailableColor();
            return;
        }
    }

    private void GetNextAvailableColor() {
        for (int i = index; i < colorOptions.Length + index; i++) {
            if (!CheckColorTaken(i % colorOptions.Length)) {
                index = i % colorOptions.Length;
                text.color = colorOptions[index];
                USNL.PacketSend.PlayerSetupInfo(PlayerInfoManager.instance.PlayerInfos[MatchManager.instance.PlayerID].Username, new Vector3(colorOptions[index].r, colorOptions[index].g, colorOptions[index].b));
                return;
            }
        }
    }

    private bool CheckColorTaken(int _index) {
        for (int i = 0; i < USNL.ClientManager.instance.ServerInfo.ConnectedClientIds.Length; i++) {
            if (colorOptions[_index] == PlayerInfoManager.instance.PlayerInfos[USNL.ClientManager.instance.ServerInfo.ConnectedClientIds[i]].Color) return true;
        }
        return false;
    }

    private IEnumerator DelayedColourPick() {
        yield return new WaitForSeconds(0.1f);
        
        GetNextAvailableColor();
        delayedColourPickExecuted = true;
    }
}
