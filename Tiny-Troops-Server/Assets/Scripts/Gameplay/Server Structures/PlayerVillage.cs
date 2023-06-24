using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USNL.Package;

public class PlayerVillage : MonoBehaviour {
    #region Variables
    
    [Header("Spawner")]
    [SerializeField] private PathfindingLocation spawnPathfindingLocation;
    [Space]
    [SerializeField] private GameObject villagerPrefab;

    private Dictionary<int, Villager> villagers = new Dictionary<int, Villager>();

    [Header("Structure")]
    [SerializeField] private GameplayStructure gameplayStructure;

    private Tile tile;

    public Dictionary<int, Villager> Villagers { get => villagers; set => villagers = value; }
    
    public Vector2Int Location { get => tile.TileInfo.Location; }
    public int PlayerID { get => gameplayStructure.GetComponent<Structure>().PlayerID; }

    #endregion

    #region Core

    private void Awake() {
        GetTileParent();
    }

    private void Start() {
        spawnPathfindingLocation = tile.GetComponent<GameplayTile>().PathfindingLocationParent.GetRandomCentralPathfindingLocation(PlayerID);

        if (!VillagerManager.instance.Villages.ContainsKey(PlayerID)) VillagerManager.instance.Villages.Add(PlayerID, new List<PlayerVillage>());
        VillagerManager.instance.Villages[PlayerID].Add(this);
        
        if (MatchManager.instance.MatchState != MatchState.Lobby) {
            SpawnVillager(gameplayStructure.GetComponent<Structure>().PlayerID, 0, new int[] { });
        }
    }
    
    private void GetTileParent() {
        Transform t = transform.parent;
        while (true) {
            if (t.GetComponent<Tile>()) {
                // If tile script found
                tile = t.GetComponent<Tile>();
                break;
            } else {
                // If we're at the top of the heirarchy, break out of the loop.
                if (t.parent == null) break;
                t = t.parent;
            }
        }
    }
    private void OnEnable() {
        gameplayStructure.OnStructureAction += SpawnVillager;
        USNL.CallbackEvents.OnUnitPathfindPacket += OnUnitPathfindPacket;
    }

    private void OnDisable() {
        VillagerManager.instance.Villages[PlayerID].Remove(this);
        
        gameplayStructure.OnStructureAction -= SpawnVillager;
        USNL.CallbackEvents.OnUnitPathfindPacket -= OnUnitPathfindPacket;
    }

    private void OnDestroy() {
        // Loop through villagers and destroy them all
        List<Villager> villagersList = new List<Villager>(villagers.Values);
        
        for (int i = 0; i < villagersList.Count; i++) {
            Destroy(villagersList[i].gameObject);
        }
        villagers.Clear();
    }

    #endregion

    #region Callbacks

    #endregion

    #region Utils

    private void SpawnVillager(int _playerID, int _actionID, int[] _configurationInts) {
        int uuid = System.BitConverter.ToInt32(System.Guid.NewGuid().ToByteArray(), 0); // Generate UUID
        int randomSeed = Random.Range(0, 99999999);
        
        Vector3 p = spawnPathfindingLocation.transform.position;
        Vector3 pos = new Vector3(p.x + GameUtils.Random(randomSeed + 2, -spawnPathfindingLocation.Radius, spawnPathfindingLocation.Radius), p.y, p.z + GameUtils.Random(randomSeed + 3, -spawnPathfindingLocation.Radius, spawnPathfindingLocation.Radius));
        
        int[] configurationInts = { uuid, randomSeed };
        USNL.PacketSend.StructureAction(_playerID, gameplayStructure.TileLocation, 0, configurationInts);
        Debug.Log(spawnPathfindingLocation.Location, spawnPathfindingLocation);
        GameObject villager = Instantiate(villagerPrefab, pos, Quaternion.identity);
        villager.GetComponent<PathfindingAgent>().Initialize(spawnPathfindingLocation.Location, randomSeed);
        villager.GetComponent<Villager>().VillagerUUID = uuid;
        villager.GetComponent<Villager>().Village = this;
        villagers.Add(uuid, villager.GetComponent<Villager>());

        VillagerManager.instance.UpdateVillagersConstruction();
    }
    
    private void OnUnitPathfindPacket(object _packetObject) {
        USNL.UnitPathfindPacket packet = (USNL.UnitPathfindPacket)_packetObject;

        for (int i = 0; i < packet.UnitUUIDs.Length; i++) {
            if (villagers.ContainsKey(packet.UnitUUIDs[i])) {
                Vector2Int targetLocation = TileManagement.instance.GetTileAtLocation(Vector2Int.RoundToInt(packet.TargetTileLocation)).Tile.GetComponent<GameplayTile>().PathfindingLocationParent.GetRandomCentralPathfindingLocation().Location;
                villagers[packet.UnitUUIDs[i]].GetComponent<PathfindingAgent>().PathfindToLocation(targetLocation);
                USNL.PacketSend.UnitPathfind(packet.UnitUUIDs, targetLocation, new Vector2[] { });
            }
        }
    }

    #endregion
}
