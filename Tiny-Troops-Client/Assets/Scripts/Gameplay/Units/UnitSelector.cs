using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelector : MonoBehaviour {
    public static UnitSelector instance;

    private Dictionary<int, UnitInfo> selectedUnits = new Dictionary<int, UnitInfo>();

    public Dictionary<int, UnitInfo> SelectedUnits { get => selectedUnits; set => selectedUnits = value; }

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
}
