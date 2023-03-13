using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Structure Build Info", menuName = "RBHK/Structures/Structure Build Info")]
public class StructureBuildInfo : ScriptableObject {
    [SerializeField] private int structureSize;
    [Space]
    [SerializeField] private GameObject structurePrefab;
    [Tooltip("This is used to display the position of a structure before it is placed.\nCould just be a semi-transparent model of the structure.")]
    [SerializeField] private GameObject structureDisplayPrefab;
    [Tooltip("Resources Required to Build Structure.")]
    [SerializeField] private StructureUpgradeCost[] cost = new StructureUpgradeCost[5];

    public int StructureSize { get => structureSize; set => structureSize = value; }
    public GameObject StructurePrefab { get => structurePrefab; set => structurePrefab = value; }
    public GameObject StructureDisplayPrefab { get => structureDisplayPrefab; set => structureDisplayPrefab = value; }
    public StructureUpgradeCost[] Cost { get => cost; set => cost = value; }

    [System.Serializable]
    public struct StructureUpgradeCost {
        [SerializeField] private GameResources resource;
        [SerializeField] private float amount;

        public GameResources Resource { get => resource; set => resource = value; }
        public float Amount { get => amount; set => amount = value; }
    }
}
