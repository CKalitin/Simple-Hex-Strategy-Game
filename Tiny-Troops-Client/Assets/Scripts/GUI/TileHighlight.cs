using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHighlight : MonoBehaviour {
    [SerializeField] private GameObject[] outlines;

    public void ToggleHighlight(bool _toggle) {
        for (int i = 0; i < outlines.Length; i++) outlines[i].SetActive(_toggle);
    }
}
