using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    #region Variables

    [Header("Structure")]
    [SerializeField] private bool playerOwnedStructure = false;

    [Header("Bonus")]
    [SerializeField] private Bonus[] bonuses;
    [SerializeField] private bool applyBonuses = false;
    List<int> bonusResourceEntryIndexes = new List<int>();

    [Header("UI")]
    [SerializeField] private GameObject structureUI;

    [Header("Pathfinding")]
    [Tooltip("     (1, 3)  (2, 3)   \n(0, 2) (1, 2) (2, 2)\n     (1, 1)  (2, 1)")]
    [SerializeField] private Vector2Int[] unwalkableLocalPathfindingLocations;

    [Header("Other")]
    [SerializeField] private Health health;

    private Vector2Int tileLocation;
    private bool addedToStructureManager = false;
    private GameplayTile gameplayTile;

    public delegate void StructureActionCallback(int playerID, int actionID, int[] configurationInts);
    public event StructureActionCallback OnStructureAction;

    public GameObject StructureUI { get => structureUI; set => structureUI = value; }
    public Bonus[] Bonuses { get => bonuses; set => bonuses = value; }

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

    // The code in Start() and OnEnable() is the same because of the different times a structure can be instantiated. With the tile, or on the tile by a player.
    private void Start() {
        Initialize();
        gameplayTile = GameUtils.GetTileParent(transform).GetComponent<GameplayTile>();
        SetPathfindingNodesWalkable(false);
        if (applyBonuses) {
            ApplyBonuses();
            for (int i = 0; i < GameUtils.Directions.Length; i++) {
                GameplayStructure gameStruct;
                Vector2Int _loc = GameUtils.GetTargetDirection(GetComponent<Structure>().Tile.TileInfo.Location, GameUtils.Directions[i]);
                if (TileManagement.instance.GetTileAtLocation(_loc).Tile.Structures.Count <= 0) continue;
                if ((gameStruct = TileManagement.instance.GetTileAtLocation(_loc).Tile.Structures[0].GetComponent<GameplayStructure>()) == null) continue;
                gameStruct.ReapplyBonuses();
            }
        }
    }

    private void OnEnable() {
        Initialize();
    }

    private void OnDisable() {
        SetPathfindingNodesWalkable(true);
        RemoveBonuses();
        if (!playerOwnedStructure) return;
        StructureManager.instance.RemoveGameplayStructure(tileLocation, this);
        addedToStructureManager = false;
    }

    private void Initialize() {
        if (!playerOwnedStructure) return;
        if (addedToStructureManager == false && GetComponent<Structure>().Tile.TileInfo != null) {
            tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
            StructureManager.instance.AddGameplayStructure(tileLocation, this);
            addedToStructureManager = true;
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

    private void ApplyBonuses() {
        for (int i = 0; i < bonuses.Length; i++) {
            int bonus = GameUtils.GetDirectionsWithID(GetComponent<Structure>().Tile.Location, bonuses[i].BonusStructureID).Count * bonuses[i].BonusAmount;
            ResourceEntry resourceEntry = ScriptableObject.CreateInstance<ResourceEntry>();
            resourceEntry.ResourceId = GetComponent<Structure>().ResourceEntries[0].ResourceId;
            resourceEntry.ResourceEntryIds = GetComponent<Structure>().ResourceEntries[0].ResourceEntryIds;
            resourceEntry.Change = bonus;
            resourceEntry.ChangeOnTick = GetComponent<Structure>().ResourceEntries[0].ChangeOnTick;
            bonusResourceEntryIndexes.Add(ResourceManager.instances[GetComponent<Structure>().PlayerID].AddResourceEntry(resourceEntry));
        }
    }

    private void RemoveBonuses() {
        for (int i = 0; i < bonusResourceEntryIndexes.Count; i++) {
            ResourceManager.instances[GetComponent<Structure>().PlayerID].RemoveResourceEntry(bonusResourceEntryIndexes[i]);
        }
    }

    public void ReapplyBonuses() {
        RemoveBonuses();
        ApplyBonuses();
    }

    #endregion
}
