using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingLocationParent : MonoBehaviour {
    [Tooltip("These are pathfinding locations that are physically on the tile, Some are inbetween tiles.")]
    [SerializeField] private PathfindingLocation[] centralPathfindingLocations;

    public PathfindingLocation GetRandomCentralPathfindingLocation(ref System.Random _rand) {
        PathfindingLocation output = null;

        while (output == null || !output.Walkable) {
            int rand = _rand.Next(0, centralPathfindingLocations.Length - 1);
            output = centralPathfindingLocations[rand];
        }

        return output;
    }

    public PathfindingLocation GetRandomCentralPathfindingLocation(int _randomSeed) {
        System.Random rand = new System.Random(_randomSeed);
        return GetRandomCentralPathfindingLocation(ref rand);
    }

    public PathfindingLocation GetRandomCentralPathfindingLocation() {
        PathfindingLocation output = null;

        while (output == null || !output.Walkable) {
            output = centralPathfindingLocations[Random.Range(0, centralPathfindingLocations.Length - 1)];
        }

        return output;
    }
}
