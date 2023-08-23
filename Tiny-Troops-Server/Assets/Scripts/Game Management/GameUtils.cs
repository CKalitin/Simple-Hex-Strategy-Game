using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtils {
    // nodes around the tile in every direction
    //    (0, 1)  (1, 1)
    // (-1,0) (self) (1, 0)
    //    (0,-1)  (1,-1)
    public static Vector2Int[] Directions = new Vector2Int[6] { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(0, 1) };

    public static int Random(int _seed, int _min, int _max) {
        return new System.Random(_seed).Next(_min, _max);
    }

    public static float Random(int _seed, float _min, float _max) {
        return new System.Random(_seed).Next(Mathf.RoundToInt(_min * 1000000), Mathf.RoundToInt(_max * 1000000)) / 1000000f;
    }

    public static int Random(ref System.Random _random, int _min, int _max) {
        return _random.Next(_min, _max);
    }

    public static float Random(ref System.Random _random, float _min, float _max) {
        return _random.Next(Mathf.RoundToInt(_min * 1000000), Mathf.RoundToInt(_max * 1000000)) / 1000000f;
    }

    public static Tile GetTileParent(Transform _transform) {
        Tile output = null;
        Transform t = _transform.parent;
        while (true) {
            if (t.GetComponent<Tile>()) {
                // If tile script found
                output = t.GetComponent<Tile>();
                break;
            } else {
                // If we're at the top of the heirarchy, break out of the loop.
                if (t.parent == null) break;
                t = t.parent;
            }
        }
        return output;
    }

    public static List<Vector2Int> GetDirectionsWithID(Vector2Int _location, StructureID _structureID) {
        List<Vector2Int> output = new List<Vector2Int>();
        for (int i = 0; i < Directions.Length; i++) {
            Tile tile;
            if ((tile = TileManagement.instance.GetTileAtLocation(GetTargetDirection(_location, Directions[i])).Tile) != null)
                if (tile.Structures.Count > 0 && tile.Structures[0].StructureID == _structureID) output.Add(Directions[i]);
        }
        return output;
    }

    private static Vector2Int GetTargetDirection(Vector2Int _currentLocation, Vector2Int _targetLocation) {
        Vector2Int _targetDirection = _currentLocation + _targetLocation;
        if (_currentLocation.y % 2 == 0 && _targetLocation.y != 0) _targetDirection.x -= 1;
        return _targetDirection;
    }
}
