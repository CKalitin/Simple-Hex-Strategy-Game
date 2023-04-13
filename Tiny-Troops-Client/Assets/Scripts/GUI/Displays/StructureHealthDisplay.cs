using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureHealthDisplay : MonoBehaviour {
    [SerializeField] private Image healthBar;
    [Space]
    [SerializeField] private Health health;

    private void Update() {
        healthBar.fillAmount = health.CurrentHealth / health.MaxHealth;
    }
}
