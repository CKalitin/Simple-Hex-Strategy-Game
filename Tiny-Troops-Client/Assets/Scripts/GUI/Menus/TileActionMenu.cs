using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionMenu : MonoBehaviour {
    [SerializeField] private GameObject togglableUI;
    [Space]
    [SerializeField] private Tile tile;

    public bool Active { get => togglableUI.activeSelf; }

    private void Awake() {
        if (tile == null)
            tile = transform.parent.parent.GetComponent<Tile>();
    }

    public void ToggleActive(bool _active) {
        if (!_active) TileSelector.instance.CurrentTile = null;
        
        if (tile.Structures.Count <= 0) togglableUI.SetActive(_active);
        if (tile.Structures.Count > 0 && tile.Structures[0].GetComponent<GameplayStructure>() && tile.Structures[0].GetComponent<GameplayStructure>().StructureUI != null) {
            tile.Structures[0].GetComponent<GameplayStructure>().SetTileActionMenu(this);
            tile.Structures[0].GetComponent<GameplayStructure>().StructureUI.SetActive(_active);
        }
        else togglableUI.SetActive(_active);
    }
    
    public void OnCloseButton() {
        ToggleActive(false);
    }
}
