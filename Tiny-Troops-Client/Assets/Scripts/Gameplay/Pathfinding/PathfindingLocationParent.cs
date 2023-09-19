using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PathfindingLocationParent : MonoBehaviour {
    [Tooltip("These are pathfinding locations that are physically on the tile, Some are inbetween tiles.")]
    [SerializeField] private PathfindingLocation[] centralPathfindingLocations;

    private Dictionary<Vector2Int, PathfindingLocation> pathfindingLocations = new Dictionary<Vector2Int, PathfindingLocation>();

    public PathfindingLocation[] CentralPathfindingLocations { get => centralPathfindingLocations; set => centralPathfindingLocations = value; }
    public Dictionary<Vector2Int, PathfindingLocation> PathfindingLocations { get => pathfindingLocations; set => pathfindingLocations = value; }

    private void Awake() {
        for (int i = 0; i < transform.childCount; i++) {
            pathfindingLocations.Add(transform.GetChild(i).GetComponent<PathfindingLocation>().LocalLocation, transform.GetChild(i).GetComponent<PathfindingLocation>());
        }
    }

    public PathfindingLocation GetRandomCentralPathfindingLocation(ref System.Random _rand) {
        PathfindingLocation output = null;

        int iters = 0;
        while (output == null || !output.Walkable || iters > 10) {
            int rand = _rand.Next(0, centralPathfindingLocations.Length - 1);
            output = centralPathfindingLocations[rand];
            iters++;
        }

        return output;
    }

    public PathfindingLocation GetRandomCentralPathfindingLocation(int _randomSeed) {
        System.Random rand = new System.Random(_randomSeed);
        return GetRandomCentralPathfindingLocation(ref rand);
    }

    public PathfindingLocation GetRandomCentralPathfindingLocation() {
        PathfindingLocation output = null;

        int iters = 0;
        while (output == null || !output.Walkable || iters > 10) {
            output = centralPathfindingLocations[Random.Range(0, centralPathfindingLocations.Length - 1)];
            iters++;
        }

        return output;
    }
}
