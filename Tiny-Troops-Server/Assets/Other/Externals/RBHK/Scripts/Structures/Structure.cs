using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structure : MonoBehaviour {
    #region Variables

    [Header("Config")]
    [Tooltip("Id of the player associated with this Structure.")]
    [SerializeField] private int playerID;
    [Tooltip("Use this if you need to, it does nothing in RBHK itself.")]
    [SerializeField] private StructureID structureID;
    [SerializeField] private StructureBuildInfo structureBuildInfo;
    [Space]
    [SerializeField] private GameResource[] alwaysApplyCostResources;
    [Space]
    [Tooltip("Rounded refunds")]
    [SerializeField] private float refundPercentage = 1;
    [Tooltip("These are refunded even if refunds are disabled.")]
    [SerializeField] GameResource[] fullRefundResources;
    [Space]
    [SerializeField] private bool applyCost = true;
    private bool applyFullRefunds = false;
    private bool dontApplyRefunds = false;

    [Header("Upgrades")]
    [Tooltip("This field always has to be set, the first upgrade is what's used to initialize the resourceEntries.")]
    [SerializeField] private StructureUpgrade[] upgrades;

    [Tooltip("This is only public so you can see the value, changing it won't do anything.\nbtw hey there future chris - past chris was a dumbass")]
    private int upgradeIndex = 0;

    [Header("Other")]
    [Tooltip("These fields need to be specified if the Structure if placed in editor and not in game.\nIf it's placed in game using the Structure Building these fields are determiend.")]
    [SerializeField] private Tile tile;
    [Tooltip("These fields need to be specified if the Structure if placed in editor and not in game.\nIf it's placed in game using the Structure Building these fields are determiend.")]
    [SerializeField] private StructureLocation structureLocation;
    [Tooltip("If this is not specified by an upgrade, this is the default value.")]
    [SerializeField] ResourceEntry[] resourceEntries = new ResourceEntry[0];

    private List<ResourceEntry> costResourceEntries = new List<ResourceEntry>();

    public int PlayerID { get => playerID; set => playerID = value; }
    public StructureID StructureID { get => structureID; set => structureID = value; }
    public GameResource[] FullRefundResources { get => fullRefundResources; set => fullRefundResources = value; }
    public float RefundPercentage { get => refundPercentage; set => refundPercentage = value; }
    public bool ApplyFullRefunds { get => applyFullRefunds; set => applyFullRefunds = value; }
    public bool DontApplyRefunds { get => dontApplyRefunds; set => dontApplyRefunds = value; }
    public int UpgradeIndex { get => upgradeIndex; set => upgradeIndex = value; }

    public StructureLocation StructureLocation { get => structureLocation; set => structureLocation = value; }
    public ResourceEntry[] ResourceEntries { get => resourceEntries; set => resourceEntries = value; }
    public Tile Tile { get => tile; set => tile = value; }
    public StructureBuildInfo StructureBuildInfo { get => structureBuildInfo; set => structureBuildInfo = value; }

    #endregion

    #region Core

    private void Awake() {
        InitVars();

        InitializeResourceEntries();
        InstantiateCopiesOfResourceEntries();
    }

    private void Start() {
        // These functions are done in Start() because they require a playerId
        AddResourceEntriesToManagement();
        ApplyCost();
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
        RefundCost();
        RemoveResourceEntriesFromManagement();
        tile.Structures.Remove(this);
    }

    public void DestroyStructure() {
        if (structureLocation && structureLocation.AssignedStructure) structureLocation.AssignedStructure = null;
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

    #region Resource Entries

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

    private void AddResourceEntriesToManagement() {
        for (int i = 0; i < resourceEntries.Length; i++) {
            if (playerID >= 0) ResourceManager.instances[playerID].AddResourceEntry(resourceEntries[i]);
        }
    }

    private void RemoveResourceEntriesFromManagement() {
        for (int i = 0; i < resourceEntries.Length; i++) {
            if (playerID >= 0) ResourceManager.instances[playerID].RemoveResourceEntry(resourceEntries[0]);
        }
    }

    #endregion

    #region Utils

    public void ApplyCost() {
        if (playerID < 0) return;
        if (!applyCost) { AlwaysApplyCostResoucres(); return; }

        for (int i = 0; i < structureBuildInfo.Cost.Length; i++) {
            costResourceEntries.Add(ScriptableObject.CreateInstance<ResourceEntry>());
            costResourceEntries[i].ResourceId = structureBuildInfo.Cost[i].Resource;
            costResourceEntries[i].Change = -Mathf.Abs(structureBuildInfo.Cost[i].Amount);
            costResourceEntries[i].ChangeOnTick = ResourceManager.instances[playerID].Resources[(int)structureBuildInfo.Cost[i].Resource].ChangeOnTickResource;
            ResourceManager.instances[playerID].AddResourceEntry(costResourceEntries[i]);
        }
    }

    public void RefundCost() {
        if (playerID < 0) return;
        if (!applyCost) { AlwaysApplyCostResoucresRefund(); return; }
        
        for (int i = 0; i < costResourceEntries.Count; i++) {
            if (fullRefundResources.Contains(costResourceEntries[i].ResourceId)) { ResourceManager.instances[playerID].RemoveResourceEntry(costResourceEntries[i]); continue; }
            if (dontApplyRefunds) continue;
            if (applyFullRefunds) ResourceManager.instances[playerID].RemoveResourceEntry(costResourceEntries[i]);
            else ApplyPartialRefunds();
        }
    }

    private void AlwaysApplyCostResoucres() {
        for (int i = 0; i < structureBuildInfo.Cost.Length; i++) {
            if (!alwaysApplyCostResources.Contains(structureBuildInfo.Cost[i].Resource)) continue;
            costResourceEntries.Add(ScriptableObject.CreateInstance<ResourceEntry>());
            costResourceEntries[i].ResourceId = structureBuildInfo.Cost[i].Resource;
            costResourceEntries[i].Change = -Mathf.Abs(structureBuildInfo.Cost[i].Amount);
            costResourceEntries[i].ChangeOnTick = ResourceManager.instances[playerID].Resources[(int)structureBuildInfo.Cost[i].Resource].ChangeOnTickResource;
            ResourceManager.instances[playerID].AddResourceEntry(costResourceEntries[i]);
        }
    }

    private void AlwaysApplyCostResoucresRefund() {
        for (int i = 0; i < costResourceEntries.Count; i++) {
            if (!alwaysApplyCostResources.Contains(costResourceEntries[i].ResourceId)) continue;
            
            if (fullRefundResources.Contains(costResourceEntries[i].ResourceId)) { ResourceManager.instances[playerID].RemoveResourceEntry(costResourceEntries[i]); continue; }
            if (dontApplyRefunds) continue;
            if (applyFullRefunds) ResourceManager.instances[playerID].RemoveResourceEntry(costResourceEntries[i]);
            else ApplyPartialRefunds();
        }
    }

    private void ApplyPartialRefunds() {
        for (int i = 0; i < costResourceEntries.Count; i++) {
            if (fullRefundResources.Contains(costResourceEntries[i].ResourceId)) continue;
            ResourceEntry re = ScriptableObject.CreateInstance<ResourceEntry>();
            re.ResourceId = structureBuildInfo.Cost[i].Resource;
            re.Change = Mathf.Abs(Mathf.Round(costResourceEntries[i].Change * refundPercentage));
            re.ChangeOnTick = ResourceManager.instances[playerID].Resources[(int)structureBuildInfo.Cost[i].Resource].ChangeOnTickResource;
            
            ResourceManager.instances[playerID].AddResourceEntry(re);
        }
        costResourceEntries.Clear();
    }

    #endregion
}