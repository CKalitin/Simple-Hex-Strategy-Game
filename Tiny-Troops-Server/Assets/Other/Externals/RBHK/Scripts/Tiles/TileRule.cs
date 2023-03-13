using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Tile Rule", menuName = "RBHK/Tiles/Tile Rule")]
public class TileRule : ScriptableObject {
    [SerializeField] private Tiles intendedTileId;
    [Space]
    [SerializeField] private GameObject updatedTilePrefab;

    [Tooltip("This is the list of tiles needed to use the updatedTilePrefab.\n" +
        "They should be in order from the bottom y first, then moving left, like the GetAdjacentTiles function.\n" +
        "Leave Blank for no tile required.")]
    [SerializeField] private Tiles[] requiredAdjacentTileIds = new Tiles[6];

    public Tiles IntendedTileId { get => intendedTileId; set => intendedTileId = value; }
    public GameObject UpdatedTilePrefab { get => updatedTilePrefab; set => updatedTilePrefab = value; }
    public Tiles[] RequiredAdjacentTiles { get => requiredAdjacentTileIds; set => requiredAdjacentTileIds = value; }

    // Return true is updatedTilePrefab should be used
    public bool CheckTileRule(List<Vector2Int> _adjacentTileLocs) {
        // Output is false by default so find a tile that is correct and return true
        for (int i = 0; i < _adjacentTileLocs.Count; i++) {
            if (requiredAdjacentTileIds[i] == Tiles.Null) continue; // If there is no TileRule for specified tile
            if (!TileManagement.instance.GetTileAtLocation(_adjacentTileLocs[i]).Spawned) continue; // If the tile is not spawned (there is no tile at the location)
            if (TileManagement.instance.GetTileAtLocation(_adjacentTileLocs[i]).TileId != requiredAdjacentTileIds[i]) continue; // If tile ID is correct

            return true;
        }

        return false;
    }
}
