using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureUI : MonoBehaviour {
    [SerializeField] private StructureID structureID;

    private ClientUnitSpawner clientUnitSpawner;
    private Health structureHealth;

    public StructureID StructureID { get => structureID; set => structureID = value; }
    public ClientUnitSpawner ClientUnitSpawner { get => clientUnitSpawner; set => clientUnitSpawner = value; }
    public Health StructureHealth { get => structureHealth; set => structureHealth = value; }
}
