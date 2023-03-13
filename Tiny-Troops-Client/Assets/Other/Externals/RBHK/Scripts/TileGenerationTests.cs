using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerationTests : MonoBehaviour {
    #region Variables

    [Header("Test Generation 1")]
    [SerializeField] private GameObject tilePrefab;

    [Header("Test Generation 2")]
    [SerializeField] private GameObject[] tilePrefabs;

    [Header("Test Generation 3")]
    [SerializeField] private TilePrefabsArray[] tilePrefabsMatrix;

    [System.Serializable]
    public struct TilePrefabsArray {
        [SerializeField] public GameObject[] tilePrefabs;
    }

    #endregion

    void Start() {
        TestGeneration3();
    }

    #region Test Generation 3

    private void TestGeneration3() {
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

    #endregion

    #region Test Generation 2

    private void TestGeneration2() {
        for (int i = 0; i < tilePrefabs.Length; i++) {
            TileManagement.instance.SpawnTile(tilePrefabs[i], new Vector2Int(i, 0));
        }
        TileManagement.instance.SpawnTile(tilePrefabs[0], new Vector2Int(1, 1));

        // NECCESSARY TILE SPAWNING STUFF COPY THIS TO FINAL TILE GENERATION CODE (IN THIS ORDER)
        TileManagement.instance.SpawningComplete = true;
        TileManagement.instance.ApplyTileRules();
        TileManagement.instance.ApplyResourceModifiersOnAllTiles();
        // NECCESSARY TILE SPAWNING STUFF COPY THIS TO FINAL TILE GENERATION CODE (IN THIS ORDER)
    }

    #endregion

    #region Test Generation 1

    private void TestGeneration1() {
        GenerateTiles1();

        TileManagement.instance.ApplyTileRules();

        List<Vector2Int> tileLocs = TileManagement.instance.GetAdjacentTilesInRadius(new Vector2Int(12, 12), 3);
        tileLocs.AddRange(TileManagement.instance.GetAdjacentTilesInRadius(new Vector2Int(7, 7), 4));

        StartCoroutine(DestroyTilesDelayed1(tileLocs));
        //DestroyTiles1(tileLocs);
    }


    private void GenerateTiles1() {
        for (int x = 0; x < 21; x++) {
            for (int y = 0; y < 21; y++) {
                Vector2Int tileLoc = TileManagement.instance.SpawnTile(tilePrefab, new Vector2Int(x, y));
            }
        }
    }

    private void DestroyTiles1(List<Vector2Int> tileLocs) {
        for (int i = 0; i < tileLocs.Count; i++) {
            TileManagement.instance.DestroyTile(tileLocs[i]);
        }
        // NECCESSARY TILE SPAWNING STUFF COPY THIS TO FINAL TILE GENERATION CODE (IN THIS ORDER)
        TileManagement.instance.SpawningComplete = true;
        TileManagement.instance.ApplyTileRules();
        TileManagement.instance.ApplyResourceModifiersOnAllTiles();
        // NECCESSARY TILE SPAWNING STUFF COPY THIS TO FINAL TILE GENERATION CODE (IN THIS ORDER)
    }

    private IEnumerator DestroyTilesDelayed1(List<Vector2Int> tileLocs) {
        float waitTime = 0;// .025f;

        //yield return new WaitForSeconds(2);

        for (int i = 0; i < tileLocs.Count; i++) {
            yield return new WaitForSeconds(waitTime);
            TileManagement.instance.DestroyTile(tileLocs[i]);
        }
        //yield return new WaitForSeconds(0.1f);

        // NECCESSARY TILE SPAWNING STUFF COPY THIS TO FINAL TILE GENERATION CODE (IN THIS ORDER)
        TileManagement.instance.SpawningComplete = true;
        TileManagement.instance.ApplyTileRules();
        TileManagement.instance.ApplyResourceModifiersOnAllTiles();
        // NECCESSARY TILE SPAWNING STUFF COPY THIS TO FINAL TILE GENERATION CODE (IN THIS ORDER)
    }


    #endregion
}
