using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayTile : MonoBehaviour {
    [SerializeField] private TilePathfinding tilePathfinding;
    [Space]
    [SerializeField] private TileHighlight tileHighlight;
    
    private Tile tile;
    
    private int previousStructureCount = 0;

    private TilePathfinding currentTilePathfinding;

    public TileHighlight TileHighlight { get => tileHighlight; set => tileHighlight = value; }

    private void Awake() {
        tile = GetComponent<Tile>();
        
        if (tilePathfinding != null) {
            previousStructureCount = 0;
            currentTilePathfinding = tilePathfinding;
        }
    }

    private void Update() {
        if (tilePathfinding != null) CheckTileStructures();
    }

    private void CheckTileStructures() {
        if (previousStructureCount != tile.Structures.Count) {
            if (tile.Structures.Count == 0 || (tile.Structures.Count > 0 && tile.Structures[0].GetComponent<GameplayStructure>() == null)) {
                currentTilePathfinding = tilePathfinding;
            } else {
                currentTilePathfinding = tile.Structures[0].GetComponent<GameplayStructure>().TilePathfinding;
            }
            previousStructureCount = tile.Structures.Count;
        }
    }
    
    public TilePathfinding GetTilePathfinding() {
        if (currentTilePathfinding == null) currentTilePathfinding = tilePathfinding;
        return currentTilePathfinding;
    }
}
