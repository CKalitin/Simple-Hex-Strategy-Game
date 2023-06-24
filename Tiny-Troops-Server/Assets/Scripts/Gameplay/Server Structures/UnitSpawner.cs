using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour {
    #region Variables

    [Header("Spawner")]
    [SerializeField] private PathfindingLocation spawnPathfindingLocation;

    [Header("Structure")]
    [SerializeField] private GameplayStructure gameplayStructure;

    private Tile tile;
    private Vector2Int location;

    List<UnitSpawnInfo> unitSpawnQueue = new List<UnitSpawnInfo>();

    private struct UnitSpawnInfo {
        private int playerID;
        private int unitID;
        private int[] configurationInts;

        public UnitSpawnInfo(int playerID, int unitID, int[] configurationInts) {
            this.playerID = playerID;
            this.unitID = unitID;
            this.configurationInts = configurationInts;
        }

        public int PlayerID { get => playerID; set => playerID = value; }
        public int UnitID { get => unitID; set => unitID = value; }
        public int[] ConfigurationInts { get => configurationInts; set => configurationInts = value; }
    }

    #endregion

    #region Core

    private void Awake() {
        GetTileParent();
        location = tile.TileInfo.Location;
    }

    private void OnEnable() { gameplayStructure.OnStructureAction += OnStructureAction; }
    private void OnDisable() { gameplayStructure.OnStructureAction -= OnStructureAction; }

    #endregion

    #region Spawning

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

    private void OnStructureAction(int _playerID, int _actionID, int[] _configurationInts) {
        // UnitID is ActionID - 1000 so other building scripts on the same tile can have different actions
        if (_actionID >= 1000) {
            unitSpawnQueue.Add(new UnitSpawnInfo(_playerID, _actionID - 1000, _configurationInts));
            
            ApplyUnitCosts(_playerID, UnitManager.instance.UnitPrefabs[_actionID - 1000].GetComponent<Unit>().UnitCost);

            // If SpawnUnitCoroutine is not already active
            if (unitSpawnQueue.Count <= 1) StartCoroutine(SpawnUnitCoroutine());
        }
    }

    private IEnumerator SpawnUnitCoroutine() {
        while (unitSpawnQueue.Count > 0) {
            yield return new WaitForSeconds(UnitManager.instance.UnitPrefabs[unitSpawnQueue[0].UnitID].GetComponent<Unit>().TrainingTime);
            SpawnUnit(unitSpawnQueue[0].PlayerID, unitSpawnQueue[0].UnitID, unitSpawnQueue[0].ConfigurationInts);
            unitSpawnQueue.RemoveAt(0);
        }
    }
    
    public void SpawnUnit(int _playerID, int _unitID, int[] _configurationInts) {
        int uuid = System.BitConverter.ToInt32(System.Guid.NewGuid().ToByteArray(), 0); // Generate UUID
        int randomSeed = Random.Range(0, 99999999);
        
        Vector3 p = spawnPathfindingLocation.transform.position;
        Vector3 pos = new Vector3(p.x + GameUtils.Random(randomSeed, -spawnPathfindingLocation.Radius, spawnPathfindingLocation.Radius), p.y, p.z + GameUtils.Random(randomSeed + 1, -spawnPathfindingLocation.Radius, spawnPathfindingLocation.Radius));

        int[] configurationInts = { uuid, randomSeed };
        USNL.PacketSend.StructureAction(_playerID, gameplayStructure.TileLocation, _unitID + 1000, configurationInts);

        GameObject unit = Instantiate(UnitManager.instance.UnitPrefabs[_unitID], pos, Quaternion.identity);
        unit.GetComponent<Unit>().PlayerID = _playerID;
        unit.GetComponent<Unit>().RandomSeed = randomSeed;
        unit.GetComponent<Unit>().UnitUUID = uuid;
        unit.GetComponent<PathfindingAgent>().Initialize(spawnPathfindingLocation.Location, unit.GetComponent<Unit>().RandomSeed);
    }

    private void ApplyUnitCosts(int _playerID, RBHKCost[] costs) {
        for (int i = 0; i < costs.Length; i++) {
            ResourceManager.instances[_playerID].GetResource(costs[i].Resource).Supply -= costs[i].Amount;
        }
        GameController.instance.SendResourcesPacketToAllClients();
    }

    #endregion
}
