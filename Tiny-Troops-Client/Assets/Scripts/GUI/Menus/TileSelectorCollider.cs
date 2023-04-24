using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelectorCollider : MonoBehaviour {
    [SerializeField] private Tile tile;
    
    public Tile Tile { get => tile; set => tile = value; }
}
