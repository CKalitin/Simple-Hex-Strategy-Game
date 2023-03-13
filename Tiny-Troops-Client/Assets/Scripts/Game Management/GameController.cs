using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    #region Variables

    public static bool ApplicationQuitting = false;

    [Header("Connection")]
    [SerializeField] private bool connectOnStart = true;
    [Space]
    [SerializeField] private int port;

    [Header("Game")]
    [Tooltip("In order of Tile ID")]
    [SerializeField] private GameObject[] tilePrefabs;

    #endregion

    #region Core

    private void Awake() {
        Application.runInBackground = true;
        Debug.LogError("Opened Console.");
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
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnConnected -= OnConnected;
        USNL.CallbackEvents.OnTilesPacket -= OnTilesPacket;
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
    }

    #endregion
}
