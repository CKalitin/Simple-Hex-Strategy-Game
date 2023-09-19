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
    private Vector2Int location;

    public Tile Tile { get => tile; set => tile = value; }
    public Vector2Int LocalLocation { get => localLocation; set => localLocation = value; }
    public float Radius { get => radius; set => radius = value; }
    public bool Walkable { get => walkable; set => walkable = value; }
    public Vector2Int MapPathfindingLocation { get => pathfindingLocation; set => pathfindingLocation = value; }
    public Vector2Int Location { get => location; set => location = value; }

    private void Start() {
        pathfindingLocation = PathfindingManager.AddPathfindingLocationToMap(localLocation, tile.Location, (int)tile.TileId, this);

        location = pathfindingLocation;

        //locationText.text = pathfindingLocation.ToString();
    }
    
    public void OnDestroy() {
        if (pathfindingLocation != new Vector2Int(-999999999, -999999999)) PathfindingManager.RemovePathfindingLocationFromMap(pathfindingLocation);
        pathfindingLocation = new Vector2Int(-999999999, -999999999);
    }
}
