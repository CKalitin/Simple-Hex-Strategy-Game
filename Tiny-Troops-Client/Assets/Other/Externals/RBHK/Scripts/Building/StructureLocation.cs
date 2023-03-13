using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLocation : MonoBehaviour {
    [SerializeField] private int structureSize;
    [Space]
    [SerializeField] private GameObject assignedStructure;

    public int StructureSize { get => structureSize; set => structureSize = value; }
    public GameObject AssignedStructure { get => assignedStructure; set => assignedStructure = value; }
}
