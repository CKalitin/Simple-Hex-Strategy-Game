using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour {
    #region Variables

    public static GameController instance;

    [SerializeField] private InitialResource[] initialResources;

    private List<int> resourcesChangedOnPlayerId = new List<int>();

    [Serializable]
    public struct InitialResource {
        [SerializeField] private GameResource resource;
        [SerializeField] private int initialSupply;

        public GameResource Resource { get => resource; set => resource = value; }
        public int InitialSupply { get => initialSupply; set => initialSupply = value; }
    }

    #endregion

    #region Core

    private void Awake() {
        Debug.LogError("Opened Console.");

        if (instance == null) instance = this;
        else {
            Debug.Log($"Game Controller instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
    }

    private void Start() {
        USNL.ServerManager.instance.StartServer();

        ToggleResourceManagers(false);
    }

    private void Update() {
        // When a player's resources are changed, send an update
        if (resourcesChangedOnPlayerId.Count > 0) {
            for (int i = 0; i < resourcesChangedOnPlayerId.Count; i++)
                SendResourcesPacket(resourcesChangedOnPlayerId[i], resourcesChangedOnPlayerId[i]);
            resourcesChangedOnPlayerId = new List<int>();
        }
        
        if (MatchManager.instance.MatchState == MatchState.InGame && USNL.ServerManager.GetNumberOfConnectedClients() <= 0)
            MatchManager.instance.ChangeMatchState(MatchState.Ended);
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnClientConnected += OnClientConnected;
        USNL.CallbackEvents.OnClientDisconnected += OnClientDisconnected;
        ResourceManager.OnResourcesChanged += OnResourcesChanged;
        MatchManager.OnMatchStateChanged += OnMatchStateChange;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnClientConnected -= OnClientConnected;
        USNL.CallbackEvents.OnClientDisconnected -= OnClientDisconnected;
        ResourceManager.OnResourcesChanged -= OnResourcesChanged;
        MatchManager.OnMatchStateChanged -= OnMatchStateChange;
    }

    #endregion

    #region Match State

    private void OnMatchStateChange(MatchState _ms) {
        if (_ms == MatchState.Lobby) {
            USNL.ServerManager.instance.AllowNewConnections = true;
        } else if (_ms == MatchState.InGame) {
            USNL.ServerManager.instance.AllowNewConnections = false;
            StartGame();
        } else if (_ms == MatchState.Ended) {
            ResetGame();
            if (USNL.ServerManager.GetNumberOfConnectedClients() <= 0)
                MatchManager.instance.ChangeMatchState(MatchState.Lobby);
        }
    }

    private void StartGame() {
        ToggleResourceManagers(true);

        for (int i = 0; i < USNL.ServerManager.GetConnectedClientIds().Length; i++) {
            SendResourcesPacketToAllClients();
        }

        for (int i = 0; i < ResourceManager.instances.Count; i++) {
            for (int x = 0; x < initialResources.Length; x++) {
                ResourceManager.instances[i].ChangeResource(initialResources[x].Resource, initialResources[x].InitialSupply);
            }
        }
    }

    private void ResetGame() {
        TileManagement.instance.ResetAllTiles();

        // Reset all Resource Managers
        for (int i = 0; i < ResourceManager.instances.Count; i++) {
            ResourceManager.instances[i].ResetResources();
            ResourceManager.instances[i].ResetResourceEntries();
        }

        ToggleResourceManagers(false);
    }

    #endregion

    #region Utils

    private void ToggleResourceManagers(bool _toggle) {
        for (int i = 0; i < ResourceManager.instances.Count; i++)
            ResourceManager.instances[i].enabled = _toggle;
    }

    #endregion

    #region Sending Packets

    private void SendTiles(int _clientId) {
        // Send tiles in batches of 250 because of 4096 byte limit
        int tileCount = TileManagement.instance.GetTiles.Count;
        for (int i = 0; i < tileCount; i++) {
            List<int> tileIds = new List<int>();
            List<Vector2> tileLocations = new List<Vector2>();
            
            for (int x = i * 250; x < 250 & x < tileCount; x++) {
                var item = TileManagement.instance.GetTiles.ElementAt(x);
                tileIds.Add((int)item.Value.TileId - 1);
                tileLocations.Add((Vector2)item.Key);
            }

            USNL.PacketSend.Tiles(_clientId, tileIds.ToArray(), tileLocations.ToArray());
        }  
    }
    
    public void SendTilesToAllClients() {
        int[] connectedClientIds = USNL.ServerManager.GetConnectedClientIds();
        for (int i = 0; i < connectedClientIds.Length; i++) {
            SendTiles(connectedClientIds[i]);
        }
    }

    private void SendResourcesPacket(int _toClient, int _playerID) {
        float[] supplys = new float[ResourceManager.instances[_playerID].Resources.Length];
        float[] demands = new float[ResourceManager.instances[_playerID].Resources.Length];

        for (int i = 0; i < ResourceManager.instances[_playerID].Resources.Length; i++) {
            supplys[i] = ResourceManager.instances[_playerID].Resources[i].Supply;
            demands[i] = ResourceManager.instances[_playerID].Resources[i].Demand;
        }

        USNL.PacketSend.Resources(_toClient, _playerID, supplys, demands);
    }

    public void SendResourcesPacketToAllClients() {
        int[] connectedClientIds = USNL.ServerManager.GetConnectedClientIds();
        for (int i = 0; i < connectedClientIds.Length; i++) {
            SendResourcesPacket(connectedClientIds[i], connectedClientIds[i]);
        }
    }

    private void SendStructures(int _toClient) {
        Structure[] structures = FindObjectsOfType<Structure>();

        for (int i = 0; i < structures.Length; i++) {
            USNL.PacketSend.BuildStructure(_toClient, structures[i].PlayerID, structures[i].Tile.TileInfo.Location, structures[i].StructureID);
        }
    }
    
    public void SendStructuresToAllClients() {
        int[] connectedClientIds = USNL.ServerManager.GetConnectedClientIds();
        for (int i = 0; i < connectedClientIds.Length; i++) {
            SendStructures(connectedClientIds[i]);
        }
    }

    #endregion

    #region Callbacks

    private void OnClientConnected(object _clientIdObject) {
        int clientId = (int)_clientIdObject;
        
        // Send all Tiles, Resources, and Structures
        SendTiles(clientId);
        SendResourcesPacket(clientId, clientId);
        SendStructures(clientId);
    }

    private void OnClientDisconnected(object _clientIdObject) {
        int clientId = (int)_clientIdObject;
    }

    private void OnResourcesChanged(int _playerId) {
        if (_playerId < 0) return;
        if (resourcesChangedOnPlayerId.Contains(_playerId)) return;
        
        resourcesChangedOnPlayerId.Add(_playerId);
    }

    #endregion
}
