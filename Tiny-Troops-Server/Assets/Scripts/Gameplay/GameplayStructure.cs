using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    #region Variables

    [Header("Bonus")]
    [SerializeField] private Bonus[] bonuses;

    [Header("Pathfinding")]
    [Tooltip("     (1, 3)  (2, 3)   \n(0, 2) (1, 2) (2, 2)\n     (1, 1)  (2, 1)")]
    [SerializeField] private Vector2Int[] unwalkableLocalPathfindingLocations;

    [Header("Other")]
    [SerializeField] private Health health;

    private Vector2Int tileLocation;
    private bool addedToStructureManager = false;
    private GameplayTile gameplayTile;

    private float previousHealth = -1f; // This is -1 so on the first Update() it gets updated

    public delegate void StructureActionCallback(int playerID, int actionID, int[] configurationInts);
    public event StructureActionCallback OnStructureAction;
    
    public Vector2Int TileLocation { get => tileLocation; set => tileLocation = value; }

    [Serializable]
    public struct Bonus {
        [Tooltip("If the structure is adjacent to structures of this ID, it gets a bonus.")]
        [SerializeField] private StructureID bonusStructureID;
        [SerializeField] private int bonusAmount;

        public StructureID BonusStructureID { get => bonusStructureID; set => bonusStructureID = value; }
        public int BonusAmount { get => bonusAmount; set => bonusAmount = value; }
    }

    #endregion

    #region Core

    private void Start() {
        Initialize();
        gameplayTile = GameUtils.GetTileParent(transform).GetComponent<GameplayTile>();
        SetPathfindingNodesWalkable(false);
    }

    private void Update() {
        if (previousHealth != health.CurrentHealth) {
            previousHealth = health.CurrentHealth;

            USNL.PacketSend.StructureHealth(TileLocation, health.CurrentHealth, health.MaxHealth);
            if (health.CurrentHealth <= 0) GetComponent<Structure>().DestroyStructure();
        }
    }

    private void OnEnable() {
        Initialize();
    }

    private void OnDisable() {
        SetPathfindingNodesWalkable(true);
        StructureManager.instance.RemoveGameplayStructure(tileLocation, this);
        addedToStructureManager = false;
    }

    private void Initialize() {
        if (addedToStructureManager == false && GetComponent<Structure>().Tile.TileInfo != null) {
            tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
            StructureManager.instance.AddGameplayStructure(tileLocation, this);
            addedToStructureManager = true;
        }
    }

    #endregion

    #region Structure Actions

    public void OnStructureActionPacket(int _playerID, int _actionID, int[] _configurationInts) {
        OnStructureAction(_playerID, _actionID, _configurationInts);
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
