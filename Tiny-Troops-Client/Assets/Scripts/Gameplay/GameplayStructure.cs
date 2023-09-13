using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    #region Variables

    [Header("Structure")]
    [SerializeField] private bool playerOwnedStructure = false;
    [Tooltip("Turn this off if it is the outcome of a construction structure.")]
    [SerializeField] private bool applyCost;
    [SerializeField] private bool applyRefunds;

    [Header("Bonus")]
    [SerializeField] private Bonus[] bonuses;
    [SerializeField] private bool applyBonuses = false;
    
    private ResourceEntry firstStructureResourceEntry;
    private List<ResourceEntry> bonusResourceEntries = new List<ResourceEntry>();

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
    private bool productionEnabled = true; // Used in ProductionSubtractor.cs
    private float distToNearestVillage = 0f; // Used in ProductionSubtractor.cs
    private float firstStructureResourceEntryInitialProduction = 0;

    private bool beingDestroyed = false;
    private List<int> destroyerPlayerIDs = new List<int>();

    public delegate void StructureActionCallback(int playerID, int actionID, int[] configurationInts);
    public event StructureActionCallback OnStructureAction;

    public GameObject StructureUI { get => structureUI; set => structureUI = value; }
    public Bonus[] Bonuses { get => bonuses; set => bonuses = value; }
    public bool BeingDestroyed { get => beingDestroyed; set => beingDestroyed = value; }
    public List<int> DestroyerPlayerIDs { get => destroyerPlayerIDs; set => destroyerPlayerIDs = value; }
    public bool ApplyCost { get => applyCost; set => applyCost = value; }
    public bool ProductionEnabled { get => productionEnabled; set => productionEnabled = value; }
    public float DistToNearestVillage { get => distToNearestVillage; set => distToNearestVillage = value; }

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

    private void Awake() {
        // Copy it, now not the original Scriptable Object in the project files. Scriptable Objects are not the proper way to store this data
        if (GetComponent<Structure>().ResourceEntries.Length <= 0) return;
        firstStructureResourceEntry = ScriptableObject.CreateInstance<ResourceEntry>();
        firstStructureResourceEntry.ResourceId = GetComponent<Structure>().ResourceEntries[0].ResourceId;
        firstStructureResourceEntry.ResourceEntryIds = GetComponent<Structure>().ResourceEntries[0].ResourceEntryIds;
        firstStructureResourceEntry.Change = GetComponent<Structure>().ResourceEntries[0].Change;
        firstStructureResourceEntry.ChangeOnTick = GetComponent<Structure>().ResourceEntries[0].ChangeOnTick;
        if (firstStructureResourceEntry != null) firstStructureResourceEntryInitialProduction = firstStructureResourceEntry.Change;
    }

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
        if (!applyRefunds) GetComponent<Structure>().DontApplyRefunds = true;

        if (firstStructureResourceEntry != null) firstStructureResourceEntryInitialProduction = firstStructureResourceEntry.Change;
    }

    private void Update() {
        if (firstStructureResourceEntry != null) firstStructureResourceEntry.ResourceId = firstStructureResourceEntry.ResourceId; // Prevents garbage collection? Why does it go null?
    }

    private void OnEnable() {
        Initialize();
    }

    private void OnDisable() {
        SetPathfindingNodesWalkable(true);
        RemoveBonuses();
        StructureManager.instance.RemoveGameplayStructure(tileLocation, this);
        addedToStructureManager = false;
    }

    private void OnDestroy() {
        GetComponent<Structure>().StructureID = StructureID.Null;
        for (int i = 0; i < GameUtils.Directions.Length; i++) {
            GameplayStructure gameStruct;
            Vector2Int _loc = GameUtils.GetTargetDirection(GetComponent<Structure>().Tile.TileInfo.Location, GameUtils.Directions[i]);
            if (TileManagement.instance.GetTileAtLocation(_loc).Tile.Structures.Count <= 0) continue;
            if ((gameStruct = TileManagement.instance.GetTileAtLocation(_loc).Tile.Structures[0].GetComponent<GameplayStructure>()) == null) continue;
            gameStruct.ReapplyBonuses();
        }

        if (ProductionSubtractor.instance.DisabledStructures.ContainsKey(GetComponent<Structure>())) {
            ProductionSubtractor.instance.DisabledStructures.Remove(GetComponent<Structure>());
        }
    }

    private void Initialize() {
        //if (!playerOwnedStructure) return;
        if (addedToStructureManager == false && GetComponent<Structure>().Tile.TileInfo != null) {
            tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
            StructureManager.instance.AddGameplayStructure(tileLocation, this);
            addedToStructureManager = true;
        }
    }

    #endregion

    #region Structure Actions

    public void OnStructureActionPacket(int _playerID, int _actionID, int[] _configurationInts) {
        SetBeingDestroyed(_actionID, (int[])_configurationInts.Clone());

        if (!playerOwnedStructure) return;
        if (OnStructureAction != null) OnStructureAction(_playerID, _actionID, (int[])_configurationInts.Clone());
    }

    public void OnStructureHealthPacket(float _health, float _maxHealth) {
        health.SetHealth(_health, _maxHealth);
        if (health.CurrentHealth <= 0) {
            //if (!beingDestroyed) GetComponent<Structure>().DontApplyRefunds = true; // If villager is not destroying this structure <- this is a server thing
            GetComponent<Structure>().DestroyStructure();
        }
    }

    #endregion

    #region Bonuses

    private void ApplyBonuses() {
        return; // Doesn't matter, this is client
        /*for (int i = 0; i < bonuses.Length; i++) {
            int bonus = GameUtils.GetDirectionsWithID(GetComponent<Structure>().Tile.Location, bonuses[i].BonusStructureID).Count * bonuses[i].BonusAmount;
            ResourceEntry resourceEntry = ScriptableObject.CreateInstance<ResourceEntry>();
            resourceEntry.ResourceId = firstStructureResourceEntry.ResourceId;
            resourceEntry.ResourceEntryIds = firstStructureResourceEntry.ResourceEntryIds;
            resourceEntry.Change = bonus;
            resourceEntry.ChangeOnTick = firstStructureResourceEntry.ChangeOnTick;
            bonusResourceEntries.Add(resourceEntry);
            ResourceManager.instances[GetComponent<Structure>().PlayerID].AddResourceEntry(resourceEntry);
        }*/
    }

    private void RemoveBonuses() {
        for (int i = 0; i < bonusResourceEntries.Count; i++) {
            ResourceManager.instances[GetComponent<Structure>().PlayerID].RemoveResourceEntry(bonusResourceEntries[i]);
        }
        bonusResourceEntries.Clear();
    }

    public void ReapplyBonuses() {
        RemoveBonuses();
        ApplyBonuses();
    }

    #endregion

    #region Utils

    private void SetPathfindingNodesWalkable(bool _walkable) {
        for (int i = 0; i < unwalkableLocalPathfindingLocations.Length; i++) {
            gameplayTile.PathfindingLocationParent.PathfindingLocations[unwalkableLocalPathfindingLocations[i]].Walkable = _walkable;
        }
    }

    private void SetBeingDestroyed(int _actionID, int[] _configurationInts) {
        if (_actionID == -2000) destroyerPlayerIDs.Add(_configurationInts[0]);
        else if (_actionID == -2001) destroyerPlayerIDs.Remove(_configurationInts[0]);

        beingDestroyed = destroyerPlayerIDs.Count > 0;
    }

    public void ToggleProduction(bool _toggle) {
        if (GetComponent<Structure>().ResourceEntries.Length <= 0) return;
            if (_toggle) {
            if (bonuses.Length <= 0) ApplyBonuses();
            GetComponent<Structure>().ResourceEntries[0].Change = 0;
            productionEnabled = true;
        } else {
            RemoveBonuses();
            GetComponent<Structure>().ResourceEntries[0].Change = firstStructureResourceEntryInitialProduction;
            productionEnabled = false;
        }
    }
    
    #endregion
}