using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Resource", menuName = "RBHK/Resources/Resource")]
public class Resource : ScriptableObject {
    [SerializeField] private GameResource resourceId;
    [SerializeField] private ResourceInfo resourceInfo;
    [Tooltip("This variable is used by the Resource Management script. It is only Serialize Field so you can see it. If you make changes to it before runtime they will not be used.")]
    private float supply;
    [Tooltip("This variable is used by the Resource Management script. It is only Serialize Field so you can see it. If you make changes to it before runtime they will not be used.")]
    private float demand;
    [Space]
    [Tooltip("Leave as 0 to use standard Resource Manager tick time.")]
    [SerializeField] private float customTickTime;
    [Space]
    [Tooltip("Change On Tick Resources are updated in real time. Eg. +5 Wood per tick. If this is false the resource is updated when new resource entries are added to Resource Manager. Eg. +10 population when placing a village.")]
    [SerializeField] private bool changeOnTickResource;

    public GameResource ResourceId { get => resourceId; set => resourceId = value; }
    public ResourceInfo ResourceInfo { get => resourceInfo; set => resourceInfo = value; }
    public float Supply { get => supply; set => supply = value; }
    public float Demand { get => demand; set => demand = value; }
    public float CustomTickTime { get => customTickTime; set => customTickTime = value; }
    public bool ChangeOnTickResource { get => changeOnTickResource; set => changeOnTickResource = value; }

    public Resource(float _supply, float _demand) {
        supply = _supply;
        demand = _demand;
    }

    public Resource() {
        supply = 0;
        demand = 0;
    }
}
