using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : MonoBehaviour {
    [SerializeField] private int playerID;
    [Space]
    [SerializeField] private GameObject selectedIndicator;

    private ClientVillage village;

    public int PlayerID { get => playerID; set => playerID = value; }
    public ClientVillage Village { get => village; set => village = value; }
    public bool Selected { get => selectedIndicator.activeSelf; }

    private void Start() {
        if (selectedIndicator) selectedIndicator.SetActive(false);
    }

    void Update() {
        if (village == null) Destroy(gameObject);
    }

    private void OnEnable() {
        MatchManager.OnMatchStateChanged += OnMatchStateChanged;
    }

    private void OnDisable() {
        MatchManager.OnMatchStateChanged -= OnMatchStateChanged;
    }

    private void OnMatchStateChanged(MatchState _ms) {
        if (_ms == MatchState.Ended) {
            Destroy(gameObject);
        }
    }
    
    public void ToggleSelectedIndicator(bool _toggle) {
        selectedIndicator.SetActive(_toggle);
    }
}
