using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : MonoBehaviour {
    private ClientVillage village;

    public ClientVillage Village { get => village; set => village = value; }

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
}
