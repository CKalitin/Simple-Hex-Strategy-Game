using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerResourceManager : MonoBehaviour {
    public static ServerResourceManager instance;

    [SerializeField] private Resource[] resources;

    private void Awake() {
        Singleton();
        InstantiateNewLocalResources();
    }

    private void Singleton() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Game Controller instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnResourcesPacket += OnResourcesPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnResourcesPacket += OnResourcesPacket;
    }

    private void OnResourcesPacket(object _packetObject) {
        USNL.ResourcesPacket resourcesPacket = (USNL.ResourcesPacket)_packetObject;
        if (resourcesPacket.PlayerID != MatchManager.instance.PlayerID) return;

        // Copy packet in resources
        for (int i = 0; i < resources.Length; i++) {
            resources[i].Supply = resourcesPacket.Supplys[i];
            resources[i].Demand = resourcesPacket.Demands[i];
        }
    }

    // Can't use the Scriptable Objects because those are shared between all instances of the ResourceManager
    private void InstantiateNewLocalResources() {
        // Create list for updated resource entries
        Resource[] _resources = new Resource[resources.Length];

        // Loop through resources
        for (int i = 0; i < resources.Length; i++) {
            Resource newResource = ScriptableObject.CreateInstance<Resource>(); // Create new resource entry

            // Set values of new resource  entry
            newResource.ResourceId = resources[i].ResourceId;
            newResource.ResourceInfo = resources[i].ResourceInfo;
            newResource.Supply = resources[i].Supply;
            newResource.Demand = resources[i].Demand;
            newResource.CustomTickTime = resources[i].CustomTickTime;

            _resources[i] = newResource;
        }

        resources = _resources;
    }

    public Resource GetResource(GameResource _resourceID) {
        // Loop through resources and find resource that matches parameter id
        for (int i = 0; i < resources.Length; i++) {
            if (resources[i].ResourceId == _resourceID)
                return resources[i];
        }

        // If no resources found, return Null
        Debug.LogWarning($"Resource of ID: {_resourceID} cannot be found.");
        return null;
    }
}
