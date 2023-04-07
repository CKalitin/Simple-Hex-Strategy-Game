using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour {
    #region Variables

    [Header("Spawner")]
    [SerializeField] private PathfindingNode spawnPathfindingNode;
    [Space]
    [SerializeField] private GameObject[] unitPrefabs;

    [Header("Structure")]
    [SerializeField] private GameplayStructure gameplayStructure;

    private Tile tile;
    private Vector2Int location;

    #endregion

    #region Core

    private void Start() {
        GetTileParent();
        location = tile.TileInfo.Location;

        SpawnUnit(0, 0);
    }

    private void OnEnable() { gameplayStructure.OnStructureAction += SpawnUnit; }
    private void OnDisable() { gameplayStructure.OnStructureAction -= SpawnUnit; }

    #endregion

    #region Spawning

    private void GetTileParent() {
        Transform t = transform.parent;
        while (true) {
            if (t.GetComponent<Tile>()) {
                // If tile script found
                tile = t.GetComponent<Tile>();
                break;
            } else {
                // If we're at the top of the heirarchy, break out of the loop.
                if (t.parent == null) break;
                t = t.parent;
            }
        }
    }

    // ActionID is the UnitID
    // UnitID is ActionID - 1000 so other building scripts on the same tile can have different actions
    public void SpawnUnit(int _playerID, int _unitID) {
        _unitID -= 1000;
        Vector3 pos = spawnPathfindingNode.transform.position + (Vector3.one * Random.Range(-spawnPathfindingNode.Radius, spawnPathfindingNode.Radius));
        pos.y = spawnPathfindingNode.transform.position.y;
        GameObject unit = Instantiate(unitPrefabs[_unitID], pos, Quaternion.identity);
        unit.GetComponent<PathfindingAgent>().Initialize(location, spawnPathfindingNode);
        unit.GetComponent<Unit>().PlayerID = _playerID;
    }

    #endregion
}
