using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGameObjectButton : MonoBehaviour {
    [SerializeField] private GameObject toggleMe;

    public void OnButtonPressed() {
        toggleMe.SetActive(!toggleMe.activeSelf);
    }
}
