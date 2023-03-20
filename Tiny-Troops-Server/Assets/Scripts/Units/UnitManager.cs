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

    public UnitInfo(GameObject gameObject, Unit script, int playerID) {
        this.gameObject = gameObject;
        this.script = script;
        this.playerID = playerID;
    }

    public GameObject GameObject { get => gameObject; set => gameObject = value; }
    public Unit Script { get => script; set => script = value; }
    public Vector2Int Location { get => script.Location; set => gameObject.GetComponent<PathfindingAgent>().SetLocation(value); }
    public int PlayerID { get => playerID; set => playerID = value; }
}


public class UnitManager : MonoBehaviour {
    public static UnitManager instance;

    private Dictionary<int, UnitInfo> units = new Dictionary<int, UnitInfo>(); // The key is a UUID

    public Dictionary<int, UnitInfo> Units { get => units; set => units = value; }

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (instance == null) instance = this;
        else {
            Debug.Log($"Unit Manager instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
    }

    public void AddUnit(int _uuid, GameObject _gameObject, Unit _script, int _playerID) {
        units.Add(_uuid, new UnitInfo(_gameObject, _script, _playerID));
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

    public UnitInfo GetClosestEnemyUnitAtLocation(Vector2Int _location, int _playerID, Vector3 _pos) {
        List<int> unitsAtLocation = GetUnitsAtLocation(_location);
        if (unitsAtLocation.Count == 0) return new UnitInfo(null, null, -1);

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
}
