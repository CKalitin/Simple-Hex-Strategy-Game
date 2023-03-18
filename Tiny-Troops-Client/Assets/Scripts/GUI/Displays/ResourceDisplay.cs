using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceDisplay : MonoBehaviour {
    [SerializeField] private GameResource resource;
    [Space]
    [SerializeField] private TextMeshProUGUI resourceSupplyText;
    [SerializeField] private TextMeshProUGUI resourceDemandText;

    private Resource resourceReference;

    private void Update() {
        resourceSupplyText.text = resourceReference.Supply.ToString();
        resourceDemandText.text = resourceReference.Demand.ToString();
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnConnected += OnConnected;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnConnected += OnConnected;
    }

    private void OnConnected(object _object) {
        resourceReference = ResourceManager.instances[MatchManager.instance.PlayerID].GetResource(resource);
    }
}
