using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Resource Entry", menuName = "RBHK/Resources/Resource Entry")]
public class ResourceEntry : ScriptableObject {
    // Unlike MonoBehaviours all variables are public by default in Scriptable Objects, the more u know
    
    [Tooltip("The resource that will be changed by this entry.")]
    [SerializeField] private GameResources resourceId;
    [Tooltip("The Resource Entry type, this is used by resource modifiers")]
    [SerializeField] private ResourceEntries[] resourceEntryIds;
    [Space]
    [SerializeField] private float change;
    [Tooltip("If Change On Tick is enabled then the resource will be changed every tick, if not it will be applied once.")]
    [SerializeField] private bool changeOnTick;

    public GameResources ResourceId { get => resourceId; set => resourceId = value; }
    public ResourceEntries[] ResourceEntryIds { get => resourceEntryIds; set => resourceEntryIds = value; }
    public float Change { get => change; set => change = value; }
    public bool ChangeOnTick { get => changeOnTick; set => changeOnTick = value; }
}
