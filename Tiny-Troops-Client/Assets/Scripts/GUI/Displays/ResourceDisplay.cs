using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceDisplay : MonoBehaviour {
    [SerializeField] private GameResource resource;
    [Space]
    [SerializeField] private TextMeshProUGUI resourceSupplyText;
    [SerializeField] private TextMeshProUGUI resourceDemandText;
    [Space]
    [Tooltip("Offset per character in the demand text. Used to calculate the position of the demand text.")]
    [SerializeField] private float demandXOffsetPerCharacter = 30f;
    [Tooltip("Default number of characters in editor. Used to calculate the position of the demand text.")]
    [SerializeField] private int defaultSupplyCharactersLength = 3;

    private float demandTextOriginalX;

    private Resource resourceReference;

    private void Start() {
        if (resourceDemandText) demandTextOriginalX = resourceDemandText.transform.localPosition.x;
    }

    private void Update() {
        if (resourceReference == null) resourceReference = ResourceManager.instances[MatchManager.instance.PlayerID].GetResource(resource);
        
        if (resourceSupplyText) resourceSupplyText.text = Mathf.FloorToInt(resourceReference.Supply).ToString();

        if (resourceDemandText) {
            if (resourceReference.Demand >= 0) resourceDemandText.text = "+" + Mathf.FloorToInt(resourceReference.Demand).ToString();
            else resourceDemandText.text = "-" + Mathf.FloorToInt(resourceReference.Demand).ToString();
            float xOffset = (Mathf.FloorToInt(resourceReference.Supply).ToString().Length - defaultSupplyCharactersLength) * demandXOffsetPerCharacter;
            resourceDemandText.transform.localPosition = new Vector2(demandTextOriginalX + xOffset, resourceDemandText.transform.localPosition.y);
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
