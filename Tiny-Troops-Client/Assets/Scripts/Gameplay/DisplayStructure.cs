using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayStructure : MonoBehaviour {
    [Tooltip("Tile IDs to highlight bonus over")]
    [SerializeField] private Tiles targetTileID;
    [Tooltip("Clockwise from top left")]
    [SerializeField] private GameObject[] bonusDisplays;
    
    private Tile tile;

    // nodes around the tile in every direction
    //    (0, 1)  (1, 1)
    // (-1,0) (self) (1, 0)
    //    (0,-1)  (1,-1)
    private Vector2Int[] directions = new Vector2Int[6] { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(0, 1) };

    public Tile Tile { get => tile; set => tile = value; }

    private void Awake() {
        for (int i = 0; i < bonusDisplays.Length; i++) {
            bonusDisplays[i].SetActive(false);
        }
    }

    private void Update() {
        if (targetTileID == Tiles.Null) return;
        if (tile == null) return;

        for (int i = 0; i < directions.Length; i++) {
            if (TileManagement.instance.GetTileAtLocation(GetTargetDirection(tile.Location, directions[i])).Tile.TileId == targetTileID) {
                bonusDisplays[i].SetActive(true);
            } else {
                bonusDisplays[i].SetActive(false);
            }
        }
    }

    public void Initialize(Tiles _TargetTileID, int _bonusAmount, string _bonusPrefix) {
        targetTileID = _TargetTileID;
        for (int i = 0; i < bonusDisplays.Length; i++) {
            bonusDisplays[i].GetComponent<TextMeshProUGUI>().text = _bonusPrefix + _bonusAmount;
        }
    }
    private Vector2Int GetTargetDirection(Vector2Int _currentLocation, Vector2Int _targetLocation) {
        Vector2Int _targetDirection = _currentLocation + _targetLocation;
        if (_currentLocation.y % 2 == 0 && _targetLocation.y != 0) _targetDirection.x -= 1;
        return _targetDirection;
    }
}
