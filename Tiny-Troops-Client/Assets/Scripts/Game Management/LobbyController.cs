using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using USNL.Package;

public class LobbyController : MonoBehaviour {
    #region Variables

    //public static LobbyController instance;

    [SerializeField] private List<LobbyPlayerDisplay> playerDisplays;

    // For some reason playerDisplays is null inside OnPlayerInfoPacket on the second load of the game scene. But in Update it's fine. 
    private List<USNL.PlayerInfoPacket> unhandledPlayerInfoPackets = new List<USNL.PlayerInfoPacket>();

    // This is a list of the last time a player disconnect packet (Player Setup Info with username of "") was received to stop the display from flashing
    private Dictionary<int, float> lastReceivedDisconnectByPlayerID = new Dictionary<int, float>();

    #endregion

    #region Core

    private void Awake() {
        /*if (instance == null) instance = this;
        else {
            Debug.Log($"Lobby Controller instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }*/

        // Set all player displays to inactive
        for (int i = 0; i < playerDisplays.Count; i++) playerDisplays[i].gameObject.SetActive(false);
    }

    private void Update() {
        if (unhandledPlayerInfoPackets.Count > 0) {
            for (int i = 0; i < unhandledPlayerInfoPackets.Count; ) {
                HandlePlayerInfoPacket(unhandledPlayerInfoPackets[i]);
                unhandledPlayerInfoPackets.RemoveAt(0);
            }
        }
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnPlayerReadyPacket += OnPlayerReadyPacket;
        USNL.CallbackEvents.OnPlayerInfoPacket += OnPlayerInfoPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnPlayerReadyPacket -= OnPlayerReadyPacket;
        USNL.CallbackEvents.OnPlayerInfoPacket += OnPlayerInfoPacket;
    }

    #endregion

    #region Lobby Controller

    private void HandlePlayerInfoPacket(USNL.PlayerInfoPacket packet) {
        int id = GameUtils.IdToIndex(packet.ClientID);
        
        // Check Client disconnected
        // This stops it from flashing quickly, tech debt
        if (packet.Username == "" && lastReceivedDisconnectByPlayerID.ContainsKey(id) && Time.timeSinceLevelLoad - lastReceivedDisconnectByPlayerID[id] < 1f) {
            if (lastReceivedDisconnectByPlayerID.ContainsKey(id)) lastReceivedDisconnectByPlayerID[id] = Time.timeSinceLevelLoad;
            else lastReceivedDisconnectByPlayerID.Add(id, Time.timeSinceLevelLoad);

            return;
        }

        // Real Check Client disconnected
        if (packet.Username == "") {
            if (lastReceivedDisconnectByPlayerID.ContainsKey(id)) lastReceivedDisconnectByPlayerID[id] = Time.timeSinceLevelLoad;
            else lastReceivedDisconnectByPlayerID.Add(id, Time.timeSinceLevelLoad);
            
            OnClientDisconnected(packet.ClientID);
            return;
        }

        // If player display is not active, activate it. Then update Username
        if (playerDisplays[id].gameObject.activeSelf == false) SetPlayerDisplayActive(id, true);
        playerDisplays[id].UsernameText.text = packet.Username;
    }
    
    private void SetPlayerDisplayActive(int id, bool _active) {
        playerDisplays[id].UsernameText.text = "";
        playerDisplays[id].TogglableReady.SetActive(false);
        playerDisplays[id].gameObject.SetActive(_active);
    }

    #endregion

    #region Callbacks

    private void OnPlayerReadyPacket(object _packetObject) {
        USNL.PlayerReadyPacket packet = (USNL.PlayerReadyPacket)_packetObject;
        int id = GameUtils.IdToIndex(packet.ClientID);
        
        // If player display is not active, activate it. Then update Ready display
        //if (playerDisplays[id].gameObject.activeSelf == false) SetPlayerDisplayActive(id, true);
        if (playerDisplays[id].gameObject.activeSelf != false) playerDisplays[id].TogglableReady.SetActive(packet.Ready);
    }

    private void OnPlayerInfoPacket(object _packetObject) {
        unhandledPlayerInfoPackets.Add((USNL.PlayerInfoPacket)_packetObject);
    }

    private void OnClientDisconnected(int _clientID) {
        int id = GameUtils.IdToIndex(_clientID);
        playerDisplays[id].gameObject.SetActive(false);
    }

    #endregion
}
