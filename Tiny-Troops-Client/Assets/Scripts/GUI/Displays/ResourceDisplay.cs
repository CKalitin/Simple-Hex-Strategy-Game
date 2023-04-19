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
        if (resourceReference == null) resourceReference = ResourceManager.instances[MatchManager.instance.PlayerID].GetResource(resource);
        
        if (resourceSupplyText) resourceSupplyText.text = resourceReference.Supply.ToString();

        if (resourceDemandText) {
            if (resourceReference.Demand >= 0) resourceDemandText.text = "+" + resourceReference.Demand.ToString();
            else resourceDemandText.text = "-" + resourceReference.Demand.ToString();
        }
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnConnected += OnConnected;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnConnected += OnConnected;
    }

    private void OnConnected(object _object) {
        MatchManager.instance.SetPlayerID();
        resourceReference = ResourceManager.instances[MatchManager.instance.PlayerID].GetResource(resource);
    }
}
