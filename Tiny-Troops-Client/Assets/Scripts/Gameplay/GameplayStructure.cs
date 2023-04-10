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

    private bool addedToStructureManager = false;

    public delegate void StructureActionCallback(int playerID, int actionID, int[] configurationInts);
    public event StructureActionCallback OnStructureAction;

    public GameObject StructureUI { get => structureUI; set => structureUI = value; }
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

    public void OnStructureActionPacket(int _playerID, int _actionID, int[] _configurationInts) {
        OnStructureAction(_playerID, _actionID, _configurationInts);
    }

    #endregion
}
