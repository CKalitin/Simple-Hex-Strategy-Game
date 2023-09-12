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
    
    private void Awake() {
        if (resourceDemandText) demandTextOriginalX = resourceDemandText.transform.localPosition.x;
    }

    private void UpdateText(float _supply, float _demand) {
        if (resourceSupplyText) resourceSupplyText.text = Mathf.FloorToInt(_supply).ToString();

        if (resourceDemandText) {
            if (_demand >= 0) resourceDemandText.text = "+" + Mathf.FloorToInt(_demand).ToString();
            else resourceDemandText.text = "-" + Mathf.FloorToInt(_demand).ToString();
            float xOffset = (Mathf.FloorToInt(_supply).ToString().Length - defaultSupplyCharactersLength) * demandXOffsetPerCharacter;
            resourceDemandText.transform.localPosition = new Vector2(demandTextOriginalX + xOffset, resourceDemandText.transform.localPosition.y);
        }
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnResourcesPacket += OnResourcesPacket;
    }

    private void OnDisable() {;
        USNL.CallbackEvents.OnResourcesPacket += OnResourcesPacket;
    }

    private void OnResourcesPacket(object _packetObject) {
        USNL.ResourcesPacket resourcesPacket = (USNL.ResourcesPacket)_packetObject;
        if (resourcesPacket.PlayerID != MatchManager.instance.PlayerID) return;

        for (int i = 0; i < ResourceManager.instances[0].Resources.Length; i++) {
            if (ResourceManager.instances[0].Resources[i].ResourceId != resource) continue;
            UpdateText(resourcesPacket.Supplys[i], resourcesPacket.Demands[i]);
        }
    }
}
