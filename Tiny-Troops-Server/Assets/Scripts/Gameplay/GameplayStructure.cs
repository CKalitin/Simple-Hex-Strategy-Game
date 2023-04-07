using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    #region Variables

    [SerializeField] private TilePathfinding tilePathfinding;

    private Vector2Int tileLocation;
    
    public TilePathfinding TilePathfinding { get => tilePathfinding; set => tilePathfinding = value; }

    public delegate void StructureActionCallback(int playerID, int actionID);
    public event StructureActionCallback OnStructureAction;

    #endregion

    #region Core
    
    private void OnEnable() {
        tileLocation = gameObject.GetComponent<Structure>().Tile.TileInfo.Location;
        StructureManager.instance.AddGameplayStructure(tileLocation, this);
    }

    private void OnDisable() {
        StructureManager.instance.RemoveGameplayStructure(tileLocation, this);
    }

    #endregion

    #region Structure Actions

    public void OnStructureActionPacket(int _playerID, int _actionID) {
        OnStructureAction(_playerID, _actionID);
    }

    #endregion
}
