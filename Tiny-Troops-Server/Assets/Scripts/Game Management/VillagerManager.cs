using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillagerManager : MonoBehaviour {
    #region Variables

    public static VillagerManager instance;

    [SerializeField] private float villagerConstructionTickTime = 3f;
    [Space]
    [SerializeField] private float villagerConstructionChangePerTick = 0.2f;

    private Dictionary<int, List<Villager>> villagers = new Dictionary<int, List<Villager>>();
    private Dictionary<int, List<PlayerVillage>> villages = new Dictionary<int, List<PlayerVillage>>();
    private Dictionary<int, List<ConstructionStructureInfo>> constructionStructures = new Dictionary<int, List<ConstructionStructureInfo>>();

    [SerializeField] private List<thing> things = new List<thing>();

    [Serializable]
    private struct thing {
        public int id;
        public Vector2Int Location;
        public ConstructionStructure ConstructionStructure;
        public List<Villager> ConstructionVillagers;

        public thing(int id, Vector2Int location, ConstructionStructure constructionStructure, List<Villager> constructionVillagers) {
            this.id = id;
            Location = location;
            ConstructionStructure = constructionStructure;
            ConstructionVillagers = constructionVillagers;
        }
    }

    public float VillagerConstructionTickTime { get => villagerConstructionTickTime; set => villagerConstructionTickTime = value; }
    public float VillagerConstructionChangePerTick { get => villagerConstructionChangePerTick; set => villagerConstructionChangePerTick = value; }
    public Dictionary<int, List<Villager>> Villagers { get => villagers; set => villagers = value; }
    public Dictionary<int, List<PlayerVillage>> Villages { get => villages; set => villages = value; }

    private struct ConstructionStructureInfo {
        public Vector2Int Location;
        public ConstructionStructure ConstructionStructure;
        public List<Villager> ConstructionVillagers;

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

    private void Update() {
        // Copy consrtuction structures into things
        things.Clear();
        foreach (KeyValuePair<int, List<ConstructionStructureInfo>> kvp in constructionStructures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                things.Add(new thing(kvp.Key, kvp.Value[i].Location, kvp.Value[i].ConstructionStructure, kvp.Value[i].ConstructionVillagers));
            }
        }
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
        if (TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.CurrentLocation).Tile.Structures.Count <= 0) return;
        if (!TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.CurrentLocation).Tile.Structures[0].GetComponent<ConstructionStructure>()) return;

        TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.CurrentLocation).Tile.Structures[0].GetComponent<ConstructionStructure>().ChangeBuildPercentage(villagerConstructionChangePerTick);
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
                constructionStructures[playerID].RemoveAt(i);
                break;
            }
        }
    }


    #endregion

    #region Villager Pathfinding

    // Get villagers to go to construction or back to village
    public void SetVillagersTargetLocation() {
        foreach (KeyValuePair<int, List<Villager>> kvp in villagers) {
            for (int x = 0; x < kvp.Value.Count; x++) {
                // If villager is not at village, not moving, and not constructing
                Villager v = kvp.Value[x];
                if (!IsVillagerAtVillage(v) && !IsVillagerMoving(v) && !IsVillagerAtConstruction(v)) {
                    v.PathfindingAgent.PathfindToLocation(v.Village.Location);
                    USNL.PacketSend.UnitPathfind(new int[1] { v.VillagerUUID }, v.Village.Location);
                }
            }
        }

        SetVillagersToConstruct();
    }

    private void SetVillagersToConstruct() {
        foreach (KeyValuePair<int, List<ConstructionStructureInfo>> kvp in constructionStructures) {
            for (int x = 0; x < kvp.Value.Count; x++) {
                if (kvp.Value[x].ConstructionVillagers.Count > 0) continue;
                
                PlayerVillage playerVillage = null;
                if ((playerVillage = GetClosestVillage(kvp.Value[x].ConstructionStructure.PlayerID, kvp.Value[x].Location)) != null) {
                    foreach (KeyValuePair<int, Villager> v in playerVillage.Villagers) {
                        // If villager is not moving to construction, and not at construction
                        if (!IsVillagerMovingToConstruction(v.Value) && !IsVillagerAtConstruction(v.Value)) {
                            v.Value.PathfindingAgent.PathfindToLocation(kvp.Value[x].Location);
                            kvp.Value[x].ConstructionVillagers.Add(v.Value);
                            USNL.PacketSend.UnitPathfind(new int[1] { v.Value.VillagerUUID }, kvp.Value[x].Location);
                            break;
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Utils

    private PlayerVillage GetClosestVillage(int _playerID, Vector2Int _location) {
        if (!villages.ContainsKey(_playerID)) return null;

        List<PlayerVillage> playerVillages = villages[_playerID];
        playerVillages = playerVillages.OrderBy(x => Vector3.Distance(TileManagement.instance.TileLocationToWorldPosition(_location, 0), TileManagement.instance.TileLocationToWorldPosition(x.Location, 0))).ToList();

        return playerVillages[0];
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
        if (TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.TargetLocation).Tile.Structures.Count <= 0) return false;
        if (!TileManagement.instance.GetTileAtLocation(_villager.PathfindingAgent.TargetLocation).Tile.Structures[0].GetComponent<ConstructionStructure>()) return false;
        return true;
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

    #endregion

    #region Callbacks

    private void OnUnitPathfindPacket(object _packetObject) {
        USNL.UnitPathfindPacket packet = (USNL.UnitPathfindPacket)_packetObject;

        Villager v = GetVillager(packet.UnitUUIDs[0]);

        if (v != null) {
            RemoveConstructionVillager(v);
            v.PathfindingAgent.PathfindToLocation(Vector2Int.RoundToInt(packet.TargetTileLocation));
            USNL.PacketSend.UnitPathfind(new int[1] { v.VillagerUUID }, Vector2Int.RoundToInt(packet.TargetTileLocation));
        }

        SetVillagersTargetLocation();
    }

    #endregion
}
