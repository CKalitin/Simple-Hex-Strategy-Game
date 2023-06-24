using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayTile : MonoBehaviour {
    [SerializeField] private PathfindingLocationParent pathfindingLocationParent;

    public PathfindingLocationParent PathfindingLocationParent { get => pathfindingLocationParent; set => pathfindingLocationParent = value; }
}
