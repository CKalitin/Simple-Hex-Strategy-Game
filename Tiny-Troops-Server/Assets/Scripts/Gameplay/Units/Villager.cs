using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : MonoBehaviour {
    #region Variables

    [Header("References")]
    [SerializeField] private Health health;
    
    private int villagerUUID;
    private int playerID;
    
    private bool previousFinishedMoving = true;
    private float previousHealth = -1f; // This is -1 so on the first Update() it gets updated

    private PathfindingAgent pathfindingAgent;
    private PlayerVillage village;

    public int VillagerUUID { get => villagerUUID; set => villagerUUID = value; }
    public int PlayerID { get => playerID; set => playerID = value; }
    public PathfindingAgent PathfindingAgent { get => pathfindingAgent; set => pathfindingAgent = value; }
    public PlayerVillage Village { get => village; set => village = value; }

    public Vector2Int Location { get => pathfindingAgent.CurrentLocation; }

    #endregion

    #region Core

    private void Awake() {
        pathfindingAgent = GetComponent<PathfindingAgent>();

        if (!VillagerManager.instance.Villagers.ContainsKey(PlayerID)) VillagerManager.instance.Villagers.Add(PlayerID, new List<Villager>());
        VillagerManager.instance.Villagers[playerID].Add(this);
    }

    private void OnDestroy() {
        if (VillagerManager.instance.Villagers.ContainsKey(PlayerID) && VillagerManager.instance.Villagers[playerID].Contains(this))
            VillagerManager.instance.Villagers[playerID].Remove(this);
        
        if (village.Villagers.ContainsKey(VillagerUUID))
            village.Villagers.Remove(VillagerUUID);
    }

    private void Update() {
        if (previousFinishedMoving != pathfindingAgent.FinishedMoving) {
            if (pathfindingAgent.FinishedMoving) StartCoroutine(SendLocation());
            previousFinishedMoving = pathfindingAgent.FinishedMoving;
            VillagerManager.instance.SetVillagersTargetLocation();
        }

        if (previousHealth != health.CurrentHealth) {
            previousHealth = health.CurrentHealth;

            USNL.PacketSend.UnitHealth(villagerUUID, health.CurrentHealth, health.MaxHealth);

            if (health.CurrentHealth <= 0) Destroy(gameObject);
        }
    }

    #endregion

    #region Villager

    private IEnumerator SendLocation() {
        yield return new WaitForSeconds(UnitManager.instance.UnitPositionSyncDelay);

        if (pathfindingAgent.FinishedMoving == false) yield break;

        int nodeIndex = -1;

        for (int i = 0; i < TileManagement.instance.GetTileAtLocation(Location).Tile.GetComponent<GameplayTile>().GetTilePathfinding().NodesOnTile.Count; i++) {
            if (TileManagement.instance.GetTileAtLocation(Location).Tile.GetComponent<GameplayTile>().GetTilePathfinding().NodesOnTile[i] == pathfindingAgent.CurrentNode) {
                nodeIndex = i;
                break;
            }
        }

        if (nodeIndex >= 0) {
            USNL.PacketSend.SetUnitLocation(villagerUUID, Location, nodeIndex, new Vector2(transform.position.x, transform.position.z));
        }
    }

    #endregion
}
