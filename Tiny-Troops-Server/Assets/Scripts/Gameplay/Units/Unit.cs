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
    [Tooltip("Shown to dev only for debug purposes.")]
    [SerializeField] private int randomSeed;

    private int unitUUID;

    private PathfindingAgent pathfindingAgent;

    private bool previousFinishedMoving = true;

    public int PlayerID { get => playerID; set => playerID = value; }
    public int RandomSeed { get => randomSeed; set => randomSeed = value; }
    public int UnitUUID { get => unitUUID; set => unitUUID = value; }
    public PathfindingAgent PathfindingAgent { get => pathfindingAgent; set => pathfindingAgent = value; }

    public Vector2Int Location { get => pathfindingAgent.CurrentLocation; }
    public bool Selected { get => selectedIndicator.activeSelf; }

    private void Awake() {
        pathfindingAgent = GetComponent<PathfindingAgent>();

        // The if is here so configuration is easier on the Server
        if (selectedIndicator) selectedIndicator.SetActive(false);
    }

    private void Start() {
        UnitManager.instance.AddUnit(UnitUUID, gameObject, this, playerID, randomSeed);
        previousFinishedMoving = pathfindingAgent.FinishedMoving;
    }

    private void Update() {
        if (previousFinishedMoving != pathfindingAgent.FinishedMoving) {
            if (pathfindingAgent.FinishedMoving) StartCoroutine(SendLocation());
            previousFinishedMoving = pathfindingAgent.FinishedMoving;
        }
    }

    private void OnDestroy() {
        UnitManager.instance.RemoveUnit(UnitUUID);
    }

    public void ToggleSelectedIndicator(bool _toggle) {
        selectedIndicator.SetActive(_toggle);
    }

    private IEnumerator SendLocation() {
        yield return new WaitForSeconds(UnitManager.instance.UnitPositionSyncDelay);

        int nodeIndex = -1;
        
        for (int i = 0; i < TileManagement.instance.GetTileAtLocation(Location).Tile.GetComponent<GameplayTile>().TilePathfinding.NodesOnTile.Count; i++) {
            if (TileManagement.instance.GetTileAtLocation(Location).Tile.GetComponent<GameplayTile>().TilePathfinding.NodesOnTile[i] == pathfindingAgent.CurrentNode) {
                nodeIndex = i;
                break;
            }
        }

        if (nodeIndex >= 0) {
            USNL.PacketSend.SetUnitLocation(UnitUUID, Location, nodeIndex);
        }
    }
}
