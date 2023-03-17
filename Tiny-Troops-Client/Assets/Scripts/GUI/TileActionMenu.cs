using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionMenu : MonoBehaviour {
    [SerializeField] private GameObject togglableUI;
    [Space]
    [SerializeField] private float scale = 1;

    public bool Active { get => togglableUI.activeSelf; }
    
    private void Update() {
        if (Active) {
            float dist = Vector2.Distance(new Vector2(togglableUI.transform.position.y, togglableUI.transform.position.z), new Vector2(Camera.main.transform.position.y, Camera.main.transform.position.z));
            togglableUI.transform.localScale = Vector3.one * dist * scale;
        }
    }

    public void ToggleActive(bool _active) {
        if (!_active) TileSelector.instance.CurrentTile = null;
        togglableUI.SetActive(_active);
    }
    
    public void OnCloseButton() {
        ToggleActive(false);
    }
}
