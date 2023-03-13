using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Structure Upgrade", menuName ="RBHK/Structures/Structure Upgrade")]
public class StructureUpgrade : ScriptableObject {
    [Tooltip("New Resource Entries for Structure, replaces old entries, modifiers are still applied.")]
    [SerializeField] private ResourceEntry[] resourceEntries;
    [Tooltip("Resources Required to Apply Upgrade.")]
    [SerializeField] private StructureUpgradeCost[] cost = new StructureUpgradeCost[5];

    public ResourceEntry[] ResourceEntries { get => resourceEntries; set => resourceEntries = value; }
    public StructureUpgradeCost[] Cost { get => cost; set => cost = value; }

    [System.Serializable]
    public struct StructureUpgradeCost {
        [SerializeField] private GameResources resource;
        [SerializeField] private float amount;

        public GameResources Resource { get => resource; set => resource = value; }
        public float Amount { get => amount; set => amount = value; }
    }
}
