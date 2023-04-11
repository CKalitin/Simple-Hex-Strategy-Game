using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    #region Variables

    [Header("Structure")]
    [SerializeField] private bool playerOwnedStructure = false;

    [Header("UI")]
    [SerializeField] private GameObject structureUI;
    [Space]
    [SerializeField] private TilePathfinding tilePathfinding;

    [Header("Other")]
    [SerializeField] private Health health;

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
        if (!playerOwnedStructure) return;
        if (addedToStructureManager == false && GetComponent<Structure>().Tile.TileInfo != null) {
            tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
            StructureManager.instance.AddGameplayStructure(tileLocation, this);
            addedToStructureManager = true;
        }
    }

    private void OnEnable() {
        if (!playerOwnedStructure) return;
        if (addedToStructureManager == false && GetComponent<Structure>().Tile.TileInfo != null) {
            tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
            StructureManager.instance.AddGameplayStructure(tileLocation, this);
            addedToStructureManager = true;
        }
    }

    private void OnDisable() {
        if (!playerOwnedStructure) return;
        StructureManager.instance.RemoveGameplayStructure(tileLocation, this);
        addedToStructureManager = false;
    }

    #endregion

    #region UI

    public void OnStructureUICloseButton() {
        if (!playerOwnedStructure) return;
        tileActionMenu.ToggleActive(false);
    }

    public void SetTileActionMenu(TileActionMenu _tam) {
        if (!playerOwnedStructure) return;
        tileActionMenu = _tam;
        structureUI.GetComponent<StructureActionMenu>().TileActionMenu = _tam;
    }

    #endregion

    #region Structure Actions

    public void OnStructureActionPacket(int _playerID, int _actionID, int[] _configurationInts) {
        if (!playerOwnedStructure) return;
        OnStructureAction(_playerID, _actionID, _configurationInts);
    }

    public void OnStructureHealthPacket(float _health, float _maxHealth) {
        health.SetHealth(_health, _maxHealth);
        if (health.CurrentHealth <= 0) GetComponent<Structure>().DestroyStructure();
    }

    #endregion
}
