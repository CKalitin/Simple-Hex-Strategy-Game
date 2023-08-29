using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using USNL;

public class GameController : MonoBehaviour {
    #region Variables

    public static GameController instance;

    [SerializeField] private InitialResource[] initialResources;
    [Space]
    [Tooltip("For initial buildings Resoucre Entries to work they need to be rebuild on Game Start.")]
    [SerializeField] private GameObject[] playerBaseTiles;

    private List<int> resourcesChangedOnPlayerId = new List<int>();

    private int winnerPlayerID = -1;

    public int WinnerPlayerID { get => winnerPlayerID; set => winnerPlayerID = value; }

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
        //Debug.LogError("Opened Console.");

        if (instance == null) instance = this;
        else {
            Debug.Log($"Game Controller instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
        
        USNL.ServerManager.instance.AllowNewConnections = false;
        StartCoroutine(AllowConnectionsDelay());
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

        if (MatchManager.instance.MatchState == MatchState.InGame) CheckGameCompleted();
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
            Lobby();
        } else if (_ms == MatchState.InGame) {
            USNL.ServerManager.instance.AllowNewConnections = false;
            StartGame();
        } else if (_ms == MatchState.Ended) {
            ResetGame();
            MatchManager.instance.ChangeMatchState(MatchState.Lobby);
        }
    }

    private void Lobby() {
        if (TileManagement.instance.GetTiles.Count <= 0)
            TestTileGenerator.instance.GenerateTiles();

        RebuildPlayerBases();
        //SendStructuresToAllClients();
    }

    private void StartGame() {
        ToggleResourceManagers(true);
        
        RebuildPlayerBases();
        SendStructuresToAllClients();

        PlayerInfoManager.instance.ResetPlayerReady();
        PlayerInfoManager.instance.ResetPlayerScore();
        
        for (int i = 0; i < ResourceManager.instances.Count; i++) {
            ResourceManager.instances[i].ResetResources();
            ResourceManager.instances[i].ResetResourceEntries();
        }
        
        for (int i = 0; i < ResourceManager.instances.Count; i++) {
            for (int x = 0; x < initialResources.Length; x++) {
                ResourceManager.instances[i].ChangeResource(initialResources[x].Resource, initialResources[x].InitialSupply);
            }
        }
        
        SendResourcesPacketToAllClients();

        // Allow current clients to reconnect, but not new ones
        int[] connectedClientIDs = ServerManager.GetConnectedClientIds();
        for (int i = 0; i < connectedClientIDs.Length; i++) {
            USNL.ServerManager.instance.AllowReconnectionIPs.Add(USNL.Package.Server.Clients[connectedClientIDs[i]].IpAddress.Split(':')[0]);
        }
    }

    private void ResetGame() {
        winnerPlayerID = -1;

        TileManagement.instance.ResetAllTiles();

        // Reset all Resource Managers
        for (int i = 0; i < ResourceManager.instances.Count; i++) {
            ResourceManager.instances[i].ResetResources();
            ResourceManager.instances[i].ResetResourceEntries();
        }

        ToggleResourceManagers(false);

        UnitManager.instance.DestroyAllUnits();
        UnitAttackManager.instance.ResetManager();

        VillagerManager.instance.ResetVillagerManager();

        USNL.ServerManager.instance.AllowReconnectionIPs = new List<string>();
    }

    #endregion

    #region Utils

    private IEnumerator AllowConnectionsDelay() {
        yield return new WaitForSeconds(1f);
        USNL.ServerManager.instance.AllowNewConnections = true;
    }

    private void CheckGameCompleted() {
        List<Structure> playerBases = new List<Structure>();
        
        // Loop through all tiles and find Bases
        foreach (TileInfo tile in TileManagement.instance.GetTiles.Values) {
            if (tile.Tile.Structures.Count <= 0) continue;
            if (!tile.Tile.Structures[0].GetComponent<PlayerBase>()) continue;
            
            bool playerAlreadyAdded = false;
            for (int i = 0; i < playerBases.Count; i++) {
                if (tile.Tile.Structures[0].PlayerID == playerBases[i].PlayerID) playerAlreadyAdded = true;
            }
            if (!playerAlreadyAdded) playerBases.Add(tile.Tile.Structures[0]);
        }

        if (playerBases.Count == 1) {
            winnerPlayerID = playerBases[0].PlayerID;
            USNL.PacketSend.GameEnded(WinnerPlayerID);
            MatchManager.instance.ChangeMatchState(MatchState.Ended);
        } else if (playerBases.Count <= 0){
            winnerPlayerID = -1;
            USNL.PacketSend.GameEnded(WinnerPlayerID);
            MatchManager.instance.ChangeMatchState(MatchState.Ended);
        }
    }

    private void ToggleResourceManagers(bool _toggle) {
        for (int i = 0; i < ResourceManager.instances.Count; i++)
            ResourceManager.instances[i].enabled = _toggle;
    }

    private void RebuildPlayerBases() {
        // Loop through all tiles and find Bases
        List<TileInfo> tiles = TileManagement.instance.GetTiles.Values.ToList();
        for (int i = 0; i < tiles.Count; i++) {
            if (tiles[i].Tile.Structures.Count <= 0) continue;
            if (!tiles[i].Tile.Structures[0].GetComponent<PlayerBase>()) continue;

            int playerID = tiles[i].Tile.Structures[0].PlayerID;
            Vector2Int location = tiles[i].Location;
            TileManagement.instance.DestroyTile(location);
            TileManagement.instance.SpawnTile(playerBaseTiles[playerID], location);
            
            ServerStructureBuilder.instance.SendBuildStructurePacketToAllClients(playerID, location, (int)playerBaseTiles[playerID].GetComponent<Tile>().Structures[0].StructureID);
        }
    }

    #endregion

    #region Sending Packets

    private void SendTiles(int _clientId) {
        // Send tiles in batches of 250 because of 4096 byte limit
        int tileCount = TileManagement.instance.GetTiles.Count;
        
        for (int i = 0; i < Mathf.CeilToInt(tileCount / 250f); i++) {
            List<int> tileIds = new List<int>();
            List<Vector2> tileLocations = new List<Vector2>();
            
            for (int x = i * 250; x < (i + 1) * 250 & x < tileCount; x++) {
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
            USNL.PacketSend.BuildStructure(_toClient, structures[i].PlayerID, structures[i].Tile.TileInfo.Location, (int)structures[i].StructureID);
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

        USNL.PacketSend.MatchUpdate(clientId, (int)MatchManager.instance.MatchState);
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
