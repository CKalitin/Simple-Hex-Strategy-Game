using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInfoDisplay : MonoBehaviour {
    [SerializeField] private DisplayType displayType;
    [Space]
    [SerializeField] private GameObject playerInfoElementPrefab;
    [SerializeField] private Transform playerInfoElementsParent;
    [SerializeField] private float ySpacing;
    [Space]
    [SerializeField] private int maxElements;
    
    private enum DisplayType {
        SortByID,
        SortByScore
    }

    private Dictionary<int, Transform> playerInfoElements = new Dictionary<int, Transform>();

    private void Update() {
        if (!USNL.ClientManager.instance.IsConnected) return;

        CheckForDisconnectedClients(USNL.ClientManager.instance.ServerInfo.ConnectedClientIds);
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnPlayerInfoPacket += OnPlayerInfoPacket;
    }
    
    private void OnDisable() {
        USNL.CallbackEvents.OnPlayerInfoPacket -= OnPlayerInfoPacket;
    }

    private void DisplayPlayerInfos() {
        int[] connectedClientIds = USNL.ClientManager.instance.ServerInfo.ConnectedClientIds;

        List<PlayerInfo> pis = PlayerInfoManager.instance.PlayerInfos;

        int[] playerScores = new int[connectedClientIds.Length];
        for (int i = 0; i < playerScores.Length; i++) {
            if (connectedClientIds.Contains(i)) playerScores[i] = pis[i].Score;
        }
        
        if (displayType == DisplayType.SortByID) {
            DisplayPlayerInfos(connectedClientIds);
        } else if (displayType == DisplayType.SortByScore) {
            int[] ids = connectedClientIds.OrderBy(x => -playerScores[x]).ToArray();
            DisplayPlayerInfos(ids);
        }
    }

    private void DisplayPlayerInfos(int[] indexes) {
        int count = 0;
        for (int i = 0; i < indexes.Length; i++) {
            if (count >= maxElements) return;

            int x = indexes[i];
            if (playerInfoElements.ContainsKey(x)) {
                playerInfoElements[x].localPosition = new Vector3(0, -count * ySpacing, 0);
                count++;
            } else {
                playerInfoElements.Add(x, Instantiate(playerInfoElementPrefab, playerInfoElementsParent).transform);
                playerInfoElements[x].localPosition = Vector3.zero;
                playerInfoElements[x].localPosition = new Vector3(0, -count * ySpacing, 0);
                count++;
            }
        }
    }
    
    private void CheckForDisconnectedClients(int[] connectedClientIds) {
        for (int i = 0; i < playerInfoElements.Count; i++) {
            if (!connectedClientIds.Contains(i) && playerInfoElements.ContainsKey(i)) {
                if (playerInfoElements[i].gameObject != null) Destroy(playerInfoElements[i].gameObject);
                playerInfoElements.Remove(i);
            }
        }
    }
    
    private void OnPlayerInfoPacket(object _packetObject) {
        USNL.PlayerInfoPacket packet = (USNL.PlayerInfoPacket)_packetObject;

        DisplayPlayerInfos();

        if (playerInfoElements.ContainsKey(packet.ClientID)) {
            PlayerInfoElement playerInfoElement = playerInfoElements[packet.ClientID].GetComponent<PlayerInfoElement>();
            playerInfoElement.SetInfo(packet);
        }
    }
}
