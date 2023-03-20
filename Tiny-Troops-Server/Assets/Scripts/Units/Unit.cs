using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
    [SerializeField] private int playerID;

    private int unitUUID;

    private PathfindingAgent pathfindingAgent;

    public int PlayerID { get => playerID; set => playerID = value; }
    public int UnitUUID { get => unitUUID; }
    public Vector2Int Location { get => pathfindingAgent.CurrentLocation; }
    
    private void Awake() {
        pathfindingAgent = GetComponent<PathfindingAgent>();
        
        unitUUID = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0); // Generate UUID
    }

    private void Start() {
        UnitManager.instance.AddUnit(UnitUUID, gameObject, this, Location, playerID);
    }

    private void OnDestroy() {
        UnitManager.instance.RemoveUnit(UnitUUID);
    }
}
