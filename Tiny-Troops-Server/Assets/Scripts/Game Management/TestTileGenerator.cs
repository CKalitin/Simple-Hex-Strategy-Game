using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTileGenerator : MonoBehaviour {
    public static TestTileGenerator instance;

    [SerializeField] private TilePrefabsArray[] tilePrefabsMatrix;
    
    [System.Serializable]
    public struct TilePrefabsArray {
        [SerializeField] public GameObject[] tilePrefabs;
    }

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (instance == null) instance = this;
        else {
            Debug.Log($"Test Tile Generator instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    void Start() {
        GenerateTiles();
    }
    
    public void GenerateTiles() {
        PathfindingManager.instance.PathfindingLocationsMap = new Dictionary<Vector2Int, PathfindingLocation>();

        for (int x = 0; x < tilePrefabsMatrix.Length; x++) {
            for (int y = 0; y < tilePrefabsMatrix[x].tilePrefabs.Length; y++) {
                TileManagement.instance.SpawnTile(tilePrefabsMatrix[x].tilePrefabs[y], new Vector2Int(x, y));
            }
        }

        // NECCESSARY TILE SPAWNING STUFF COPY THIS TO FINAL TILE GENERATION CODE (IN THIS ORDER)
        TileManagement.instance.SpawningComplete = true;
        TileManagement.instance.ApplyTileRules();
        // NECCESSARY TILE SPAWNING STUFF COPY THIS TO FINAL TILE GENERATION CODE (IN THIS ORDER)

        GameController.instance.SendTilesToAllClients();
    }
}
