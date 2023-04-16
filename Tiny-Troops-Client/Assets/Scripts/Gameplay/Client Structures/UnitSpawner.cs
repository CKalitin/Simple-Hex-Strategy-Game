using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour {
    #region Variables

    [Header("Spawner")]
    [SerializeField] private PathfindingNode spawnPathfindingNode;

    [Header("Structure")]
    [SerializeField] private GameplayStructure gameplayStructure;

    private Tile tile;

    public Vector2Int Location { get => tile.TileInfo.Location; }

    #endregion

    #region Core

    private void Awake() {
        GetTileParent();
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
    public void SpawnUnit(int _playerID, int _unitID, int[] _configurationInts) {
        _unitID -= 1000;

        Vector3 pos = spawnPathfindingNode.transform.position + (Vector3.one * GameUtils.Random(_configurationInts[1], -spawnPathfindingNode.Radius, spawnPathfindingNode.Radius));
        pos.y = spawnPathfindingNode.transform.position.y;

        GameObject unit = Instantiate(UnitManager.instance.UnitPrefabs[_unitID], pos, Quaternion.identity);
        unit.GetComponent<Unit>().PlayerID = _playerID;
        unit.GetComponent<Unit>().UnitUUID = _configurationInts[0];
        unit.GetComponent<Unit>().RandomSeed = _configurationInts[1];
        unit.GetComponent<PathfindingAgent>().Initialize(tile.TileInfo.Location, spawnPathfindingNode, unit.GetComponent<Unit>().RandomSeed);
    }

    #endregion
}
