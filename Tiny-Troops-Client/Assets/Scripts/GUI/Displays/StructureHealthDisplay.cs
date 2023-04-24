using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureHealthDisplay : MonoBehaviour {
    [SerializeField] private Image healthBar;
    [Space]
    [SerializeField] private StructureUIManager structureUIManager;

    private Health health;

    private void Update() {
        if (health == null) health = structureUIManager.StructureHealth;
        healthBar.fillAmount = health.CurrentHealth / health.MaxHealth;
    }

    private void OnDisable() {
        health = null;
    }
}
