using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTileGenerator : MonoBehaviour {
    [SerializeField] private TilePrefabsArray[] tilePrefabsMatrix;
    
    [System.Serializable]
    public struct TilePrefabsArray {
        [SerializeField] public GameObject[] tilePrefabs;
    }
    
    void Start() {
        GenerateTiles();
    }
    
    private void GenerateTiles() {
        for (int x = 0; x < tilePrefabsMatrix.Length; x++) {
            for (int y = 0; y < tilePrefabsMatrix[x].tilePrefabs.Length; y++) {
                TileManagement.instance.SpawnTile(tilePrefabsMatrix[x].tilePrefabs[y], new Vector2Int(x, y));
            }
        }

        // NECCESSARY TILE SPAWNING STUFF COPY THIS TO FINAL TILE GENERATION CODE (IN THIS ORDER)
        TileManagement.instance.SpawningComplete = true;
        TileManagement.instance.ApplyTileRules();
        TileManagement.instance.ApplyResourceModifiersOnAllTiles();
        // NECCESSARY TILE SPAWNING STUFF COPY THIS TO FINAL TILE GENERATION CODE (IN THIS ORDER)
    }
}
