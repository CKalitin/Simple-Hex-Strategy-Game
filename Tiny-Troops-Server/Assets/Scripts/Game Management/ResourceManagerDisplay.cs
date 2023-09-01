using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManagerDisplay : MonoBehaviour {
    [SerializeField] List<ResourceEntryDisplay> resourceEntries = new List<ResourceEntryDisplay>();

    [SerializeField] List<GameResourceDisplay> resources = new List<GameResourceDisplay>();

    ResourceManager rm;

    [Serializable]
    private struct ResourceEntryDisplay {
        [Tooltip("The resource that will be changed by this entry.")]
        [SerializeField] private GameResource resourceId;
        [Tooltip("The Resource Entry type, this is used by resource modifiers")]
        [SerializeField] private GameResourceEntry[] resourceEntryIds;
        [Space]
        [SerializeField] private float change;
        [Tooltip("If Change On Tick is enabled then the resource will be changed every tick, if not it will be applied once.")]
        [SerializeField] private bool changeOnTick;

        public ResourceEntryDisplay(GameResource resourceId, GameResourceEntry[] resourceEntryIds, float change, bool changeOnTick) {
            this.resourceId = resourceId;
            this.resourceEntryIds = resourceEntryIds;
            this.change = change;
            this.changeOnTick = changeOnTick;
        }

        public GameResource ResourceId { get => resourceId; set => resourceId = value; }
        public GameResourceEntry[] ResourceEntryIds { get => resourceEntryIds; set => resourceEntryIds = value; }
        public float Change { get => change; set => change = value; }
        public bool ChangeOnTick { get => changeOnTick; set => changeOnTick = value; }
    }

    [Serializable] struct GameResourceDisplay {
        [SerializeField] private GameResource resourceId;
        [SerializeField] private ResourceInfo resourceInfo;
        [Tooltip("This variable is used by the Resource Management script. It is only Serialize Field so you can see it. If you make changes to it before runtime they will not be used.")]
        [SerializeField] private float supply;
        [Tooltip("This variable is used by the Resource Management script. It is only Serialize Field so you can see it. If you make changes to it before runtime they will not be used.")]
        [SerializeField] private float demand;
        [Space]
        [Tooltip("Leave as 0 to use standard Resource Manager tick time.")]
        [SerializeField] private float customTickTime;
        [Space]
        [Tooltip("Change On Tick Resources are updated in real time. Eg. +5 Wood per tick. If this is false the resource is updated when new resource entries are added to Resource Manager. Eg. +10 population when placing a village.")]
        [SerializeField] private bool changeOnTickResource;

        public GameResourceDisplay(GameResource resourceId, ResourceInfo resourceInfo, float supply, float demand, float customTickTime, bool changeOnTickResource) {
            this.resourceId = resourceId;
            this.resourceInfo = resourceInfo;
            this.supply = supply;
            this.demand = demand;
            this.customTickTime = customTickTime;
            this.changeOnTickResource = changeOnTickResource;
        }

        public GameResource ResourceId { get => resourceId; set => resourceId = value; }
        public ResourceInfo ResourceInfo { get => resourceInfo; set => resourceInfo = value; }
        public float Supply { get => supply; set => supply = value; }
        public float Demand { get => demand; set => demand = value; }
        public float CustomTickTime { get => customTickTime; set => customTickTime = value; }
        public bool ChangeOnTickResource { get => changeOnTickResource; set => changeOnTickResource = value; }
    }

    private void Awake() {
        rm = GetComponent<ResourceManager>();
    }

    private void Update() {
        if (!Application.isEditor) return;

        resourceEntries.Clear();
        for (int i = 0; i < rm.ResourceEntries.Count; i++) {
            resourceEntries.Add(new ResourceEntryDisplay(rm.ResourceEntries[i].ResourceId, rm.ResourceEntries[i].ResourceEntryIds, rm.ResourceEntries[i].Change, rm.ResourceEntries[i].ChangeOnTick));
        }

        // Populate resources list
        resources.Clear();
        for (int i = 0; i < rm.Resources.Length; i++) {
            resources.Add(new GameResourceDisplay(rm.Resources[i].ResourceId, rm.Resources[i].ResourceInfo, rm.Resources[i].Supply, rm.Resources[i].Demand, rm.Resources[i].CustomTickTime, rm.Resources[i].ChangeOnTickResource));
        }
    }
}
