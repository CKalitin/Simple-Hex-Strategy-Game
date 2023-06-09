using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillagerManager : MonoBehaviour {
    #region Variables

    public static VillagerManager instance;

    [SerializeField] private float villagerConstructionTickTime = 0.25f;
    [Space]
    [SerializeField] private float villagerConstructionChangePerTick = 0.04f;

    private Dictionary<int, List<Villager>> villagers = new Dictionary<int, List<Villager>>();
    private Dictionary<int, List<PlayerVillage>> villages = new Dictionary<int, List<PlayerVillage>>();
    private Dictionary<int, List<ConstructionStructureInfo>> constructionStructures = new Dictionary<int, List<ConstructionStructureInfo>>();
    
    public float VillagerConstructionTickTime { get => villagerConstructionTickTime; set => villagerConstructionTickTime = value; }
    public float VillagerConstructionChangePerTick { get => villagerConstructionChangePerTick; set => villagerConstructionChangePerTick = value; }
    public Dictionary<int, List<Villager>> Villagers { get => villagers; set => villagers = value; }
    public Dictionary<int, List<PlayerVillage>> Villages { get => villages; set => villages = value; }

    private struct ConstructionStructureInfo {
        public Vector2Int Location;
        public ConstructionStructure ConstructionStructure;
        public List<Villager> ConstructionVillagers;

        public bool IsVillagerAssigned { get => ConstructionVillagers.Count > 0; }

        public ConstructionStructureInfo(Vector2Int location, ConstructionStructure constructionStructure, List<Villager> constructionVillagers) {
            Location = location;
            ConstructionStructure = constructionStructure;
            ConstructionVillagers = constructionVillagers;
        }
    }

    #endregion

    #region Core

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Villager Manager instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    private void Start() {
        StartCoroutine(VillagerConstructionCoroutine());
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnUnitPathfindPacket += OnUnitPathfindPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnUnitPathfindPacket -= OnUnitPathfindPacket;
    }

    #endregion

    #region Villager Construction

    private IEnumerator VillagerConstructionCoroutine() {
        while (true) {
            yield return new WaitForSeconds(VillagerConstructionTickTime);

            foreach (KeyValuePair<int, List<Villager>> kvp in villagers) {
                for (int i = 0; i < kvp.Value.Count; i++)
                    VillagerConstruction(kvp.Value[i]);
            }
        }
    }

    private void VillagerConstruction(Villager _villager) {
        if (_villager.PathfindingAgent.FinishedMoving == false) return;
        if (TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.CurrentTile).Tile.Structures.Count <= 0) return;
        if (!TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.CurrentTile).Tile.Structures[0].GetComponent<ConstructionStructure>()) return;

        TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.CurrentTile).Tile.Structures[0].GetComponent<ConstructionStructure>().ChangeBuildPercentage(villagerConstructionChangePerTick);
    }
    
    public void AddConstructionStructure(Vector2Int _location, ConstructionStructure _constructionStructure) {
        int playerID = _constructionStructure.PlayerID;
        if (!constructionStructures.ContainsKey(playerID)) constructionStructures.Add(playerID, new List<ConstructionStructureInfo>());
        constructionStructures[playerID].Add(new ConstructionStructureInfo(_location, _constructionStructure, new List<Villager>()));
    }

    public void RemoveConstructionStructure(ConstructionStructure _constructionStructure) {
        int playerID = _constructionStructure.PlayerID;
        if (!constructionStructures.ContainsKey(playerID)) return;

        for (int i = 0; i < constructionStructures[playerID].Count; i++) {
            if (constructionStructures[playerID][i].ConstructionStructure == _constructionStructure) {
                // Pathfind villagers back to village
                for (int x = 0; x < constructionStructures[playerID][i].ConstructionVillagers.Count; x++) {
                    Villager v = constructionStructures[playerID][i].ConstructionVillagers[x];
                    PathfindVillagerToTile(v, v.Village.Location);
                }
                constructionStructures[playerID].RemoveAt(i);
                break;
            }
        }
    }
    
    #endregion

    #region Villager Pathfinding

    // Get villagers to go to construction or back to village
    public void UpdateVillagersConstruction() {
        foreach (KeyValuePair<int, List<Villager>> kvp in villagers) {
            for (int x = 0; x < kvp.Value.Count; x++) {
                // If villager is not at village, not moving, and not constructing
                Villager v = kvp.Value[x];
                if (!IsVillagerAtVillage(v) && !IsVillagerMoving(v) && !IsVillagerAtConstruction(v)) {
                    PathfindVillagerToTile(v, v.Village.Location);
                }
            }
        }

        SetVillagersToConstruct();
    }

    private void SetVillagersToConstruct() {
        // Loop through each player's construction structures
        foreach (KeyValuePair<int, List<ConstructionStructureInfo>> kvp in constructionStructures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                CheckNullVillagers(kvp.Value[i]);
                ConstructStructure(kvp.Value[i]);
            }
        }
    }

    private void ConstructStructure(ConstructionStructureInfo _info) {
        if (_info.IsVillagerAssigned) return;

        List<PlayerVillage> playerVillages = GetClosestVillages(_info.ConstructionStructure.PlayerID, _info.Location);

        if (playerVillages == null) return;

        // Loop through player villages
        for (int i = 0; i < playerVillages.Count && i < 3; i++) {
            foreach (KeyValuePair<int, Villager> v in playerVillages[i].Villagers) {
                if (!IsVillagerMovingToConstruction(v.Value) && !IsVillagerAtConstruction(v.Value) && v.Value && !VillagerInConstructionInfosList(v.Value)) {
                    PathfindVillagerToTile(v.Value, _info.Location);
                    _info.ConstructionVillagers.Add(v.Value);
                    return;
                }
            }
        }
    }

    #endregion

    #region Utils

    private List<PlayerVillage> GetClosestVillages(int _playerID, Vector2Int _location) {
        if (!villages.ContainsKey(_playerID)) return null;

        List<PlayerVillage> playerVillages = villages[_playerID];
        playerVillages = playerVillages.OrderBy(x => Vector3.Distance(TileManagement.instance.TileLocationToWorldPosition(_location, 0), TileManagement.instance.TileLocationToWorldPosition(x.Location, 0))).ToList();

        return playerVillages;
    }

    private bool IsVillagerAtVillage(Villager _villager) {
        return _villager.Village.Location == _villager.Location;
    }

    private bool IsVillagerMoving(Villager _villager) {
        return !_villager.PathfindingAgent.FinishedMoving;
    }

    private bool IsVillagerAtConstruction(Villager _villager) {
        if (TileManagement.instance.GetTileAtLocation(_villager.Location).Tile.Structures.Count <= 0) return false;
        if (!TileManagement.instance.GetTileAtLocation(_villager.Location).Tile.Structures[0].GetComponent<ConstructionStructure>()) return false;
        return true;
    }

    private bool IsVillagerMovingToConstruction(Villager _villager) {
        if (TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.GetTargetLocation()).Tile == null) return false;
        if (TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.GetTargetLocation()).Tile.Structures.Count <= 0) return false;
        if (!TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.GetTargetLocation()).Tile.Structures[0].GetComponent<ConstructionStructure>()) return false;
        return true;
    }

    private bool VillagerInConstructionInfosList(Villager villager) {
        foreach (KeyValuePair<int, List<ConstructionStructureInfo>> kvp in constructionStructures) {
            for (int x = 0; x < kvp.Value.Count; x++) {
                if (kvp.Value[x].ConstructionVillagers.Contains(villager)) return true;
            }
        }
        return false;
    }

    private Villager GetVillager(int _uuid) {
        Villager v = null;

        foreach (KeyValuePair<int, List<Villager>> kvp in villagers) {
            for (int x = 0; x < kvp.Value.Count; x++) {
                if (kvp.Value[x].VillagerUUID == _uuid) {
                    return v;
                }
            }
        }

        return v;
    }

    private void RemoveConstructionVillager(Villager _v) {
        foreach (KeyValuePair<int, List<ConstructionStructureInfo>> kvp in constructionStructures) {
            for (int x = 0; x < kvp.Value.Count; x++) {
                if (kvp.Value[x].ConstructionVillagers.Contains(_v)) {
                    kvp.Value[x].ConstructionVillagers.Remove(_v);
                }
            }
        }
    }

    private void PathfindVillagerToTile(Villager v, Vector2Int _loc) {
        Vector2Int targetLocation = TileManagement.instance.GetTileAtLocation(_loc).Tile.GetComponent<GameplayTile>().PathfindingLocationParent.GetRandomCentralPathfindingLocation(v.PathfindingAgent.RandomSeed).Location;

        v.PathfindingAgent.PathfindToLocation(targetLocation);
        USNL.PacketSend.UnitPathfind(new int[1] { v.VillagerUUID }, targetLocation, new Vector2[] { });
    }

    private void CheckNullVillagers(ConstructionStructureInfo _info) {
        List<Villager> newConstructionVillagers = _info.ConstructionVillagers;
        for (int i = 0; i < _info.ConstructionVillagers.Count; i++) {
            if (_info.ConstructionVillagers[i] == null) newConstructionVillagers.Remove(_info.ConstructionVillagers[i]);
        }
        _info.ConstructionVillagers = newConstructionVillagers;
    }

    #endregion

    #region Callbacks

    private void OnUnitPathfindPacket(object _packetObject) {
        USNL.UnitPathfindPacket packet = (USNL.UnitPathfindPacket)_packetObject;

        Villager v = GetVillager(packet.UnitUUIDs[0]);

        if (v != null) {
            Vector2Int targetLocation = TileManagement.instance.GetTileAtLocation(Vector2Int.RoundToInt(packet.TargetTileLocation)).Tile.GetComponent<GameplayTile>().PathfindingLocationParent.GetRandomCentralPathfindingLocation().Location;
            
            RemoveConstructionVillager(v);
            v.PathfindingAgent.PathfindToLocation(targetLocation);
            USNL.PacketSend.UnitPathfind(new int[1] { v.VillagerUUID }, targetLocation, new Vector2[] { });
        }

        UpdateVillagersConstruction();
    }

    #endregion
}
