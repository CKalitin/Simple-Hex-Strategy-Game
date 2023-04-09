using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    #region Variables

    [SerializeField] private TilePathfinding tilePathfinding;

    private Vector2Int tileLocation;
    
    private bool addedToStructureManager = false;

    public delegate void StructureActionCallback(int playerID, int actionID);
    public event StructureActionCallback OnStructureAction;

    public TilePathfinding TilePathfinding { get => tilePathfinding; set => tilePathfinding = value; }

    #endregion

    #region Core

    private void Start() {
        if (addedToStructureManager == false && GetComponent<Structure>().Tile.TileInfo != null) {
            tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
            StructureManager.instance.AddGameplayStructure(tileLocation, this);
            addedToStructureManager = true;
        }
    }

    private void OnEnable() {
        if (addedToStructureManager == false && GetComponent<Structure>().Tile.TileInfo != null) {
            tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
            StructureManager.instance.AddGameplayStructure(tileLocation, this);
            addedToStructureManager = true;
        }
    }

    private void OnDisable() {
        StructureManager.instance.RemoveGameplayStructure(tileLocation, this);
        addedToStructureManager = false;
    }

    #endregion

    #region Structure Actions

    public void OnStructureActionPacket(int _playerID, int _actionID) {
        OnStructureAction(_playerID, _actionID);
    }

    #endregion
}
