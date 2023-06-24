using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayTile : MonoBehaviour {
    [SerializeField] private TileHighlight tileHighlight;

    [SerializeField] private PathfindingLocationParent pathfindingLocationParent;

    public TileHighlight TileHighlight { get => tileHighlight; set => tileHighlight = value; }
    public PathfindingLocationParent PathfindingLocationParent { get => pathfindingLocationParent; set => pathfindingLocationParent = value; }
}
