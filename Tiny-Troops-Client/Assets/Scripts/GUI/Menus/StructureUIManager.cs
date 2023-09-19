using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USNL;

public class StructureUIManager : MonoBehaviour {
    #region Variables

    public static StructureUIManager instance;

    [SerializeField] private GameObject structureUIBackground;
    [Space]
    [SerializeField] private GameObject buildUIParent;

    private Dictionary<int, StructureUI> structureUIs = new Dictionary<int, StructureUI>();

    private ClientUnitSpawner clientUnitSpawner;
    private Health structureHealth;
    private Tile tile;

    public ClientUnitSpawner ClientUnitSpawner { get => clientUnitSpawner; set => clientUnitSpawner = value; }
    public Health StructureHealth { get => structureHealth; set => structureHealth = value; }
    public Tile Tile { get => tile; set => tile = value; }

    #endregion

    #region Core

    private void Awake() {
        Singleton();
        
        // Loop through children and check if they have a structureUI component. If they do, add them to the dictionary
        for (int i = 0; i < transform.childCount; i++) {
            StructureUI structureUI = transform.GetChild(i).GetComponent<StructureUI>();
            if (structureUI != null) structureUIs.Add((int)structureUI.StructureID, structureUI);
        }
    }
    
    private void Singleton() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Tile Selector instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnMatchUpdatePacket += OnMatchUpdatePacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnMatchUpdatePacket -= OnMatchUpdatePacket;
    }

    #endregion

    #region Structure UIs

    public bool ActivateStructureUI(int _structureID) {
        if (!structureUIs.ContainsKey(_structureID)) {
            return false;
        }

        // Activate the structure UI
        structureUIBackground.SetActive(true);
        buildUIParent.SetActive(false);
        structureUIs[_structureID].gameObject.SetActive(true);
        structureUIs[_structureID].ClientUnitSpawner = clientUnitSpawner;
        structureUIs[_structureID].StructureHealth = structureHealth;
        return true;
    }

    public void DeactivateStructureUIs() {
        structureUIBackground.SetActive(false);
        buildUIParent.SetActive(true);
        foreach (KeyValuePair<int, StructureUI> structureUI in structureUIs)
            structureUI.Value.gameObject.SetActive(false);
    }

    private void OnMatchUpdatePacket(object _packetObject) {
        MatchUpdatePacket packet = (MatchUpdatePacket)_packetObject;
        if ((MatchState)packet.MatchState == MatchState.Ended) DeactivateStructureUIs();
    }

    #endregion
}
