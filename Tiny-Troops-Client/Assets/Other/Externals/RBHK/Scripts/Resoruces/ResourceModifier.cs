using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Resource Modifier", menuName ="RBHK/Resources/Resource Modifier")]
public class ResourceModifier : ScriptableObject {
    [Tooltip("This is the Resource Type the Modifier effects.")]
    [SerializeField] private GameResource resourceIdTarget;
    [Tooltip("This is the Resource Entry Type the Modifier effects.")]
    [SerializeField] private GameResourceEntry resourceEntryIdTarget;
    [Space]
    [Tooltip("This is the amount the resource entry is changed by.")]
    [SerializeField] private float change;
    [Tooltip("The corresponding Resource Entry is changed by this percentage. The Resource Entry is multiplied by this so eg. 5 * 1 = 5, the base value has to be 1")]
    [SerializeField] private float percentageChange = 1;

    private int targetPlayerID = -1;

    public GameResource ResourceIdTarget { get => resourceIdTarget; set => resourceIdTarget = value; }
    public GameResourceEntry ResourceEntryIdTarget { get => resourceEntryIdTarget; set => resourceEntryIdTarget = value; }
    public float Change { get => change; set => change = value; }
    public float PercentageChange { get => percentageChange; set => percentageChange = value; }
    public int TargetPlayerID { get => targetPlayerID; set => targetPlayerID = value; }
}
