using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    #region Variables

    [Header("Bonus")]
    [SerializeField] private Bonus[] bonuses;
    [SerializeField] private bool applyBonuses = false;
    
    private ResourceEntry firstStructureResourceEntry;
    private List<ResourceEntry> bonusResourceEntries = new List<ResourceEntry>();

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
    private ResourceEntry disabledResourceEntry; // When production is disabled this is used to subtract from production
    private bool beingDestroyed = false;

    private float previousHealth = -1f; // This is -1 so on the first Update() it gets updated

    public delegate void StructureActionCallback(int playerID, int actionID, int[] configurationInts);
    public event StructureActionCallback OnStructureAction;
    
    public Vector2Int TileLocation { get => tileLocation; set => tileLocation = value; }
    public bool BeingDestroyed { get => beingDestroyed; set => beingDestroyed = value; }
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
        
        if (firstStructureResourceEntry != null) firstStructureResourceEntryInitialProduction = firstStructureResourceEntry.Change;
    }

    private void Update() {
        if (previousHealth != health.CurrentHealth) {
            previousHealth = health.CurrentHealth;

            USNL.PacketSend.StructureHealth(TileLocation, health.CurrentHealth, health.MaxHealth);
            if (health.CurrentHealth <= 0) {
                if (!beingDestroyed) GetComponent<Structure>().DontApplyRefunds = true; // If villager is not destroying this structure
                GetComponent<Structure>().DestroyStructure();
                VillagerManager.instance.RemoveDestroyStructure(TileLocation, this);
                StartCoroutine(VillagerManager.instance.UpdateVillagersConstructionDelayed(0.1f));
            }
        }
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

        if (ProductionSubtractor.instances.ContainsKey(GetComponent<Structure>().PlayerID) && ProductionSubtractor.instances[GetComponent<Structure>().PlayerID].DisabledStructures.ContainsKey(GetComponent<Structure>())) {
            ProductionSubtractor.instances[GetComponent<Structure>().PlayerID].DisabledStructures.Remove(GetComponent<Structure>());
        }
        
        if (disabledResourceEntry != null) ResourceManager.instances[GetComponent<Structure>().PlayerID].RemoveResourceEntry(disabledResourceEntry);
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

    #region Bonuses
    
    private void ApplyBonuses() {
        if (bonusResourceEntries == null) bonusResourceEntries = new List<ResourceEntry>();
        for (int i = 0; i < bonuses.Length; i++) {
            int bonus = GameUtils.GetDirectionsWithID(GetComponent<Structure>().Tile.Location, bonuses[i].BonusStructureID).Count * bonuses[i].BonusAmount;

            ResourceEntry resourceEntry = ScriptableObject.CreateInstance<ResourceEntry>();
            resourceEntry.ResourceId = firstStructureResourceEntry.ResourceId;
            resourceEntry.ResourceEntryIds = firstStructureResourceEntry.ResourceEntryIds;
            resourceEntry.Change = bonus;
            resourceEntry.ChangeOnTick = firstStructureResourceEntry.ChangeOnTick;
            bonusResourceEntries.Add(resourceEntry);
            ResourceManager.instances[GetComponent<Structure>().PlayerID].AddResourceEntry(resourceEntry);
        }
    }

    private void RemoveBonuses() {
        if (bonusResourceEntries == null) return;
        for (int i = 0; i < bonusResourceEntries.Count; i++) {
            ResourceManager.instances[GetComponent<Structure>().PlayerID].RemoveResourceEntry(bonusResourceEntries[i]);
        }
        bonusResourceEntries.Clear();
        bonusResourceEntries = null;
    }

    public void ReapplyBonuses() {
        RemoveBonuses();
        ApplyBonuses();
    }

    #endregion

    #region Other

    private void SetPathfindingNodesWalkable(bool _walkable) {
        for (int i = 0; i < unwalkableLocalPathfindingLocations.Length; i++) {
            gameplayTile.PathfindingLocationParent.PathfindingLocations[unwalkableLocalPathfindingLocations[i]].Walkable = _walkable;
        }
    }

    public void ToggleProduction(bool _toggle) {
        if (_toggle) {
            if (bonusResourceEntries == null) ApplyBonuses();
            if (disabledResourceEntry != null) { 
                ResourceManager.instances[GetComponent<Structure>().PlayerID].RemoveResourceEntry(disabledResourceEntry);
                disabledResourceEntry = null;
            }
            productionEnabled = true;
        } else {
            RemoveBonuses();
            disabledResourceEntry = ScriptableObject.CreateInstance<ResourceEntry>();
            disabledResourceEntry.ResourceId = firstStructureResourceEntry.ResourceId;
            disabledResourceEntry.ResourceEntryIds = firstStructureResourceEntry.ResourceEntryIds;
            disabledResourceEntry.Change = -firstStructureResourceEntry.Change;
            disabledResourceEntry.ChangeOnTick = firstStructureResourceEntry.ChangeOnTick;
            ResourceManager.instances[GetComponent<Structure>().PlayerID].AddResourceEntry(disabledResourceEntry);
            productionEnabled = false;
        }
    }

    #endregion
}
