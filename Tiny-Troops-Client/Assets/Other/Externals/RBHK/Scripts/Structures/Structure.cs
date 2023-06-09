using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour {
    #region Variables

    [Header("Config")]
    [Tooltip("Id of the player associated with this Structure.")]
    [SerializeField] private int playerID;
    [Tooltip("Use this if you need to, it does nothing in RBHK itself.")]
    [SerializeField] private StructureID structureID;
    [SerializeField] private StructureBuildInfo structureBuildInfo;

    [Header("Upgrades")]
    [Tooltip("This field always has to be set, the first upgrade is what's used to initialize the resourceEntries.")]
    [SerializeField] private StructureUpgrade[] upgrades;
    
    [Tooltip("This is only public so you can see the value, changing it won't do anything.\nbtw hey there future chris")]
    private int upgradeIndex = 0;

    [Header("Other")]
    [Tooltip("These fields need to be specified if the Structure if placed in editor and not in game.\nIf it's placed in game using the Structure Building these fields are determiend.")]
    [SerializeField] private Tile tile;
    [Tooltip("These fields need to be specified if the Structure if placed in editor and not in game.\nIf it's placed in game using the Structure Building these fields are determiend.")]
    [SerializeField] private StructureLocation structureLocation;
    [Tooltip("If this is not specified by an upgrade, this is the default value.")]
    [SerializeField] ResourceEntry[] resourceEntries = new ResourceEntry[0];

    private List<int> appliedResourceEntryIndexes = new List<int>(); // Applied on Resource Management
    private List<ResourceModifier> appliedResourceModifiers = new List<ResourceModifier>(); // This is used to get which ResourceModifiers don't need to be updated again

    public int PlayerID { get => playerID; set => playerID = value; }
    public StructureID StructureID { get => structureID; set => structureID = value; }

    public int UpgradeIndex { get => upgradeIndex; set => upgradeIndex = value; }

    public StructureLocation StructureLocation { get => structureLocation; set => structureLocation = value; }
    public ResourceEntry[] ResourceEntries { get => resourceEntries; set => resourceEntries = value; }
    public Tile Tile { get => tile; set => tile = value; }

    #endregion

    #region Core

    private void Awake() {
        InitVars();

        InitializeResourceEntries();
        InstantiateCopiesOfResourceEntries();
    }

    private void Start() {
        // These functions are done in Start() because they require a playerId
        // This line is done in GetAndApplyResourceModifiers(), but if there are no RMs then it wouldn't be done, bug fixed!
        if (tile.ResourceModifiers.Count <= 0) AddResourceEntriesToManagement();
        if (TileManagement.instance.SpawningComplete) GetAndApplyResourceModifiers();
    }

    private void InitVars() {
        tile = GetTileParent(transform);
        if (!tile.Structures.Contains(this)) tile.Structures.Add(this);
    }

    public static Tile GetTileParent(Transform _transform) {
        Tile output = null;
        Transform t = _transform.parent;
        while (true) {
            if (t.GetComponent<Tile>()) {
                // If tile script found
                output = t.GetComponent<Tile>();
                break;
            } else {
                // If we're at the top of the heirarchy, break out of the loop.
                if (t.parent == null) break;
                t = t.parent;
            }
        }
        return output;
    }

    private void OnDestroy() {
        RemoveResourceEntriesFromManagement();
        tile.Structures.Remove(this);
    }

    public void DestroyStructure() {
        RefundNoTickCost();
        if (structureLocation.AssignedStructure) structureLocation.AssignedStructure = null;
        Destroy(gameObject);
    }

    #endregion

    #region Upgrading

    public void UpgradeToNextLevel() {
        if (CanAffordUpgrade()) {
            upgradeIndex = Mathf.Clamp(upgradeIndex + 1, 0, upgrades.Length - 1);
            ApplyUpgradeCost(upgrades[upgradeIndex]);
        }
    }

    public void UpgradeToNextLevelNoCost() {
        upgradeIndex = Mathf.Clamp(upgradeIndex + 1, 0, upgrades.Length - 1);
    }

    public bool CanAffordUpgrade() {
        int newUpgradeIndex = upgradeIndex + 1;
        if (newUpgradeIndex >= upgrades.Length) return false;

        bool output = true;
        for (int i = 0; i < upgrades[newUpgradeIndex].Cost.Length; i++) {
            if (upgrades[newUpgradeIndex].Cost[i].Amount >= ResourceManager.instances[playerID].GetResource(upgrades[newUpgradeIndex].Cost[i].Resource).Supply)
                output = false;
        }

        return output;
    }

    private void ApplyUpgradeCost(StructureUpgrade _su) {
        for (int i = 0; i < upgrades[upgradeIndex].Cost.Length; i++) {
            ResourceManager.instances[playerID].GetResource(upgrades[upgradeIndex].Cost[i].Resource).Supply -= upgrades[upgradeIndex].Cost[i].Amount;
        }
    }

    #endregion

    #region Public Resource Modifier Functions

    // Reapply resource modifiers on the tile of this structure
    public void UpdateResourceModifiers() {
        //ResetResourceEntries(); // Remove previous ResourceModifier changes
        //InstantiateCopiesOfResourceEntries(); // Create copies of the resource entries

        // Apply Resource Modifiers
        if (TileManagement.instance.SpawningComplete)
            GetAndApplyResourceModifiers();
    }

    #endregion

    #region Resource Modifiers

    // This is public because it's used by TileManagement
    private void InitializeResourceEntries() {
        if (upgrades.Length > 0) resourceEntries = upgrades[upgradeIndex].ResourceEntries;
    }

    // This function creates copies of the ResourceEntry Scriptable Objects so they don't affect the original ScriptableObject
    private void InstantiateCopiesOfResourceEntries() {
        // Create list for updated resource entries
        ResourceEntry[] _resourceEntries = new ResourceEntry[resourceEntries.Length];
        
        // Loop through resourceEntries
        for (int i = 0; i < resourceEntries.Length; i++) {
            ResourceEntry newResourceEntry = ScriptableObject.CreateInstance<ResourceEntry>(); // Create new resource entry

            // Set values of new resource  entry
            newResourceEntry.ResourceId = resourceEntries[i].ResourceId;
            newResourceEntry.ResourceEntryIds = resourceEntries[i].ResourceEntryIds;
            newResourceEntry.Change = resourceEntries[i].Change;
            newResourceEntry.ChangeOnTick = resourceEntries[i].ChangeOnTick;

            _resourceEntries[i] = newResourceEntry;
        }

        resourceEntries = _resourceEntries;
    }

    // Get resource modifiers on the tile of this structure and apply the modifier to the resource entries of this structure
    private void GetAndApplyResourceModifiers() {
        // Reset changes to ResourceManager resources by this script
        RemoveResourceEntriesFromManagement();

        List<ResourceModifier> applyResourceModifiers = new List<ResourceModifier>(); // These are the resource modifiers that need to be added
        List<ResourceModifier> removeResourceModifiers = new List<ResourceModifier>(); // These are the resource modifiers that need to be removed

        // Loop through ResourceModifiers on the tile and see which need to be added to this structure
        for (int i = 0; i < tile.ResourceModifiers.Count; i++) {
            if (!appliedResourceModifiers.Contains(tile.ResourceModifiers[i]) & (tile.ResourceModifiers[i].TargetPlayerID == playerID | tile.ResourceModifiers[i].TargetPlayerID == -1)) {
                applyResourceModifiers.Add(tile.ResourceModifiers[i]);
            }
        }

        // Loop through ResourceModifiers on this Structure and see which are not on the Tile and set them to be removed
        for (int i = 0; i < appliedResourceModifiers.Count; i++) {
            if (!tile.ResourceModifiers.Values.Contains(appliedResourceModifiers[i])) {
                removeResourceModifiers.Add(appliedResourceModifiers[i]);
            }
        }

        // Loop through resource entries
        for (int x = 0; x < resourceEntries.Length; x++) {
            // Apply resource modifiers
            for (int i = 0; i < applyResourceModifiers.Count; i++) {
                // If resourceId and resourceEntryId do not match, continue to next resourceEntry
                if (!CheckResourceIdMatch(applyResourceModifiers[i], resourceEntries[x])) continue;
                ApplyResourceModifier(applyResourceModifiers[i], resourceEntries[x]);
                appliedResourceModifiers.Add(tile.ResourceModifiers[i]);
            }

            // Check to Remove resource modifiers from this resource entry
            for (int i = 0; i < removeResourceModifiers.Count; i++) {
                // If resourceId and resourceEntryId do not match, continue to next resourceEntry
                if (!CheckResourceIdMatch(removeResourceModifiers[i], resourceEntries[x])) continue;
                RemoveResourceModifier(removeResourceModifiers[i], resourceEntries[x]);
                appliedResourceModifiers.Remove(removeResourceModifiers[i]);
            }
        }

        // Add changes to ResourceManager resources by this script
        AddResourceEntriesToManagement();
    }

    private void AddResourceEntriesToManagement() {
        for (int i = 0; i < resourceEntries.Length; i++) {
            if (playerID >= 0) appliedResourceEntryIndexes.Add(ResourceManager.instances[playerID].AddResourceEntry(resourceEntries[i]));
        }
    }
    
    private void RemoveResourceEntriesFromManagement() {
        for (int i = 0; i < appliedResourceEntryIndexes.Count; i++) {
            if (playerID >= 0) ResourceManager.instances[playerID].RemoveResourceEntry(appliedResourceEntryIndexes[0]); // Index is 0 because after this line index of 0 is deleted, so new 0 is previous index 1
        }
        appliedResourceEntryIndexes = new List<int>();
    }

    #endregion

    #region Resource Modifier Utils

    // Check if resourceIds and resourceEntryIds match between Resource Modifiers and ResourceEntries
    private bool CheckResourceIdMatch(ResourceModifier _rm, ResourceEntry _re) {
        // Check if the resourceId's don't match
        if (_rm.ResourceIdTarget != _re.ResourceId) return false;

        bool matchFound = false;
        // Check if resourceEntry has a matching resourceEntryId
        for (int i = 0; i < _re.ResourceEntryIds.Length; i++) {
            // Check if there is a match
            if (_re.ResourceEntryIds[i] == _rm.ResourceEntryIdTarget) matchFound = true;
        }

        return matchFound;
    }

    private void ApplyResourceModifier(ResourceModifier _rm, ResourceEntry _re) {
        float change = 0f;
        change += _rm.Change;
        if (GetDefaultResourceEntry(_re) != null)
            change += GetDefaultResourceEntry(_re).Change * _rm.PercentageChange; // Get default change value so multiple modifiers do not stack

        _re.Change = change;
    }

    private void RemoveResourceModifier(ResourceModifier _rm, ResourceEntry _re) {
        _re.Change = GetDefaultResourceEntry(_re).Change;
    }

    // Returns default value of resource entry (default from upgrade)
    private ResourceEntry GetDefaultResourceEntry(ResourceEntry _re) {
        for (int i = 0; i < resourceEntries.Length; i++) {
            if (resourceEntries[i].ResourceId != _re.ResourceId) continue;
            if (resourceEntries[i].ResourceEntryIds != _re.ResourceEntryIds) continue;
            return resourceEntries[i];
        }
        return null;
    }

    // No tick cost is costs that are applied to the supply and not demand. The changeOnTick toggle
    private void RefundNoTickCost() {
        if (playerID < 0) return;
        for (int i = 0; i < structureBuildInfo.Cost.Length; i++) {
            // If resource is updated on tick, skip it
            if (ResourceManager.instances[playerID].GetResource(structureBuildInfo.Cost[i].Resource).ChangeOnTickResource) continue;
             ResourceManager.instances[playerID].ChangeResource(structureBuildInfo.Cost[i].Resource, Mathf.Abs(structureBuildInfo.Cost[i].Amount));
        }
    }

    #endregion
}
