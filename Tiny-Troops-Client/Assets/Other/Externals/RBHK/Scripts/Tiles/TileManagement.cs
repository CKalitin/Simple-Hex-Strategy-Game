using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// This can't be a struct because structs are data types, not reference types. A reference to this is required in the Tile script.
// Just tested the class reference thing and it is AMAZING! I love references & pointers! hehe
public class TileInfo {
    private Tiles tileId;
    private Vector2Int location;
    private float y;

    private GameObject parentObject;
    private GameObject tileObject;
    private Tile tile;

    private bool spawned;

    public Tiles TileId { get => tileId; set => tileId = value; }
    public Vector2Int Location { get => location; set => location = value; }
    public float Y { get => y; set => y = value; }
    public GameObject TileObject { get => tileObject; set => tileObject = value; }
    public GameObject ParentObject { get => parentObject; set => parentObject = value; }
    public Tile Tile { get => tile; set => tile = value; }
    public bool Spawned { get => spawned; set => spawned = value; }

    public TileInfo(Tiles _tileId, Vector2Int _location, float _y, GameObject _parentObject, GameObject _tileObject) {
        tileId = _tileId;
        location = _location;

        y = _y;
        parentObject = _parentObject;
        tileObject = _tileObject;
        tile = _parentObject.GetComponent<Tile>();

        spawned = true;
    }

    // This if only for setting spawned to false. ew so inefficient
    public TileInfo(bool _spawned) {
        tileId = Tiles.Null;
        location = new Vector2Int(-999999999, -999999999);

        y = 0;
        parentObject = null;
        tileObject = null;

        tile = null;
        spawned = _spawned;
    }
}


public class TileManagement : MonoBehaviour {
    #region Variables

    public static TileManagement instance;

    [Header("Tiles")]
    private Dictionary<Vector2Int, TileInfo> tiles = new Dictionary<Vector2Int, TileInfo>();

    [Tooltip("This is used in the Structure script so they don't update all ResourceModifiers when they're spawned at first")]
    private bool spawningComplete = false;

    [Header("Specify")]
    [Tooltip("Tile Dimensions")]
    [SerializeField] private Vector2 tileDim = new Vector2(4.25f, 5f); // Tile dimensions

    public bool SpawningComplete { get => spawningComplete; set => spawningComplete = value; }

    #endregion

    #region Core

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (FindObjectsOfType<TileManagement>().Length > 1) {
            Destroy(this);
        } else {
            instance = this;
        }
    }

    #endregion

    #region Tiles

    public Vector2Int SpawnTile(GameObject _tilePrefab, Vector2Int _location) {
        // If tile already exists in location, return
        //if (tiles.ContainsKey(_location)) { return new Vector2Int(-999999999, -999999999); }

        // If tile already exists in location, delete old tile u idiot why would u just return ^^^
        if (tiles.ContainsKey(_location)) { Destroy(tiles[_location].ParentObject); }

        GameObject tileObject = Instantiate(_tilePrefab, TileLocationToWorldPosition(_location, 0), Quaternion.identity); // Instantiate Tile Prefab
        tiles.Add(_location, new TileInfo(tileObject.GetComponent<Tile>().TileId, _location, tileObject.transform.position.y, tileObject, tileObject.GetComponent<Tile>().DefaultTileObject));
        // Create a TileInfo class and add it to the tiles dictionary
        tileObject.GetComponent<Tile>().TileInfo = tiles[_location]; // Set TileInfo reference on Tile Script on Tile Object

        return _location;
    }

    public void DestroyTile(Vector2Int _location) {
        // Check if tile exists at location
        if (!tiles.ContainsKey(_location)) {
            //Debug.LogWarning("No tile to destroy at location: " + _location);
            return;
        }

        Destroy(tiles[_location].TileObject); // Delete tile's parentObject
        Destroy(tiles[_location].ParentObject); // Delete tileObject
        tiles[_location].Spawned = false; // Just some extra checks for the TileRule
        tiles[_location].TileId = Tiles.Null; // Just some extra checks for the TileRule
        tiles.Remove(_location); // Remove tileStruct from tile structs list (memory leak???, do i need to destroy the struct (class now actually)???, I hope c# handles it)

        ApplyTileRulesOnAdjacentTiles(_location);
    }

    public TileInfo GetTileAtLocation(Vector2Int _loc) {
        // If there is a tile at the location, return it
        if (tiles.ContainsKey(_loc))
            return tiles[_loc];

        // If not, create a new TileStruct with spawned=false and return it
        //Debug.LogWarning("Could not find file at location: " + _loc);
        return new TileInfo(false);
    }

    // Returns all Tile Locations around Center Tile in Radius
    // Does not Return Center Tile Location, does not Return Tile Locations without a Tile
    // Include Undefined Tiles means tile locations without a tile on them are included, this is used in the Tile script to check Tile Rules
    public List<Vector2Int> GetAdjacentTilesInRadius(Vector2Int _loc, int _r, bool _includeUndefinedTiles = false) {
        // _r = radius

        List<Vector2Int> output = new List<Vector2Int>();

        int max = _r * 2 + 1;

        for (int y = 0; y < _r * 2 + 1; y++) {
            int offset = Mathf.RoundToInt((Mathf.Abs(y - _r) - 0.1f) / 2); // Results eg: (0, 1) = 0 offset, (2, 3) = 1 offset, (4, 5) = 2 offset, ...
            offset += Mathf.Abs((_loc.y) % 2) * Mathf.Abs((y - _r) % 2); // If the locaction is odd and the distance to the center is odd, subtract 1, this fixes a bug
            
            int rowLength = offset + max - Mathf.Abs(y - _r); // Add the maximum value - the distance to the center to the offset (to the offset so the for loop works)
            
            for (int x = offset ; x < rowLength; x++) {
                Vector2Int loc = new Vector2Int(x - _r + _loc.x, y - _r + _loc.y); // Convert from for-loop location to world location

                if (loc.x == _loc.x && loc.y == _loc.y) { continue; } // If the current iteration is the center tile then skip it
                if (!_includeUndefinedTiles && !tiles.ContainsKey(loc)) { continue; } // If there is no tile at the location skip it

                output.Add(loc); // Add location to output
            }
        }

        return output;
    }

    // Convert tile location to position in world units
    public Vector3 TileLocationToWorldPosition(Vector2Int _loc, float _y) {
        return new Vector3(_loc.x * tileDim.x + (tileDim.x / 2 * (_loc.y % 2)), _y, _loc.y * (tileDim.y * 0.75f));

        /*** How it's made: ***/
        // _loc = location
        // _loc.x * tileDim.x seperates the tiles on the x axis
        // + (tiledim.x / 2) Adds half of the length of the tile to the x axis for "in-between" tiles that aren't on the exact x
        // (_loc.y % 2) sees if y is divisible by 2, if it is use the line above ^^^, if not multiply by 0
        // The tiles are seperated on the y axis by 0.75, every other tile (eg. multiples of 2) are 1.5 units apart, the "in-between" tiles are only 0.75 units up

        // Look at coordinate system section here for more info and examples: https://www.redblobgames.com/grids/hexagons/
    }

    public void ApplyTileRulesOnAdjacentTiles(Vector2Int _loc) {
        List<Vector2Int> adjacentTileLocs = GetAdjacentTilesInRadius(_loc, 1); // Get all neighbour tiles

        // Loop through neighbour tiles and apply tile rules
        for (int i = 0; i < adjacentTileLocs.Count; i++) {
            tiles[adjacentTileLocs[i]].Tile.ApplyTileRules();
        }
    }

    public void ApplyTileRules() {
        // Apply tile rules on all tiles, after they are done generating
        foreach (KeyValuePair<Vector2Int, TileInfo> kvp in tiles) {
            kvp.Value.Tile.ApplyTileRules();
        }
    }

    #endregion

    #region Other

    public void ApplyResourceModifiersOnAllTiles() {
        ResourceModifierApplier[] appliers = FindObjectsOfType<ResourceModifierApplier>();
        for (int i = 0; i < appliers.Length; i++) {
            appliers[i].ApplyResourceModifiers();
        }
    }

    #endregion
}
