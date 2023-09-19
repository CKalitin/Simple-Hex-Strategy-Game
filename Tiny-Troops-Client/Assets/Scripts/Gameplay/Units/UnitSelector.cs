using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelector : MonoBehaviour {
    public static UnitSelector instance;

    private Dictionary<int, UnitInfo> selectedUnits = new Dictionary<int, UnitInfo>();
    private Dictionary<int, Dictionary<int, UnitInfo>> selectedUnitsById = new Dictionary<int, Dictionary<int, UnitInfo>>() { { 0, new Dictionary<int, UnitInfo>() }, { 1, new Dictionary<int, UnitInfo>() }, { 2, new Dictionary<int, UnitInfo>() }, { 3, new Dictionary<int, UnitInfo>() } };

    public Dictionary<int, UnitInfo> SelectedUnits { get => selectedUnits; set => selectedUnits = value; }
    public Dictionary<int, Dictionary<int, UnitInfo>> SelectedUnitsById { get => selectedUnitsById; set => selectedUnitsById = value; }

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Unit Selector instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    public void UpdateSelectedUnitsByID() {
        selectedUnitsById.Clear();
        selectedUnitsById.Add(0, new Dictionary<int, UnitInfo>());
        selectedUnitsById.Add(1, new Dictionary<int, UnitInfo>());
        selectedUnitsById.Add(2, new Dictionary<int, UnitInfo>());
        selectedUnitsById.Add(3, new Dictionary<int, UnitInfo>());

        // Loop through all selected units
        foreach (KeyValuePair<int, UnitInfo> _unit in selectedUnits) selectedUnitsById[_unit.Value.Script.UnitID].Add(_unit.Key, _unit.Value);
    }
}
