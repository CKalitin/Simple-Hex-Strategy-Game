using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour {
    [SerializeField] private PathfindingNode spawnPathfindingNode;
    [Space]
    [SerializeField] private GameObject[] unitPrefabs;

    private Tile tile;
    private Vector2Int location;

    private void Start() {
        GetTileParent();
        location = tile.TileInfo.Location;

        SpawnUnit(0, 0);
    }

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

    public void SpawnUnit(int _unitID, int _playerID) {
        Vector3 pos = spawnPathfindingNode.transform.position + (Vector3.one * Random.Range(-spawnPathfindingNode.Radius, spawnPathfindingNode.Radius));
        pos.y = spawnPathfindingNode.transform.position.y;
        GameObject unit = Instantiate(unitPrefabs[_unitID], pos, Quaternion.identity);
        unit.GetComponent<PathfindingAgent>().Initialize(location, spawnPathfindingNode);
        unit.GetComponent<Unit>().PlayerID = _playerID;
    }
}
