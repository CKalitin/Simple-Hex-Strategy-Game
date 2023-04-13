using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureHealthDisplay : MonoBehaviour {
    [SerializeField] private Image healthBar;
    
    private Health health;
    
    private void Start() {
        Transform t = transform.parent;
        while (true) {
            if (t.GetComponent<Health>()) {
                // If tile script found
                health = t.GetComponent<Health>();
                break;
            } else {
                // If we're at the top of the heirarchy, break out of the loop.
                if (t.parent == null) break;
                t = t.parent;
            }
        }
    }

    private void Update() {
        healthBar.fillAmount = health.CurrentHealth / health.MaxHealth;
    }
}
