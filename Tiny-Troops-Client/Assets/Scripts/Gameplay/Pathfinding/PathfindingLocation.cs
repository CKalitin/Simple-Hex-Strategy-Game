using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PathfindingLocation : MonoBehaviour {
    [SerializeField] private Tile tile;
    [SerializeField] private Vector2Int localLocation;
    [SerializeField] private float radius;
    [SerializeField] private bool walkable;
    [Space]
    [SerializeField] private TextMeshProUGUI locationText;

    private Vector2Int pathfindingLocation;

    public Tile Tile { get => tile; set => tile = value; }
    public float Radius { get => radius; set => radius = value; }
    public bool Walkable { get => walkable; set => walkable = value; }
    public Vector2Int Location { get => pathfindingLocation; set => pathfindingLocation = value; }

    private void Start() {
        pathfindingLocation = PathfindingManager.AddPathfindingLocationToMap(localLocation, tile.Location, (int)tile.TileId, this);
        ///*
        locationText.text = $"({localLocation.x}, {localLocation.y})\n" +
            $"{pathfindingLocation}\n";
        //*/
    }

    private void OnDestroy() {
        PathfindingManager.RemovePathfindingLocationFromMap(pathfindingLocation);
    }
}
