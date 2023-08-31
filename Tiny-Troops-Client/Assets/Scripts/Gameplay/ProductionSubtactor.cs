using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProductionSubtactor : MonoBehaviour {
    private void Update() {
        GetStructuresSortedByDistanceToVillage(0);
    }

    private List<Structure> GetStructuresSortedByDistanceToVillage(int _playerID) {
        List<Structure> structures = new List<Structure>();
        List<Vector2Int> villageLocations = new List<Vector2Int>();

        foreach (var structure in StructureManager.instance.GameplayStructures) {
            Structure s = structure.Value[0].GetComponent<Structure>();
            if (s.PlayerID != _playerID) continue;
            if (s.GetComponent<ConstructionStructure>()) continue;
            if (s.GetComponent<ClientVillage>()) villageLocations.Add(s.Tile.Location);
            else structures.Add(s);
        }

        Dictionary<Structure, float> distancesToVillage = new Dictionary<Structure, float>();
        for (int i = 0; i < structures.Count; i++) {
            float lowestDist = 9999f;
            for (int x = 0; x < villageLocations.Count; x++) {
                float dist = Vector2Int.Distance(structures[i].Tile.Location, villageLocations[x]);
                if (dist < lowestDist) lowestDist = dist;
            }
            distancesToVillage.Add(structures[i], lowestDist);
        }

        // They are sorted twice to ensure the Client and Server get the same result, it is possible for multiple structures to have the same dist to village
        structures = structures.OrderBy(d => d.Tile.Location.x).ToList(); // Sort structures by x location
        structures = structures.OrderBy(d => distancesToVillage[d]).ToList(); // Sort structures by distance to village

        return structures;
    }
}
