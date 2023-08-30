using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerStructureBuilder : MonoBehaviour {
    #region Variables

    public static ServerStructureBuilder instance;

    [SerializeField] private StructureBuildInfo[] structureBuildInfos;

    public StructureBuildInfo[] StructureBuildInfos { get => structureBuildInfos; set => structureBuildInfos = value; }

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
        USNL.CallbackEvents.OnBuildStructurePacket += OnBuildSturcturePacket;
        USNL.CallbackEvents.OnDestroyStructurePacket += OnDestroyStructurePacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnBuildStructurePacket -= OnBuildSturcturePacket;
        USNL.CallbackEvents.OnDestroyStructurePacket -= OnDestroyStructurePacket;
    }
    
    #endregion

    #region Builder

    private void BuildStructure(int _playerID, Vector2Int _targetTileLocation, int _structureID, bool _applyCost = true) {
        Transform structureLocationsParent = TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.StructureLocationsParent;
        Transform structureLoc;
        Transform tile;
        StructureBuildInfo sbi = structureBuildInfos[_structureID];
        bool canBuild = true;

        // If can't afford structure
        if (!CanAffordStructure(_playerID, sbi) && _applyCost) canBuild = false;
        // If there is no available structure location
        if ((structureLoc = GetClosestAvailableStructureLocation(structureLocationsParent, sbi.StructureSize)) == null) canBuild = false;
        // If there is no Tile script on the parent
        if ((tile = structureLocationsParent.parent) == null) canBuild = false;
        // If tile is not taken by another player
        if (!CheckTileStructuresPlayerIDs(tile.GetComponent<Tile>(), _playerID)) canBuild = false;

        if (canBuild) {
            InstantiateStructure(structureLoc, structureLoc, _playerID, sbi);
            SendBuildStructurePacketToAllClients(_playerID, _targetTileLocation, _structureID);
        } else {
            // The -1 signifies the structure cannot be built
            SendBuildStructurePacketToAllClients(_playerID, _targetTileLocation, -1);
        }
    }

    private void InstantiateStructure(Transform _structureLocation, Transform _parent, int _playerID, StructureBuildInfo _structureBuildInfo, bool _applyCost = true) {
        GameObject newStructure = Instantiate(_structureBuildInfo.StructurePrefab, _structureLocation.position, Quaternion.identity, _parent);

        _structureLocation.GetComponent<StructureLocation>().AssignedStructure = newStructure;
        newStructure.GetComponent<Structure>().StructureLocation = _structureLocation.GetComponent<StructureLocation>();
        newStructure.GetComponent<Structure>().PlayerID = _playerID;

        if (_applyCost) ApplyStructureCost(_playerID, _structureBuildInfo);
    }

    private void DestroyStructure(int _playerID, Vector2Int _targetTileLocation) {
        Transform structureLocationsParent = TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.StructureLocationsParent;
        Transform structureLoc;
        Transform tile;

        // Checks to exit function
        if ((structureLoc = GetClosestUnavailableStructureLocation(structureLocationsParent)) == null) return; // If there is no available structure location
        if (TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.Structures[0].GetComponent<GameplayStructure>() == null) return; // If it doesn't have the GameplayStructure component, return
        if ((tile = structureLocationsParent.parent) == null) return; // If there is no Tile script on the parent
        if (CheckTileStructuresPlayerIDs(tile.GetComponent<Tile>(), _playerID) == false) return; // If structure on the tile belongs to another player

        structureLoc.GetComponent<StructureLocation>().AssignedStructure.GetComponent<Structure>().DestroyStructure();

        // The -1 signifies the structure will be destroyed
        SendBuildStructurePacketToAllClients(_playerID, _targetTileLocation, -1);
    }

    private void DestroyStrutureWithVillagers(int _playerID, Vector2Int _targetTileLocation) {
        Transform structureLocationsParent = TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.StructureLocationsParent;
        Transform structureLoc;
        Transform tile;

        // Checks to exit function
        if ((structureLoc = GetClosestUnavailableStructureLocation(structureLocationsParent)) == null) return; // If there is no available structure location
        if (TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.Structures[0].GetComponent<GameplayStructure>() == null) return; // If it doesn't have the GameplayStructure component, return
        if ((tile = structureLocationsParent.parent) == null) return; // If there is no Tile script on the parent
        if (CheckTileStructuresPlayerIDs(tile.GetComponent<Tile>(), _playerID) == false) return; // If structure on the tile belongs to another player

        VillagerManager.instance.AddDestroyStructure(_targetTileLocation, TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.Structures[0].GetComponent<GameplayStructure>(), _playerID);
        VillagerManager.instance.UpdateVillagersConstruction();
    }

    private void CancelDestroyStructure(Vector2Int _targetTileLocation, int _playerID) {
        Transform structureLocationsParent = TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.StructureLocationsParent;
        Transform structureLoc;
        Transform tile;

        // Checks to exit function
        if ((structureLoc = GetClosestUnavailableStructureLocation(structureLocationsParent)) == null) return; // If there is no available structure location
        if (TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.Structures[0].GetComponent<GameplayStructure>() == null) return; // If it doesn't have the GameplayStructure component, return
        if ((tile = structureLocationsParent.parent) == null) return; // If there is no Tile script on the parent
        // No need to check if the tile belongs to this player, that is done on the client. The structure UI is only shown if the tile belongs to the player

        VillagerManager.instance.RemoveDestroyStructure(_targetTileLocation, TileManagement.instance.GetTileAtLocation(_targetTileLocation).Tile.Structures[0].GetComponent<GameplayStructure>());
        VillagerManager.instance.UpdateVillagersConstruction();
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
            if (_tile.Structures[i].PlayerID != _playerID && _tile.Structures[i].PlayerID != -1) output = false;
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

    public void SendBuildStructurePacketToAllClients(int _playerID, Vector2Int _targetTileLocation, int _structureID) {
        int[] connectedClientIDs = USNL.ServerManager.GetConnectedClientIds();
        for (int i = 0; i < connectedClientIDs.Length; i++) {
            USNL.PacketSend.BuildStructure(connectedClientIDs[i], _playerID, _targetTileLocation, _structureID);
        }
    }

    #endregion

    #region Callbacks

    private void OnBuildSturcturePacket(object _packetObject) {
        USNL.BuildStructurePacket packet = (USNL.BuildStructurePacket)_packetObject;
        
        BuildStructure(packet.PlayerID, Vector2Int.RoundToInt(packet.TargetTileLocation), packet.StructureID);
    }

    private void OnDestroyStructurePacket(object _packetObject) {
        USNL.DestroyStructurePacket packet = (USNL.DestroyStructurePacket)_packetObject;

        if (packet.PlayerID <= -10) CancelDestroyStructure(Vector2Int.RoundToInt(packet.TargetTileLocation), Mathf.Abs(packet.PlayerID + 10));
        else DestroyStrutureWithVillagers(packet.PlayerID, Vector2Int.RoundToInt(packet.TargetTileLocation));
    }

    #endregion
}
