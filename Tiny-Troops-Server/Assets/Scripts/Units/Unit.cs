using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code to control units is in Tile Selector and Unit Selector
public class Unit : MonoBehaviour {
    [SerializeField] private GameObject selectedIndicator;
    [Space]
    [Tooltip("Shown to dev only for debug purposes.")]
    [SerializeField] private int playerID;

    private int unitUUID;

    private PathfindingAgent pathfindingAgent;

    public int PlayerID { get => playerID; set => playerID = value; }
    public int UnitUUID { get => unitUUID; }
    public Vector2Int Location { get => pathfindingAgent.CurrentLocation; }
    public bool Selected { get => selectedIndicator.activeSelf; }

    private void Awake() {
        pathfindingAgent = GetComponent<PathfindingAgent>();

        unitUUID = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0); // Generate UUID

        // The if is here so configuration is easier on the Server
        if (selectedIndicator) selectedIndicator.SetActive(false);
    }

    private void Start() {
        UnitManager.instance.AddUnit(UnitUUID, gameObject, this, playerID);
    }

    private void OnDestroy() {
        UnitManager.instance.RemoveUnit(UnitUUID);
    }

    public void ToggleSelectedIndicator(bool _toggle) {
        selectedIndicator.SetActive(_toggle);
    }
}
