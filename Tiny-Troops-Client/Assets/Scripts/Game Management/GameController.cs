using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    #region Variables

    public static GameController instance;

    public static bool ApplicationQuitting = false;

    [Header("Connection")]
    [SerializeField] private bool connectOnStart = true;
    [Space]
    [SerializeField] private int port;
    
    [Header("Game")]
    [Tooltip("In order of Tile ID")]
    [SerializeField] private GameObject[] tilePrefabs;
    [Space]
    [SerializeField] private int numTilesToExpect = 400;
    // I don't know either, goddamn tech debt
    [SerializeField] private float gameInitalizedCallbackDelay;

    private bool gameReady = false;

    private bool tilesPacketReceived = false;
    private bool resourcesPacketReceived = false;

    private int winnerPlayerID = -1;

    public delegate void GameInitializedCallback();
    public static event GameInitializedCallback OnGameInitialized;

    public int WinnerPlayerID { get => winnerPlayerID; set => winnerPlayerID = value; }

    #endregion

    #region Core

    private void Awake() {
        Application.runInBackground = true;
        Singleton();
        Debug.LogError("Opened Console.");
    }

    private void Singleton() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Game Controller instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    private void Start() {
        if (connectOnStart) USNL.ClientManager.instance.ConnectToServer(PlayerPrefs.GetInt("HostId"), port);
    }

    private void OnApplicationQuit() {
        ApplicationQuitting = true;
    }
    
    private void OnEnable() {
        USNL.CallbackEvents.OnConnected += OnConnected;
        USNL.CallbackEvents.OnTilesPacket += OnTilesPacket;
        USNL.CallbackEvents.OnResourcesPacket += OnResourcesPacket;
        USNL.CallbackEvents.OnGameEndedPacket += OnGameEndedPacket;
        MatchManager.OnMatchStateChanged += OnMatchStateChanged;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnConnected -= OnConnected;
        USNL.CallbackEvents.OnTilesPacket -= OnTilesPacket;
        USNL.CallbackEvents.OnResourcesPacket -= OnResourcesPacket;
        USNL.CallbackEvents.OnGameEndedPacket -= OnGameEndedPacket;
        MatchManager.OnMatchStateChanged -= OnMatchStateChanged;
    }

    #endregion

    #region Callbacks

    private void OnConnected(object _object) {
        USNL.PacketSend.PlayerSetupInfo(PlayerPrefs.GetString("Username", "Username"));
    }

    private void OnTilesPacket(object _packetObject) {
        USNL.TilesPacket tilesPacket = (USNL.TilesPacket)_packetObject;
        
        for (int i = 0; i < tilesPacket.TileIDs.Length; i++) {
            if (TileManagement.instance.GetTileAtLocation(Vector2Int.RoundToInt(tilesPacket.TileLocations[i])).Spawned)
                TileManagement.instance.DestroyTile(Vector2Int.RoundToInt(tilesPacket.TileLocations[i]));
            
            TileManagement.instance.SpawnTile(tilePrefabs[tilesPacket.TileIDs[i]], Vector2Int.RoundToInt(tilesPacket.TileLocations[i]));
        }
        
        if (numTilesToExpect >= TileManagement.instance.GetTiles.Count) {
            tilesPacketReceived = true;
            CheckGameReady();
        }
    }

    private void OnResourcesPacket(object _packetObject) {
        USNL.ResourcesPacket resourcesPacket = (USNL.ResourcesPacket)_packetObject;
        
        for (int i = 0; i < ResourceManager.instances[resourcesPacket.PlayerID].Resources.Length; i++) {
            ResourceManager.instances[resourcesPacket.PlayerID].Resources[i].Supply = resourcesPacket.Supplys[i];
            ResourceManager.instances[resourcesPacket.PlayerID].Resources[i].Demand = resourcesPacket.Demands[i];
        }
        
        resourcesPacketReceived = true;
        CheckGameReady();
    }

    private IEnumerator CallOnGameInitialized() {
        yield return new WaitForSeconds(gameInitalizedCallbackDelay);
        OnGameInitialized();
    }
    
    private void OnMatchStateChanged(MatchState _matchState) {
        if (_matchState == MatchState.InGame) {
            winnerPlayerID = -1;
        } else if (_matchState == MatchState.Ended) {
            tilesPacketReceived = false;
            resourcesPacketReceived = false;
            gameReady = false;
            ResetGame();
        }
    }

    private void OnGameEndedPacket(object _packetObject) {
        USNL.GameEndedPacket packet = (USNL.GameEndedPacket)_packetObject;
        winnerPlayerID = packet.WinnerPlayerID;
    }

    #endregion

    #region Utils

    private void CheckGameReady() {
        if (tilesPacketReceived && resourcesPacketReceived) {
            if (!gameReady && OnGameInitialized != null) {
                gameReady = true;
                StartCoroutine(CallOnGameInitialized());
            }
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

        UnitManager.instance.DestroyAllUnits();
        UnitAttackManager.instance.ResetManager();
    }

    #endregion
}
