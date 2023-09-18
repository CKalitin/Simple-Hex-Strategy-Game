using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaturalStructure : MonoBehaviour {
    [SerializeField] private ResourceGain[] resources;

    Tile tile;

    public ResourceGain[] Resources { get => resources; set => resources = value; }

    [Serializable]
    public struct ResourceGain {
        [SerializeField] private GameResource resource;
        [SerializeField] private int amount;

        public GameResource Resource { get => resource; set => resource = value; }
        public int Amount { get => amount; set => amount = value; }
    }

    private void Start() {
        tile = GameUtils.GetTileParent(transform);
    }

    private void OnDestroy() {
        List<int> playerIDs = new List<int>();

        // Get playerIDs of villagers on this tile
        foreach (KeyValuePair<int, List<Villager>> v in VillagerManager.instance.Villagers) {
            for (int i = 0; i < v.Value.Count; i++) {
                if (v.Value[i].Location == tile.TileInfo.Location) {
                    playerIDs.Add(v.Key);
                    break;
                }
            }
        }
        
        foreach (int playerID in playerIDs) {
            for (int i = 0; i < resources.Length; i++) {
                ResourceManager.instances[playerID].ChangeResource(resources[i].Resource, resources[i].Amount);
                ResourceEntry re = ScriptableObject.CreateInstance<ResourceEntry>();
            }
        }
    }
}
