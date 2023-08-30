using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClientStructureBuilder : MonoBehaviour {
    #region Variables

    public static ClientStructureBuilder instance;

    [SerializeField] private StructureBuildInfo[] structureBuildInfos;

    public StructureBuildInfo[] StructureBuildInfos { get => structureBuildInfos; set => structureBuildInfos = value; }

    // This queue is for building the bases after the tiles have been spawned, otherwise the bases are spawned when the game starts
    private List<BuildStructureInfo> buildStructureQueue = new List<BuildStructureInfo>();

    private struct BuildStructureInfo {
        public int PlayerID;
        public Vector2Int TargetTileLocation;
        public int StructureID;
        public bool ApplyCost;

        public BuildStructureInfo(int playerID, Vector2Int targetTileLocation, int structureID, bool applyCost) {
            PlayerID = playerID;
            TargetTileLocation = targetTileLocation;
            StructureID = structureID;
            ApplyCost = applyCost;
        }
    }


    #endregion

    #region Core

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (instance == null) instance = this;
        else {
            Debug.Log($"Server Structure Builder instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnBuildStructurePacket += OnBuildStructurePacket;
        MatchManager.OnMatchStateChanged += OnMatchStateChanged;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnBuildStructurePacket -= OnBuildStructurePacket;
        MatchManager.OnMatchStateChanged -= OnMatchStateChanged;
    }

    #endregion

    #region Builder

    private bool BuildStructure(int _playerID, Vector2Int _targetTileLocation, int _structureID, bool _applyCost = true) {
        if (TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile == null) {
            buildStructureQueue.Add(new BuildStructureInfo(_playerID, _targetTileLocation, _structureID, _applyCost = true));
            return false;
        }
        
        Transform structureLocationsParent = TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.StructureLocationsParent;
        Transform structureLoc;
        Transform tile;
        StructureBuildInfo sbi = structureBuildInfos[_structureID];

        // If can't afford structure - Not needed for Server-side building
        //if (!CanAffordStructure(_playerID, sbi) && _applyCost) return false;
        // If there is no available structure location
        if ((structureLoc = GetClosestAvailableStructureLocation(structureLocationsParent, sbi.StructureSize)) == null) return false;
        // If there is no Tile script on the parent
        if ((tile = structureLocationsParent.parent) == null) return false;
        // If tile is not taken by another player
        if (!CheckTileStructuresPlayerIDs(tile.GetComponent<Tile>(), _playerID)) return false;

        _applyCost = sbi.StructurePrefab.GetComponent<GameplayStructure>() && sbi.StructurePrefab.GetComponent<GameplayStructure>().ApplyCost;

        InstantiateStructure(structureLoc, structureLoc, _playerID, sbi, _applyCost);
        return true;
    }

    private bool DestroyStructure(int _playerID, Vector2Int _targetTileLocation) {
        Transform structureLocationsParent = TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.StructureLocationsParent;
        Transform structureLoc;
        Transform tile;
        
        // Checks to exit function
        if ((structureLoc = GetClosestUnavailableStructureLocation(structureLocationsParent)) == null) return false; // If there is no available structure location
        if ((tile = structureLocationsParent.parent) == null) return false; // If there is no Tile script on the parent
        if (CheckTileStructuresPlayerIDs(tile.GetComponent<Tile>(), _playerID) == false) return false; // If structure on the tile belongs to another player

        structureLoc.GetComponent<StructureLocation>().AssignedStructure.GetComponent<Structure>().DestroyStructure();
        return true;
    }

    private void InstantiateStructure(Transform _structureLocation, Transform _parent, int _playerID, StructureBuildInfo _structureBuildInfo, bool _applyCost) {
        GameObject newStructure = Instantiate(_structureBuildInfo.StructurePrefab, _structureLocation.position, Quaternion.identity, _parent);

        _structureLocation.GetComponent<StructureLocation>().AssignedStructure = newStructure;
        newStructure.GetComponent<Structure>().StructureLocation = _structureLocation.GetComponent<StructureLocation>();
        newStructure.GetComponent<Structure>().PlayerID = _playerID;

        if (_applyCost) ApplyStructureCost(_playerID, _structureBuildInfo);
    }

    #endregion

    #region Public Functions

    public void ReplaceStructure(Vector2Int _location, int _structureID, int _playerID) {
        DestroyStructure(_playerID, _location);
        BuildStructure(_playerID, _location, _structureID, false);
    }

    #endregion

    #region Builder Utils

    private Transform GetClosestAvailableStructureLocation(Transform _structureLocationsParent, int _structureSize) {
        List<Transform> structureLocs = GetStructureLocations(_structureLocationsParent, _structureSize);

        for (int i = 0; i < structureLocs.Count; i++) {
            if (structureLocs[i].GetComponent<StructureLocation>().AssignedStructure == null) {
                return structureLocs[i];
            }
        }
        return null;
    }

    private Transform GetClosestUnavailableStructureLocation(Transform _structureLocationsParent) {
        List<Transform> structureLocs = GetStructureLocations(_structureLocationsParent, -1);

        for (int i = 0; i < structureLocs.Count; i++) {
            if (structureLocs[i].GetComponent<StructureLocation>().AssignedStructure != null) {
                return structureLocs[i];
            }
        }
        return null;
    }

    // Returns all structure locations is structureSize <0
    private List<Transform> GetStructureLocations(Transform _structureLocationsParent, int _structureSize) {
        List<Transform> output = new List<Transform>();

        for (int i = 0; i < _structureLocationsParent.transform.childCount; i++) {
            // Return all structure locations if structureSize is lower than 0
            if (_structureSize < 0 && _structureLocationsParent.transform.GetChild(i).GetComponent<StructureLocation>()) {
                output.Add(_structureLocationsParent.transform.GetChild(i));
            } else if (_structureLocationsParent.transform.GetChild(i).GetComponent<StructureLocation>().StructureSize == _structureSize) {
                output.Add(_structureLocationsParent.transform.GetChild(i));
            }
        }

        // Sort by distance
        output = output.OrderBy((d) => (d.position - transform.position).sqrMagnitude).ToList();

        return output;
    }

    private bool CheckTileStructuresPlayerIDs(Tile _tile, int _playerID) {
        // Return false if there is another player's structure on the tile
        bool output = true;
        for (int i = 0; i < _tile.Structures.Count; i++) {
            if (_tile.Structures[i].PlayerID != _playerID & _tile.Structures[i].PlayerID != -1) output = false;
        }
        return output;
    }

    public bool CanAffordStructure(int _playerID, StructureBuildInfo _structureBuildInfo) {
        bool output = true;
        for (int i = 0; i < _structureBuildInfo.Cost.Length; i++) {
            if (_structureBuildInfo.Cost[i].Amount > ResourceManager.instances[_playerID].GetResource(_structureBuildInfo.Cost[i].Resource).Supply)
                output = false;
        }

        return output;
    }

    private void ApplyStructureCost(int _playerID, StructureBuildInfo _structureBuildInfo) {
        for (int i = 0; i < _structureBuildInfo.Cost.Length; i++) {
            ResourceManager.instances[_playerID].GetResource(_structureBuildInfo.Cost[i].Resource).Supply -= _structureBuildInfo.Cost[i].Amount;
        }
    }

    #endregion
    
    #region Client Build and Destroy

    public void BuildStructureClient(Vector2Int _targetTileLocation, int _structureID, StructureBuildInfo _sbi) {
        USNL.PacketSend.BuildStructure(MatchManager.instance.PlayerID, _targetTileLocation, _structureID);
    }

    public void DestroyStructureClient(Vector2Int _targetTileLocation) {
        USNL.PacketSend.DestroyStructure(MatchManager.instance.PlayerID, _targetTileLocation);
    }

    #endregion

    #region Callbacks

    private void OnBuildStructurePacket(object _packetObject) {
        USNL.BuildStructurePacket packet = (USNL.BuildStructurePacket)_packetObject;
        
        // If packet means to destroy a structure
        if (packet.StructureID < 0) {
            if (!DestroyStructure(packet.PlayerID, Vector2Int.RoundToInt(packet.TargetTileLocation))) {
                //Debug.Log($"Could not Destroy structure at location ({Vector2Int.RoundToInt(packet.TargetTileLocation)}) of StructureID ({packet.StructureID}) for PlayerID ({packet.PlayerID}).");
                // If can't build a structure, this is sent
            }
        } else if (!BuildStructure(packet.PlayerID, Vector2Int.RoundToInt(packet.TargetTileLocation), packet.StructureID)) {
            //Debug.Log($"Could not Build structure at location ({Vector2Int.RoundToInt(packet.TargetTileLocation)}) of StructureID ({packet.StructureID}) for PlayerID ({packet.PlayerID}).");
            // TODO, disabled this because some tiles spawn with sturctures already on them
        }
    }

    private void OnMatchStateChanged(MatchState _matchState) {
        if (_matchState == MatchState.Ended) {
            buildStructureQueue.Clear();
        }
    }

    public void BuildQueuedStructures() {
        for (int i = 0; i < buildStructureQueue.Count; i++) {
            BuildStructure(buildStructureQueue[i].PlayerID, buildStructureQueue[i].TargetTileLocation, buildStructureQueue[i].StructureID, buildStructureQueue[i].ApplyCost);
        }
        buildStructureQueue.Clear();
    }

    #endregion
}
