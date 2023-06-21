using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private GameObject[] unitPrefabs;

    private Dictionary<int, UnitInfo> units = new Dictionary<int, UnitInfo>(); // The key is a UUID

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
        USNL.CallbackEvents.OnSetUnitLocationPacket += OnSetUnitLocationPacket;
        USNL.CallbackEvents.OnUnitHealthPacket += OnUnitHealthPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnUnitPathfindPacket -= OnUnitPathfindPacket;
        USNL.CallbackEvents.OnSetUnitLocationPacket -= OnSetUnitLocationPacket;
        USNL.CallbackEvents.OnUnitHealthPacket -= OnUnitHealthPacket;
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
        List<int> unitsAtLocation = GetUnitsAtLocation(_location);
        List<int> output = new List<int>();

        for (int i = 0; i < unitsAtLocation.Count; i++) {
            if (units[unitsAtLocation[i]].PlayerID == _playerID) {
                output.Add(unitsAtLocation[i]);
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

        List<Vector2Int> path = packet.Path.Select(o => new Vector2Int(Mathf.RoundToInt(o.x), Mathf.RoundToInt(o.y))).ToList();

        for (int i = 0; i < packet.UnitUUIDs.Length; i++) {
            if (!units.ContainsKey(packet.UnitUUIDs[i])) return;
            if (path.Count <= 0) return; // This line needs to be here to prevent a bug (shrug face)
            
            units[packet.UnitUUIDs[i]].GameObject.GetComponent<PathfindingAgent>().PathfindToLocation(Vector2Int.RoundToInt(packet.TargetTileLocation), new List<Vector2Int>(path));
            
        }
    }

    private void OnSetUnitLocationPacket(object _packetObject) {
        USNL.SetUnitLocationPacket packet = (USNL.SetUnitLocationPacket)_packetObject;

        if (units.ContainsKey(packet.UnitUUID)) {
            units[packet.UnitUUID].GameObject.GetComponent<PathfindingAgent>().SetLocation(Vector2Int.RoundToInt(packet.TargetTileLocation), packet.PathfindingNodeIndex, packet.Position);
        }
    }

    private void OnUnitHealthPacket(object _packetObject) {
        USNL.UnitHealthPacket packet = (USNL.UnitHealthPacket)_packetObject;

        if (units.ContainsKey(packet.UnitUUID)) {
            units[packet.UnitUUID].Script.Health.SetHealth(packet.Health, packet.MaxHealth);
        } //else Debug.Log("Desync Detected"); Lots of these desysnsc, should be fine if Server and Client health is set right from the begining
    }

    #endregion
}
