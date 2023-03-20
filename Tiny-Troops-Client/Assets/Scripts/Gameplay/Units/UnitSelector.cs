using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelector : MonoBehaviour {
    public static UnitSelector instance;

    private List<UnitInfo> selectedUnits = new List<UnitInfo>();

    public List<UnitInfo> SelectedUnits { get => selectedUnits; set => selectedUnits = value; }

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Unit Selector instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
    }
}
