using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [Space]
    [SerializeField] private bool destroyOnZeroHealth;

    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float CurrentHealth { get => currentHealth; set => currentHealth = value; }
    
    public void ChangeHealth(float _change) {
        currentHealth += _change;
        if (currentHealth <= 0 && destroyOnZeroHealth) Destroy(gameObject);
    }
    
    public void SetHealth(float _health) {
        currentHealth = _health;
        if (currentHealth <= 0 && destroyOnZeroHealth) Destroy(gameObject);
    }
    
    public void SetHealth(float _health, float _maxHealth) {
        currentHealth = _health;
        maxHealth = _maxHealth;
        if (currentHealth <= 0 && destroyOnZeroHealth) Destroy(gameObject);
    }
}
