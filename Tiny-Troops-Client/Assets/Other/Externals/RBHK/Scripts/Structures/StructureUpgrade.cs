using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Structure Upgrade", menuName ="RBHK/Structures/Structure Upgrade")]
public class StructureUpgrade : ScriptableObject {
    [Tooltip("New Resource Entries for Structure, replaces old entries, modifiers are still applied.")]
    [SerializeField] private ResourceEntry[] resourceEntries;
    [Tooltip("Resources Required to Apply Upgrade.")]
    [SerializeField] private RBHKCost[] cost = new RBHKCost[5];

    public ResourceEntry[] ResourceEntries { get => resourceEntries; set => resourceEntries = value; }
    public RBHKCost[] Cost { get => cost; set => cost = value; }
}
