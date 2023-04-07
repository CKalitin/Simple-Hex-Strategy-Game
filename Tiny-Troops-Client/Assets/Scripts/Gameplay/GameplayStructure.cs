using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    #region Variables

    [Header("UI")]
    [SerializeField] private GameObject structureUI;
    [Space]
    [SerializeField] private TilePathfinding tilePathfinding;

    private Vector2Int tileLocation;

    private TileActionMenu tileActionMenu;

    public GameObject StructureUI { get => structureUI; set => structureUI = value; }
    public TilePathfinding TilePathfinding { get => tilePathfinding; set => tilePathfinding = value; }
    
    public delegate void StructureActionCallback(int playerID, int actionID);
    public event StructureActionCallback OnStructureAction;

    #endregion

    #region Core

    private void OnEnable() {
        Debug.Log(gameObject);
        tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
        StructureManager.instance.AddGameplayStructure(tileLocation, this);
    }

    private void OnDisable() {
        StructureManager.instance.RemoveGameplayStructure(tileLocation, this);
    }

    #endregion

    #region UI

    public void OnStructureUICloseButton() {
        tileActionMenu.ToggleActive(false);
    }

    public void SetTileActionMenu(TileActionMenu _tam) {
        tileActionMenu = _tam;
        structureUI.GetComponent<StructureActionMenu>().TileActionMenu = _tam;
    }

    #endregion

    #region Structure Actions

    public void OnStructureActionPacket(int _playerID, int _actionID) {
        OnStructureAction(_playerID, _actionID);
    }

    #endregion
}
