using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrentUnitTrainingDisplay : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Image unitTrainingPercentage;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [Space]
    [SerializeField] private StructureUI structureUI;

    private ClientUnitSpawner clientUnitSpawner;

    private void Update() {
        if (clientUnitSpawner == null) clientUnitSpawner = structureUI.ClientUnitSpawner;

        if (clientUnitSpawner.UnitSpawnQueue.Count > 0) {
            unitTrainingPercentage.fillAmount = clientUnitSpawner.UnitTrainingPercentage;
            string unitNameString = UnitManager.instance.UnitPrefabs[clientUnitSpawner.UnitSpawnQueue[0]].GetComponent<Unit>().UnitDisplayName;
            if (clientUnitSpawner.UnitSpawnQueue.Count > 1) unitNameString += " +" + (clientUnitSpawner.UnitSpawnQueue.Count - 1);
            unitNameText.text = unitNameString;
        } else {
            unitTrainingPercentage.fillAmount = 0;
            unitNameText.text = "";
        }
    }

    private void OnDisable() {
        clientUnitSpawner = null;
    }
}
