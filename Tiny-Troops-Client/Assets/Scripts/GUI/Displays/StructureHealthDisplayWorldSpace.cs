using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureHealthDisplayWorldSpace : MonoBehaviour {
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject togglableParent;

    private Tile tile;
    private Health health;

    private void Start() {
        tile = GameUtils.GetTileParent(transform);
        togglableParent.SetActive(false);
    }

    private void Update() {
        if (tile.Structures.Count <= 0) return;
        if (health == null && (health = tile.Structures[0].GetComponent<Health>()) == null) return;

        healthBar.fillAmount = health.CurrentHealth / health.MaxHealth;
        
        if (healthBar.fillAmount >= 1) togglableParent.SetActive(false);
        else togglableParent.SetActive(true);
    }
}
