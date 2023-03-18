using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionMenu : MonoBehaviour {
    [SerializeField] private GameObject togglableUI;
    [Space]
    [SerializeField] private Tile tile;
    [Space]
    [SerializeField] private float uiScale = 1;

    public bool Active { get => togglableUI.activeSelf; }

    private void Awake() {
        if (tile == null)
            tile = transform.parent.parent.GetComponent<Tile>();
    }

    private void Update() {
        if (Active) {
            float dist = Vector2.Distance(new Vector2(togglableUI.transform.position.y, togglableUI.transform.position.z), new Vector2(Camera.main.transform.position.y, Camera.main.transform.position.z));
            togglableUI.transform.localScale = Vector3.one * dist * uiScale;
        }
    }

    public void ToggleActive(bool _active) {
        if (!_active) TileSelector.instance.CurrentTile = null;
        
        if (tile.Structures.Count <= 0) togglableUI.SetActive(_active);
        if (tile.Structures.Count > 0 && tile.Structures[0].GetComponent<GameplayStructure>().StructureUI != null) {
            tile.Structures[0].GetComponent<GameplayStructure>().TileActionMenu = this;
            tile.Structures[0].GetComponent<GameplayStructure>().StructureUI.SetActive(_active);
        }
        else togglableUI.SetActive(_active);
    }
    
    public void OnCloseButton() {
        ToggleActive(false);
    }
}
