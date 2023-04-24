using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainUnitButton : MonoBehaviour {
    [Header("Spawning")]
    [SerializeField] private int unitID;
    [Tooltip("This is here only for display purposes")]
    [SerializeField] private GameObject unitPrefab;
    
    [Header("References")]
    [SerializeField] private StructureUI structureUI;
    [SerializeField] private GameObject cantAffordDisplayParent;

    private ClientUnitSpawner clientUnitSpawner;

    private void Update() {
        if (cantAffordDisplayParent == null) return;

        if (CanAffordCosts(MatchManager.instance.PlayerID, UnitManager.instance.UnitPrefabs[unitID].GetComponent<Unit>().UnitCost)) {
            cantAffordDisplayParent.SetActive(false);
        } else {
            cantAffordDisplayParent.SetActive(true);
        }
    }

    public void OnButtonDown() {
        if (clientUnitSpawner == null) clientUnitSpawner = structureUI.ClientUnitSpawner;

        if (CanAffordCosts(MatchManager.instance.PlayerID, UnitManager.instance.UnitPrefabs[unitID].GetComponent<Unit>().UnitCost))
            clientUnitSpawner.AddUnitToQuene(unitID);
    }
    
    public bool CanAffordCosts(int _playerID, RBHKCost[] costs) {
        bool output = true;
        for (int i = 0; i < costs.Length; i++) {
            if (costs[i].Amount > ResourceManager.instances[_playerID].GetResource(costs[i].Resource).Supply)
                output = false;
        }

        return output;
    }

    private void OnDisable() {
        clientUnitSpawner = null;
    }
}
