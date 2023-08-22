using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    #region Variables

    [Header("Structure")]
    [SerializeField] private bool playerOwnedStructure = false;
    [Space]
    [Tooltip("Tile IDs to highlight bonus over.")]
    [SerializeField] private Tiles bonusTileID;
    [SerializeField] private int bonusAmount;

    [Header("UI")]
    [SerializeField] private GameObject structureUI;

    [Header("Pathfinding")]
    [Tooltip("     (1, 1)  (2, 1)   \n(0, 2) (1, 2) (2, 2)\n     (1, 3)  (2, 3)")]
    [SerializeField] private Vector2Int[] unwalkableLocalPathfindingLocations;

    [Header("Other")]
    [SerializeField] private Health health;

    private Vector2Int tileLocation;
    private bool addedToStructureManager = false;
    private GameplayTile gameplayTile;

    public delegate void StructureActionCallback(int playerID, int actionID, int[] configurationInts);
    public event StructureActionCallback OnStructureAction;

    public GameObject StructureUI { get => structureUI; set => structureUI = value; }
    public Tiles BonusTileID { get => bonusTileID; set => bonusTileID = value; }
    public int BonusAmount { get => bonusAmount; set => bonusAmount = value; }

    #endregion

    #region Core

    // The code in Start() and OnEnable() is the same because of the different times a structure can be instantiated. With the tile, or on the tile by a player.
    private void Start() {
        Initialize();
    }

    private void OnEnable() {
        Initialize();
    }

    private void OnDisable() {
        if (!playerOwnedStructure) return;
        SetPathfindingNodesWalkable(true);
        StructureManager.instance.RemoveGameplayStructure(tileLocation, this);
        addedToStructureManager = false;
    }

    private void Initialize() {
        if (!playerOwnedStructure) return;
        if (addedToStructureManager == false && GetComponent<Structure>().Tile.TileInfo != null) {
            tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
            StructureManager.instance.AddGameplayStructure(tileLocation, this);
            addedToStructureManager = true;
            gameplayTile = GameUtils.GetTileParent(transform).GetComponent<GameplayTile>();
            SetPathfindingNodesWalkable(false);
        }
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

    #region Other

    private void SetPathfindingNodesWalkable(bool _walkable) {
        for (int i = 0; i < unwalkableLocalPathfindingLocations.Length; i++) {
            gameplayTile.PathfindingLocationParent.PathfindingLocations[unwalkableLocalPathfindingLocations[i]].Walkable = _walkable;
        }
    }
    
    #endregion
}
