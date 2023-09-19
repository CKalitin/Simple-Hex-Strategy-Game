using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHeldDisplay : MonoBehaviour {
    [SerializeField] private KeyCode key;
    [Space]
    [SerializeField] private GameObject displayParent;

    private void Update() {
        if (Input.GetKey(key)) displayParent.SetActive(true);
        else displayParent.SetActive(false);
    }
}
