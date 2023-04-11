using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code to control units is in Tile Selector and Unit Selector
public class Unit : MonoBehaviour {
    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private Health health;
    [Space]
    [Tooltip("Shown to dev only for debug purposes.")]
    [SerializeField] private int playerID;
    [Tooltip("Shown to dev only for debug purposes.")]
    [SerializeField] private int randomSeed;

    private int unitUUID;

    private PathfindingAgent pathfindingAgent;

    public Health Health { get => health; set => health = value; }
    public int PlayerID { get => playerID; set => playerID = value; }
    public int RandomSeed { get => randomSeed; set => randomSeed = value; }
    
    public int UnitUUID { get => unitUUID; set => unitUUID = value; }
    public Vector2Int Location { get => pathfindingAgent.CurrentLocation; }
    public bool Selected { get => selectedIndicator.activeSelf; }
    public PathfindingAgent PathfindingAgent { get => pathfindingAgent; set => pathfindingAgent = value; }

    private void Awake() {
        pathfindingAgent = GetComponent<PathfindingAgent>();

        // The if is here so configuration is easier on the Server
        if (selectedIndicator) selectedIndicator.SetActive(false);
    }

    private void Start() {
        UnitManager.instance.AddUnit(UnitUUID, gameObject, this, playerID, randomSeed);
    }

    private void OnDestroy() {
        UnitManager.instance.RemoveUnit(UnitUUID);
    }

    public void ToggleSelectedIndicator(bool _toggle) {
        selectedIndicator.SetActive(_toggle);
    }
}
