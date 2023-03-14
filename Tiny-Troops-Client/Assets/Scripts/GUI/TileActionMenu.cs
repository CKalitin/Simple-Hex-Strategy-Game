using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionMenu : MonoBehaviour {
    [SerializeField] private GameObject togglableUI;
    
    public bool Active { get => togglableUI.activeSelf; }

    public void ToggleActive(bool _active) {
        if (!_active) TileSelector.instance.CurrentTile = null;
        togglableUI.SetActive(_active);
    }

    public void OnCloseButton() {
        ToggleActive(false);
    }
}
