using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ProductionSubtractor : MonoBehaviour {
    #region Variables

    public static Dictionary<int, ProductionSubtractor> instances;

    [SerializeField] private int playerID = 0;

    private float previousPopulation = 99f;

    private Dictionary<Structure, float> disabledStructures = new Dictionary<Structure, float>();

    public Dictionary<Structure, float> DisabledStructures { get => disabledStructures; set => disabledStructures = value; }

    #endregion

    #region Core

    private void Awake() {
        if (instances == null) instances = new Dictionary<int, ProductionSubtractor>();
        if (!instances.ContainsKey(playerID)) instances.Add(playerID, this);
        else Debug.Log("ProductionSubtractor already exists for player " + playerID + ".");
    }

    private void Update() {
        if (!USNL.ServerManager.GetConnectedClientIds().Contains(playerID)) return;

        if (previousPopulation != ResourceManager.instances[playerID].GetResource(GameResource.Population).Supply) {
            float pop = ResourceManager.instances[playerID].GetResource(GameResource.Population).Supply;
            float previousDefecit = GetCurrentAppliedDefecit();
            if (pop < previousPopulation && pop < 0) {
                DisableStructures(Mathf.Abs(pop + previousDefecit));
            } else if (pop > previousPopulation) {
                EnableStructures(pop - previousPopulation);
            }
            previousPopulation = pop;
        }

        GetEnabledStructuresSortedByDistanceToVillage(0);
    }

    private void OnDestroy() {
        instances.Remove(playerID);
    }

    #endregion

    #region Functions

    private void DisableStructures(float _populationDefecit) {
        List<Structure> s = GetEnabledStructuresSortedByDistanceToVillage(playerID);

        for (int i = 0; i < s.Count; i++) {
            if (_populationDefecit > 0) {
                _populationDefecit -= Mathf.Abs(GetRequiredPopulation(s[i]));
                s[i].GetComponent<GameplayStructure>().ToggleProduction(false);
                disabledStructures.Add(s[i], s[i].GetComponent<GameplayStructure>().DistToNearestVillage);
            } else break;
        }
    }

    private void EnableStructures(float _populationIncrease) {
        List<Structure> removeStructures = new List<Structure>();
        List<Structure> disabledStructureList = disabledStructures.Keys.OrderBy(d => disabledStructures[d]).ToList();

        for (int i = 0; i < disabledStructureList.Count; i++) {
            if (_populationIncrease >= GetRequiredPopulation(disabledStructureList[i])) {
                _populationIncrease -= Mathf.Abs(GetRequiredPopulation(disabledStructureList[i]));
                removeStructures.Add(disabledStructureList[i]);
                disabledStructureList[i].GetComponent<GameplayStructure>().ToggleProduction(true);
            } else break;
        }

        for (int i = 0; i < removeStructures.Count; i++) disabledStructures.Remove(removeStructures[i]);
    }

    private List<Structure> GetEnabledStructuresSortedByDistanceToVillage(int _playerID) {
        List<Structure> structures = new List<Structure>();
        List<Vector2Int> villageLocations = new List<Vector2Int>();

        foreach (var structure in StructureManager.instance.GameplayStructures) {
            Structure s = structure.Value[0].GetComponent<Structure>();
            if (s.PlayerID != _playerID) continue;
            if (s.GetComponent<ConstructionStructure>()) continue;
            if (!s.GetComponent<GameplayStructure>().ProductionEnabled) continue;
            if (s.GetComponent<PlayerVillage>()) villageLocations.Add(s.Tile.Location);
            else structures.Add(s);
        }

        Dictionary<Structure, float> distancesToVillage = new Dictionary<Structure, float>();
        for (int i = 0; i < structures.Count; i++) {
            float lowestDist = 9999f;
            for (int x = 0; x < villageLocations.Count; x++) {
                float dist = Vector2Int.Distance(structures[i].Tile.Location, villageLocations[x]);
                if (dist < lowestDist) lowestDist = dist;
            }
            structures[i].GetComponent<GameplayStructure>().DistToNearestVillage = lowestDist;
            distancesToVillage.Add(structures[i], lowestDist);
        }

        // They are sorted twice to ensure the Client and Server get the same result, it is possible for multiple structures to have the same dist to village
        structures = structures.OrderBy(d => d.Tile.Location.x).ToList(); // Sort structures by x location
        structures = structures.OrderBy(d => distancesToVillage[d]).ToList(); // Sort structures by distance to village
        structures.Reverse();

        return structures;
    }

    #endregion

    #region Utils

    private float GetRequiredPopulation(Structure _s) {
        for (int i = 0; i < _s.StructureBuildInfo.Cost.Length; i++) {
            if (_s.StructureBuildInfo.Cost[i].Resource == GameResource.Population) return _s.StructureBuildInfo.Cost[i].Amount;
        }
        return 0;
    }

    private float GetCurrentAppliedDefecit() {
        float defecit = 0;
        foreach (var s in disabledStructures) defecit += Mathf.Abs(GetRequiredPopulation(s.Key));
        return defecit;
    }

    #endregion
}
