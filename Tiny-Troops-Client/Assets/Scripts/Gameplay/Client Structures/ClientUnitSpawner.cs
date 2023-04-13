using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientUnitSpawner : MonoBehaviour {
    [Header("References")]
    [SerializeField] private UnitSpawner unitSpawner;
    
    List<int> unitSpawnQueue = new List<int>();
    
    private float totalDeltaTime;
    private float unitTrainingPercentage = 0f;

    public float UnitTrainingPercentage { get => unitTrainingPercentage; set => unitTrainingPercentage = value; }
    public List<int> UnitSpawnQueue { get => unitSpawnQueue; set => unitSpawnQueue = value; }

    private void Update() {
        if (unitSpawnQueue.Count > 0) {
            totalDeltaTime += Time.deltaTime;
            unitTrainingPercentage = totalDeltaTime / UnitManager.instance.UnitPrefabs[unitSpawnQueue[0]].GetComponent<Unit>().TrainingTime;

            if (totalDeltaTime >= UnitManager.instance.UnitPrefabs[unitSpawnQueue[0]].GetComponent<Unit>().TrainingTime) {
                unitSpawnQueue.RemoveAt(0);
                totalDeltaTime = 0f;
                unitTrainingPercentage = 0f;
            }
        }
    }

    public void AddUnitToQuene(int _unitID) {
        unitSpawnQueue.Add(_unitID);
        USNL.PacketSend.StructureAction(_unitID + 1000, unitSpawner.Location, new int[] {});
    }
}
