using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct UnitInfo {
    private GameObject gameObject;
    private Unit script;
    private int playerID;
    private int randomSeed;

    public UnitInfo(GameObject gameObject, Unit script, int playerID, int randomSeed) {
        this.gameObject = gameObject;
        this.script = script;
        this.playerID = playerID;
        this.randomSeed = randomSeed;
    }

    public GameObject GameObject { get => gameObject; set => gameObject = value; }
    public Unit Script { get => script; set => script = value; }
    public Vector2Int Location { get => script.Location; }
    public int PlayerID { get => playerID; set => playerID = value; }
    public int RandomSeed { get => randomSeed; set => randomSeed = value; }
}

public class UnitManager : MonoBehaviour {
    #region Variables

    public static UnitManager instance;

    [Header("Unit Management")]
    [Tooltip("Amount of time to wait after a unit arriving at it's destination before sending it's location to the Clients.")]
    [SerializeField] private float unitPositionSyncDelay = 1f;
    [Space]
    [SerializeField] private GameObject[] unitPrefabs;

    private Dictionary<int, UnitInfo> units = new Dictionary<int, UnitInfo>(); // The key is a UUID

    public float UnitPositionSyncDelay { get => unitPositionSyncDelay; set => unitPositionSyncDelay = value; }
    public GameObject[] UnitPrefabs { get => unitPrefabs; set => unitPrefabs = value; }
    public Dictionary<int, UnitInfo> Units { get => units; set => units = value; }

    #endregion

    #region Core

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (instance == null) instance = this;
        else {
            Debug.Log($"Unit Manager instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnUnitPathfindPacket += OnUnitPathfindPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnUnitPathfindPacket -= OnUnitPathfindPacket;
    }

    #endregion

    #region Unit Management

    public void AddUnit(int _uuid, GameObject _gameObject, Unit _script, int _playerID, int _randomSeed) {
        units.Add(_uuid, new UnitInfo(_gameObject, _script, _playerID, _randomSeed));
    }

    public void RemoveUnit(int _uuid) {
        units.Remove(_uuid);
    }

    public List<int> GetUnitsAtLocation(Vector2Int _location) {
        List<int> output = new List<int>(); // List of UUIDs
        foreach (KeyValuePair<int, UnitInfo> unit in units) {
            if (unit.Value.Location == _location) output.Add(unit.Key);
        }
        return output;
    }

    public List<int> GetUnitsOfIdAtLocation(Vector2Int _location, int _playerID) {
        List<int> output = GetUnitsAtLocation(_location);

        int numRemoved = 0;
        for (int i = 0; i < output.Count; i++) {
            if (units[output[i - numRemoved]].PlayerID != _playerID) {
                output.RemoveAt(i - numRemoved);
                numRemoved++;
            }
        }
        return output;
    }

    // List of player's with units at location
    public List<int> GetPlayerUnitsAtLocation(Vector2Int _location) {
        List<int> output = new List<int>();
        foreach (KeyValuePair<int, UnitInfo> unit in units) {
            if (unit.Value.Location == _location && !output.Contains(unit.Value.PlayerID)) output.Add(unit.Value.PlayerID);
        }
        return output;
    }

    public UnitInfo GetClosestEnemyUnitAtLocation(Vector2Int _location, int _playerID, Vector3 _pos) {
        List<int> unitsAtLocation = GetUnitsAtLocation(_location);
        if (unitsAtLocation.Count == 0) return new UnitInfo(null, null, -1, -1);

        int numRemoved = 0;
        for (int i = 0; i < unitsAtLocation.Count; i++) {
            if (units[i - numRemoved].PlayerID == _playerID) {
                unitsAtLocation.RemoveAt(i - numRemoved);
                numRemoved++;
            }
        }

        int closestUnitUUID = unitsAtLocation.OrderBy(o => Vector2.Distance(_pos, new Vector2(units[o].GameObject.transform.position.x, units[o].GameObject.transform.position.z))).ToArray()[0]; // Sort enemy units by (x, z) position
        return units[closestUnitUUID];
    }

    public void DestroyAllUnits() {
        foreach (KeyValuePair<int, UnitInfo> unit in units) {
            Destroy(unit.Value.GameObject);
        }
        units.Clear();
    }

    #endregion

    #region Packets & Callbacks

    private void OnUnitPathfindPacket(object _packetObject) {
        USNL.UnitPathfindPacket packet = (USNL.UnitPathfindPacket)_packetObject;

        Dictionary<Vector2Int, List<int>> unitUUIDsByLocation = new Dictionary<Vector2Int, List<int>>();
        
        // Sort unitUUIDs by location into unitUUIDsByLocation
        for (int i = 0; i < packet.UnitUUIDs.Length; i++) {
            if (unitUUIDsByLocation.ContainsKey(units[packet.UnitUUIDs[i]].Location)) unitUUIDsByLocation[units[packet.UnitUUIDs[i]].Location].Add(packet.UnitUUIDs[i]);
            else unitUUIDsByLocation.Add(units[packet.UnitUUIDs[i]].Location, new List<int>() { packet.UnitUUIDs[i] });
        }

        // Loop through unitUUIDsByLocation and create a path to the target location, then give that path to the units using units[uuid].PathfindtoLocation(path); I love copilot
        foreach (KeyValuePair<Vector2Int, List<int>> unitUUIDs in unitUUIDsByLocation) {
            List<Vector2Int> path = PathfindingManager.FindPath(unitUUIDs.Key, Vector2Int.RoundToInt(packet.TargetTileLocation));

            for (int i = 0; i < unitUUIDs.Value.Count; i++) {
                units[unitUUIDs.Value[i]].Script.PathfindingAgent.PathfindToLocation(unitUUIDs.Key, new List<Vector2Int>(path));
            }
            
            USNL.PacketSend.UnitPathfind(unitUUIDs.Value.ToArray(), packet.TargetTileLocation, path.Select(o => new Vector2(o.x, o.y)).ToArray());
        }
    }
    
    #endregion
}

