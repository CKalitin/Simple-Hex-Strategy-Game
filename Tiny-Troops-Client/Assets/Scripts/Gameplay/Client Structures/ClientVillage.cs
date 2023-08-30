using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientVillage : MonoBehaviour {
    #region Variables

    public static Dictionary<Vector2Int, ClientVillage> instances = new Dictionary<Vector2Int, ClientVillage>();

    [Header("Spawner")]
    [SerializeField] private PathfindingLocation spawnPathfindingLocation;
    [Space]
    [SerializeField] private GameObject villagerPrefab;

    private Dictionary<int, GameObject> villagers = new Dictionary<int, GameObject>();
    
    
    [Header("References")]
    [SerializeField] private GameplayStructure gameplayStructure;

    private float delayBetweenTileHighlights = 0.1f;
    private float tileHighlightTime = 1f;
    
    private Tile tile;

    public Vector2Int Location { get => tile.TileInfo.Location; }
    public int PlayerID { get => gameplayStructure.GetComponent<Structure>().PlayerID; }

    #endregion

    #region Core

    private void Awake() {
        GetTileParent();
        instances.Add(Location, this);
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

    private void Start() {
        spawnPathfindingLocation = tile.GetComponent<GameplayTile>().PathfindingLocationParent.GetRandomCentralPathfindingLocation(PlayerID);
    }

    private void OnEnable() {
        gameplayStructure.OnStructureAction += OnStructureAction;
        USNL.CallbackEvents.OnUnitPathfindPacket += OnUnitPathfindPacket;
        USNL.CallbackEvents.OnSetUnitLocationPacket += OnSetUnitLocationPacket;
        USNL.CallbackEvents.OnUnitHealthPacket += OnUnitHealthPacket;
    }

    private void OnDisable() {
        gameplayStructure.OnStructureAction -= OnStructureAction;
        USNL.CallbackEvents.OnUnitPathfindPacket -= OnUnitPathfindPacket;
        USNL.CallbackEvents.OnSetUnitLocationPacket -= OnSetUnitLocationPacket;
        USNL.CallbackEvents.OnUnitHealthPacket -= OnUnitHealthPacket;
    }

    private void OnDestroy() {
        // Loop through villagers and destroy them all
        foreach (KeyValuePair<int, GameObject> villager in villagers) {
            Destroy(villager.Value);
        }
        villagers.Clear();

        instances.Remove(Location);
    }

    #endregion

    #region Callbacks

    private void OnStructureAction(int _playerID, int _actionID, int[] _configurationInts) {
        if (_actionID == 0) {
            for (int i = 0; i < _configurationInts.Length; i += 2) {
                SpawnVillager(_configurationInts[i], _configurationInts[i + 1]);
            }
        }
    }

    private void OnUnitPathfindPacket(object _packetObject) {
        USNL.UnitPathfindPacket packet = (USNL.UnitPathfindPacket)_packetObject;

        for (int i = 0; i < packet.UnitUUIDs.Length; i++) {
            if (villagers.ContainsKey(packet.UnitUUIDs[i])) {
                //StartCoroutine(HighlightPath(villagers[packet.UnitUUIDs[i]].GetComponent<PathfindingAgent>().CurrentLocation, Vector2Int.RoundToInt(packet.TargetTileLocation)));
                villagers[packet.UnitUUIDs[i]].GetComponent<PathfindingAgent>().PathfindToLocation(Vector2Int.RoundToInt(packet.TargetTileLocation));
            }
        }
    }

    private void OnSetUnitLocationPacket(object _packetObject) {
        USNL.SetUnitLocationPacket packet = (USNL.SetUnitLocationPacket)_packetObject;

        if (villagers.ContainsKey(packet.UnitUUID)) {
            Vector3 pos = new Vector3(packet.Position.x, 0, packet.Position.y);
            villagers[packet.UnitUUID].GetComponent<PathfindingAgent>().SetLocation(Vector2Int.RoundToInt(packet.TargetTileLocation), pos);
        }
    }

    private void OnUnitHealthPacket(object _packetObject) {
        USNL.UnitHealthPacket packet = (USNL.UnitHealthPacket)_packetObject;

        if (villagers.ContainsKey(packet.UnitUUID)) {
            villagers[packet.UnitUUID].GetComponent<Health>().SetHealth(packet.Health, packet.MaxHealth);
        } //else Debug.Log("Desync Detected"); Lots of these desysnsc, should be fine if Server and Client health is set right from the begining
    }

    #endregion

    #region Utils

    private void SpawnVillager(int _villagerUUID, int _randomSeed) {
        if (spawnPathfindingLocation == null) 
            spawnPathfindingLocation = tile.GetComponent<GameplayTile>().PathfindingLocationParent.GetRandomCentralPathfindingLocation(PlayerID);

        Vector3 p = spawnPathfindingLocation.transform.position;
        Vector3 pos = new Vector3(p.x + GameUtils.Random(_randomSeed + 2, -spawnPathfindingLocation.Radius, spawnPathfindingLocation.Radius), p.y, p.z + GameUtils.Random(_randomSeed + 3, -spawnPathfindingLocation.Radius, spawnPathfindingLocation.Radius));

        GameObject villager = Instantiate(villagerPrefab, pos, Quaternion.identity);
        
        villager.GetComponent<PathfindingAgent>().Initialize(spawnPathfindingLocation.Location, _randomSeed);
        villager.GetComponent<Villager>().Village = this;

        villagers.Add(_villagerUUID, villager);
    }
    
    private IEnumerator HighlightPath(Vector2Int _currentLocation, Vector2Int _targetLocation) {
        List <Vector2Int> path = PathfindingManager.FindPath(_currentLocation, _targetLocation);
        if (path.Count <= 1) yield break;
        path.RemoveAt(0);

        for (int i = 0; i < path.Count; i++) {
            PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[i])].GetComponent<PathfindingLocationHighlight>().Highlight.SetActive(true);
            yield return new WaitForSeconds(delayBetweenTileHighlights);
        }

        yield return new WaitForSeconds(tileHighlightTime);

        for (int i = 0; i < path.Count; i++) {
            PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[i])].GetComponent<PathfindingLocationHighlight>().Highlight.SetActive(false);
            yield return new WaitForSeconds(delayBetweenTileHighlights);
        }
    }

    #endregion
}
