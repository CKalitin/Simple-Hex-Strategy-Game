using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    // These variables should only be used for initialization of tileInfo, Also, it's wise to not use 0 for tileID
    [Header("Initialization")]
    [Tooltip("Very important for Tile Rules pls specify.")]
    [SerializeField] private Tiles tileId;
    [Tooltip("This variable is neccessary and only used in instantiation. Put the existing tile object here.")]
    [SerializeField] private GameObject defaultTileObject;

    [Header("Tile")]
    [SerializeField] private TileRule[] tileRules;
    [SerializeField] private Transform structureLocationsParent;

    [Header("Debug")]
    [Tooltip("DEBUG FIELD DON'T CHANGE VALUES\nThe structures that are on this tile.")]
    [SerializeField] private List<Structure> structures = new List<Structure>();
    [Tooltip("DEBUG FIELD DON'T CHANGE VALUES\nResource Modifiers that will be applied to all buildings on this tile.")]
    [SerializeField] private RBHKUtils.IndexList<ResourceModifier> resourceModifiers = new RBHKUtils.IndexList<ResourceModifier>();
    [Space]
    [SerializeField] private Vector2Int location;

    private TileInfo tileInfo;

    public Tiles TileId { get => tileId; set => tileId = value; }
    public TileInfo TileInfo { get => tileInfo; set => tileInfo = value; }
    public GameObject DefaultTileObject { get => defaultTileObject; set => defaultTileObject = value; }
    public Transform StructureLocationsParent { get => structureLocationsParent; set => structureLocationsParent = value; }
    public List<Structure> Structures { get => structures; set => structures = value; }
    public RBHKUtils.IndexList<ResourceModifier> ResourceModifiers { get => resourceModifiers; set => resourceModifiers = value; }
    public Vector2Int Location { get => location; set => location = value; }

    void Start() {
        if (TileManagement.instance.SpawningComplete) {
            ApplyTileRules();
            UpdateResourceModifiers(); // Putting this line here just in case
        }

        location = TileInfo.Location;
    }
    
    public void ApplyTileRules() {
        List<Vector2Int> adjacentTiles = TileManagement.instance.GetAdjacentTilesInRadius(tileInfo.Location, 1, true);

        // Loop through all tile Rules on this tile
        for (int i = 0; i < tileRules.Length; i++) {
            // Check if tile rule is true, input tiles adjacent to current tile
            if (tileRules[i].CheckTileRule(adjacentTiles)) {
                Destroy(tileInfo.TileObject); // Destroy old tile object

                // Spawn new tile object at same position and set parent to tile's parent object
                tileInfo.TileObject = Instantiate(tileRules[i].UpdatedTilePrefab, TileManagement.instance.TileLocationToWorldPosition(TileInfo.Location, 0), Quaternion.identity, tileInfo.ParentObject.transform);
            }
        }
    }

    // This function updates the resource modifiers on the structures on this tile
    public void UpdateResourceModifiers() {
        for (int i = 0; i < structures.Count; i++) {
            structures[i].UpdateResourceModifiers();
        }
    }
}
