using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    [SerializeField] private TilePathfinding tilePathfinding;
    
    public TilePathfinding TilePathfinding { get => tilePathfinding; set => tilePathfinding = value; }
}
